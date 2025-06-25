using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRICXI.Services
{
    public class CricketNewsService
    {
        private readonly HttpClient _client;
        private readonly ILogger<CricketNewsService> _logger;

        public CricketNewsService(IConfiguration config, ILogger<CricketNewsService> logger)
        {
            _logger = logger;
            _client = new HttpClient();

            _client.DefaultRequestHeaders.Add("x-apihub-key", config["CricketNewsApi:ApiKey"]);
            _client.DefaultRequestHeaders.Add("x-apihub-host", "Cricbuzz-Official-Cricket-API.allthingsdev.co");
            _client.DefaultRequestHeaders.Add("x-apihub-endpoint", "b02fb028-fcca-4590-bf04-d0cd0c331af4");
        }

        public async Task<List<NewsItem>> GetLatestNewsAsync()
        {
            var newsList = new List<NewsItem>();
            var requestUrl = "https://Cricbuzz-Official-Cricket-API.proxy-production.allthingsdev.co/news";

            try
            {
                var response = await _client.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("storyList", out var storyList))
                {
                    foreach (var wrapper in storyList.EnumerateArray())
                    {
                        if (wrapper.TryGetProperty("story", out var story))
                        {
                            var title = story.GetProperty("hline").GetString() ?? "";
                            var summary = story.TryGetProperty("intro", out var intro) ? intro.GetString() ?? "" : "";
                            var timestampStr = story.TryGetProperty("pubTime", out var pubTimeProp)
                                ? pubTimeProp.GetString()
                                : null;

                            var date = DateTime.UtcNow;
                            if (long.TryParse(timestampStr, out long millis))
                            {
                                date = DateTimeOffset.FromUnixTimeMilliseconds(millis).DateTime;
                            }

                            newsList.Add(new NewsItem
                            {
                                Title = title,
                                Summary = summary,
                                Date = date
                            });
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("No storyList found in API response.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch or parse news data.");
            }

            return newsList;
        }
    }

    public class NewsItem
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public DateTime Date { get; set; }
    }
}

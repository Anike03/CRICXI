using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace CRICXI.Services
{
    public class CricbuzzApiService
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly string _host;

        public CricbuzzApiService(IConfiguration config)
        {
            _client = new HttpClient();
            _baseUrl = config["CricbuzzApi:BaseUrl"];
            _apiKey = config["CricbuzzApi:ApiKey"];
            _host = config["CricbuzzApi:Host"];

            _client.DefaultRequestHeaders.Add("x-rapidapi-key", _apiKey);
            _client.DefaultRequestHeaders.Add("x-rapidapi-host", _host);
        }

        public async Task<string> GetRecentMatchesAsync()
        {
            var requestUrl = $"{_baseUrl}/matches/v1/recent";
            var response = await _client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetUpcomingMatchesAsync()
        {
            var requestUrl = $"{_baseUrl}/matches/v1/upcoming";
            var response = await _client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // ✅ NEW: Live match score fetch method
        public async Task<string> GetLiveScoreAsync(string matchId)
        {
            var requestUrl = $"{_baseUrl}/mcenter/v1/{matchId}";
            var response = await _client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}

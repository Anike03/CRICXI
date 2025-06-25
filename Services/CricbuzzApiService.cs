using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace CRICXI.Services
{
    public class CricbuzzApiService
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly string _host;
        private readonly string _matchEndpoint;
        private readonly string _newsEndpoint;

        public CricbuzzApiService(IConfiguration config)
        {
            _client = new HttpClient();
            _baseUrl = config["CricbuzzApi:BaseUrl"];
            _apiKey = config["CricbuzzApi:ApiKey"];
            _host = config["CricbuzzApi:Host"];
            _matchEndpoint = config["CricbuzzApi:Endpoint"];
            _newsEndpoint = config["CricketNewsApi:Endpoint"];

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // ✅ Upcoming Matches (used for contests)
        public async Task<string> GetUpcomingMatchesAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/matches/upcoming");
            request.Headers.Add("x-apihub-key", _apiKey);
            request.Headers.Add("x-apihub-host", _host);
            request.Headers.Add("x-apihub-endpoint", _matchEndpoint);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // ✅ Recent Matches (used for results)
        public async Task<string> GetRecentMatchesAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/matches/recent");
            request.Headers.Add("x-apihub-key", _apiKey);
            request.Headers.Add("x-apihub-host", _host);
            request.Headers.Add("x-apihub-endpoint", _matchEndpoint);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // ✅ Live Score or Players
        public async Task<string> GetLiveScoreAsync(string matchId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/mcenter/v1/{matchId}");
            request.Headers.Add("x-apihub-key", _apiKey);
            request.Headers.Add("x-apihub-host", _host);
            request.Headers.Add("x-apihub-endpoint", _matchEndpoint);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // ✅ Alias for Player Data
        public async Task<string> GetPlayersByMatchAsync(string matchId)
        {
            return await GetLiveScoreAsync(matchId);
        }
    }
}

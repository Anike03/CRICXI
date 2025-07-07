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
            _baseUrl = config["CricbuzzApi:BaseUrl"] ?? throw new ArgumentNullException("BaseUrl");
            _apiKey = config["CricbuzzApi:ApiKey"] ?? throw new ArgumentNullException("ApiKey");
            _host = config["CricbuzzApi:Host"] ?? throw new ArgumentNullException("Host");
            _matchEndpoint = config["CricbuzzApi:Endpoint"] ?? throw new ArgumentNullException("Endpoint");
            _newsEndpoint = config["CricketNewsApi:Endpoint"] ?? throw new ArgumentNullException("NewsEndpoint");

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
        // Add these new methods to CricbuzzApiService.cs

        // Get detailed match info
        public async Task<string> GetMatchInfoAsync(string matchId)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_baseUrl}/match/{matchId}"
            );
            request.Headers.Add("x-apihub-key", _apiKey);
            request.Headers.Add("x-apihub-host", _host);
            request.Headers.Add("x-apihub-endpoint", "ac951751-d311-4d23-8f18-353e75432353"); // Match info endpoint

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // Get match squads
        public async Task<string> GetMatchSquadsAsync(string matchId)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_baseUrl}/match/{matchId}/squads"
            );
            request.Headers.Add("x-apihub-key", _apiKey);
            request.Headers.Add("x-apihub-host", _host);
            request.Headers.Add("x-apihub-endpoint", "be37c2f5-3a12-44bd-8d8b-ba779eb89279"); // Squads endpoint

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}

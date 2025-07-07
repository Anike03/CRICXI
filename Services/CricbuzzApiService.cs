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

        // Get team players
        public async Task<string> GetTeamPlayersAsync(int teamId)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_baseUrl}/team/{teamId}/players"
            );
            request.Headers.Add("x-apihub-key", _apiKey);
            request.Headers.Add("x-apihub-host", _host);
            request.Headers.Add("x-apihub-endpoint", "2b298a5d-fb51-4e29-aa15-c5385291fcd8"); // Team players endpoint

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // Get series squads
        public async Task<string> GetSeriesSquadsAsync(int seriesId)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_baseUrl}/series/{seriesId}/squads"
            );
            request.Headers.Add("x-apihub-key", _apiKey);
            request.Headers.Add("x-apihub-host", _host);
            request.Headers.Add("x-apihub-endpoint", "038d223b-aca5-4096-8eb1-184dd0c09513"); // Series squads endpoint

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // Get squad details for a specific squad in a series
        public async Task<string> GetSeriesSquadDetailsAsync(int seriesId, int squadId)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_baseUrl}/series/{seriesId}/squads/{squadId}"
            );
            request.Headers.Add("x-apihub-key", _apiKey);
            request.Headers.Add("x-apihub-host", _host);
            request.Headers.Add("x-apihub-endpoint", "c4b3ccd2-0bb1-4d94-98c9-b31f389480be"); // Series squad details endpoint

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
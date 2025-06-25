using Microsoft.AspNetCore.Mvc;
using CRICXI.Services;
using System.Text.Json;

namespace CRICXI.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserService _userService;
        private readonly CricbuzzApiService _cricbuzzService;
        private readonly CricketNewsService _newsService;

        public HomeController(UserService userService, CricbuzzApiService cricbuzzService, CricketNewsService newsService)
        {
            _userService = userService;
            _cricbuzzService = cricbuzzService;
            _newsService = newsService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetWalletBalance()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return Json("0");

            var user = await _userService.GetByUsername(username);
            return Json(user?.WalletBalance.ToString("0.00") ?? "0");
        }

        [HttpGet]
        public async Task<IActionResult> GetUpcomingMatches()
        {
            try
            {
                var json = await _cricbuzzService.GetUpcomingMatchesAsync();

                using var doc = JsonDocument.Parse(json);
                var matches = new List<object>();

                if (doc.RootElement.TryGetProperty("typeMatches", out var types))
                {
                    foreach (var type in types.EnumerateArray())
                    {
                        if (type.TryGetProperty("seriesMatches", out var seriesMatches))
                        {
                            foreach (var series in seriesMatches.EnumerateArray())
                            {
                                if (series.TryGetProperty("seriesAdWrapper", out var wrapper) &&
                                    wrapper.TryGetProperty("matches", out var matchArray))
                                {
                                    foreach (var m in matchArray.EnumerateArray())
                                    {
                                        if (m.TryGetProperty("matchInfo", out var info))
                                        {
                                            var team1 = info.GetProperty("team1").GetProperty("teamName").GetString() ?? "";
                                            var team2 = info.GetProperty("team2").GetProperty("teamName").GetString() ?? "";
                                            var matchDesc = info.GetProperty("matchDesc").GetString() ?? "";
                                            var startDateMillis = info.GetProperty("startDate").GetString() ?? "0";

                                            if (long.TryParse(startDateMillis, out var millis))
                                            {
                                                var date = DateTimeOffset.FromUnixTimeMilliseconds(millis).DateTime;
                                                matches.Add(new
                                                {
                                                    team1,
                                                    team2,
                                                    tournament = matchDesc,
                                                    venue = info.GetProperty("venueInfo").GetProperty("ground").GetString() ?? "",
                                                    date = date.ToString("dd/MM/yyyy")
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return Json(matches);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNews()
        {
            try
            {
                var news = await _newsService.GetLatestNewsAsync();

                if (news == null || !news.Any())
                {
                    return Json(new { error = "No news available" });
                }

                return Json(news.Select(n => new
                {
                    title = n.Title,
                    summary = n.Summary,
                    date = n.Date.ToString("dd MMM yyyy hh:mm tt")
                }));
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Controller News Error: {ex.Message}");
                return StatusCode(500, new { error = "News service unavailable" });
            }
        }
    }
}

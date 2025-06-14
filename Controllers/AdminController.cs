﻿using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CRICXI.Controllers
{
    [Route("Admin/[action]")]
    public class AdminController : Controller
    {
        private readonly MatchService _matchService;
        private readonly ContestService _contestService;
        private readonly UserService _userService;
        private readonly FantasyTeamService _teamService;
        private readonly CricbuzzApiService _cricbuzzApiService;
        private readonly PlayerService _playerService;

        public AdminController(
            MatchService matchService,
            ContestService contestService,
            UserService userService,
            FantasyTeamService teamService,
            CricbuzzApiService cricbuzzApiService,
            PlayerService playerService)
        {
            _matchService = matchService;
            _contestService = contestService;
            _userService = userService;
            _teamService = teamService;
            _cricbuzzApiService = cricbuzzApiService;
            _playerService = playerService;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // ✅ Admin Login
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (email == "aniketsharma9360@gmail.com" && password == "Aaniket#00")
            {
                HttpContext.Session.SetString("Username", "Admin");
                HttpContext.Session.SetString("Role", "Admin");
                return RedirectToAction("Dashboard");
            }
            ViewBag.Error = "Invalid Admin Credentials!";
            return View();
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return Unauthorized();
            return View();
        }

        // ✅ MATCHES MANAGEMENT
        [HttpGet]
        public async Task<IActionResult> SyncMatches()
        {
            if (!IsAdmin()) return Unauthorized();

            var json = await _cricbuzzApiService.GetUpcomingMatchesAsync();
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                if (doc.RootElement.TryGetProperty("typeMatches", out var typeMatches))
                {
                    foreach (var type in typeMatches.EnumerateArray())
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
                                        if (m.TryGetProperty("matchInfo", out var matchInfo))
                                        {
                                            var team1 = matchInfo.GetProperty("team1").GetProperty("teamName").GetString() ?? string.Empty;
                                            var team2 = matchInfo.GetProperty("team2").GetProperty("teamName").GetString() ?? string.Empty;
                                            var matchDesc = matchInfo.GetProperty("matchDesc").GetString() ?? string.Empty;
                                            var startDate = matchInfo.GetProperty("startDate").GetString() ?? string.Empty;
                                            var cricbuzzMatchId = matchInfo.GetProperty("matchId").ToString();

                                            var match = new Match
                                            {
                                                CricbuzzMatchId = cricbuzzMatchId,
                                                TeamA = team1,
                                                TeamB = team2,
                                                MatchDesc = matchDesc,
                                                StartDate = startDate,
                                                Status = "Upcoming"
                                            };

                                            await _matchService.SaveIfNotExists(match);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return RedirectToAction("AllMatches");
        }

        [HttpGet]
        public async Task<IActionResult> AllMatches()
        {
            if (!IsAdmin()) return Unauthorized();
            var matches = await _matchService.GetAll();
            return View(matches);
        }

        // ✅ CONTEST MANAGEMENT
        [HttpGet]
        public async Task<IActionResult> AllContests()
        {
            if (!IsAdmin()) return Unauthorized();
            var contests = await _contestService.GetAll();
            return View(contests);
        }

        [HttpGet]
        public async Task<IActionResult> CreateContest(string matchId)
        {
            if (!IsAdmin()) return Unauthorized();
            var match = await _matchService.GetById(matchId);
            ViewBag.Match = match;
            return View(new Contest { MatchId = matchId });
        }

        [HttpPost]
        public async Task<IActionResult> CreateContest(Contest contest)
        {
            if (!IsAdmin()) return Unauthorized();
            await _contestService.Create(contest);
            return RedirectToAction("AllContests");
        }

        // ✅ USERS MANAGEMENT
        [HttpGet]
        public async Task<IActionResult> AllUsers()
        {
            if (!IsAdmin()) return Unauthorized();
            var users = await _userService.GetAllUsers();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> RechargeUser(string userId)
        {
            if (!IsAdmin()) return Unauthorized();
            var user = await _userService.GetById(userId);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> RechargeUser(string userId, decimal amount)
        {
            if (!IsAdmin()) return Unauthorized();
            await _userService.UpdateWallet(userId, amount, addFunds: true);
            return RedirectToAction("AllUsers");
        }

        // ✅ NEW: Ban User
        [HttpPost]
        public async Task<IActionResult> BanUser(string userId)
        {
            if (!IsAdmin()) return Unauthorized();
            await _userService.BanUser(userId, 30);
            return RedirectToAction("AllUsers");
        }

        // ✅ NEW: Delete User
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (!IsAdmin()) return Unauthorized();
            await _userService.DeleteUser(userId);
            return RedirectToAction("AllUsers");
        }

        // ✅ FANTASY TEAMS MANAGEMENT
        [HttpGet]
        public async Task<IActionResult> AllFantasyTeams()
        {
            if (!IsAdmin()) return Unauthorized();
            var teams = await _teamService.GetAllTeams();
            return View(teams);
        }

        // ✅ PLAYER SYNC + ROLE MANAGEMENT
        [HttpGet]
        public async Task<IActionResult> SyncPlayers(string matchId)
        {
            if (!IsAdmin()) return Unauthorized();

            var json = await _cricbuzzApiService.GetPlayersByMatchAsync(matchId);
            var players = new List<Player>();

            using (var doc = JsonDocument.Parse(json))
            {
                var matchInfo = doc.RootElement.GetProperty("matchInfo");

                var team1 = matchInfo.GetProperty("team1");
                var team1Name = team1.GetProperty("teamName").GetString() ?? string.Empty;
                var team1Players = team1.GetProperty("players");

                foreach (var p in team1Players.EnumerateArray())
                {
                    players.Add(new Player
                    {
                        CricbuzzMatchId = matchId,
                        CricbuzzPlayerId = p.GetProperty("id").ToString(),
                        Name = p.GetProperty("name").GetString() ?? string.Empty,
                        Team = team1Name,
                        Role = ""
                    });
                }

                var team2 = matchInfo.GetProperty("team2");
                var team2Name = team2.GetProperty("teamName").GetString() ?? string.Empty;
                var team2Players = team2.GetProperty("players");

                foreach (var p in team2Players.EnumerateArray())
                {
                    players.Add(new Player
                    {
                        CricbuzzMatchId = matchId,
                        CricbuzzPlayerId = p.GetProperty("id").ToString(),
                        Name = p.GetProperty("name").GetString() ?? string.Empty,
                        Team = team2Name,
                        Role = ""
                    });
                }
            }

            await _playerService.SavePlayers(players);
            return RedirectToAction("ManagePlayers", new { matchId });
        }

        [HttpGet]
        public async Task<IActionResult> ManagePlayers(string matchId)
        {
            if (!IsAdmin()) return Unauthorized();
            var players = await _playerService.GetPlayersByMatch(matchId);
            ViewBag.MatchId = matchId;
            return View(players);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRoles(string matchId, Dictionary<string, string> roles)
        {
            if (!IsAdmin()) return Unauthorized();

            foreach (var kvp in roles)
            {
                await _playerService.UpdateRole(kvp.Key, kvp.Value);
            }

            TempData["Success"] = "Player roles updated successfully!";
            return RedirectToAction("ManagePlayers", new { matchId });
        }
    }
}

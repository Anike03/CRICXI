using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
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
        private readonly ContestEntryService _contestEntryService;

        public AdminController(
            MatchService matchService,
            ContestService contestService,
            UserService userService,
            FantasyTeamService teamService,
            CricbuzzApiService cricbuzzApiService,
            PlayerService playerService,
            ContestEntryService contestEntryService)
        {
            _matchService = matchService;
            _contestService = contestService;
            _userService = userService;
            _teamService = teamService;
            _cricbuzzApiService = cricbuzzApiService;
            _playerService = playerService;
            _contestEntryService = contestEntryService;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

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

        // MATCHES MANAGEMENT
        [HttpGet]
        public async Task<IActionResult> SyncMatches()
        {
            if (!IsAdmin()) return Unauthorized();

            var json = await _cricbuzzApiService.GetUpcomingMatchesAsync();
            int matchesProcessed = 0;
            int matchesWithVenue = 0;

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
                                // Extract series ID
                                int seriesId = 0;
                                if (series.TryGetProperty("seriesAdWrapper", out var seriesWrapper))
                                {
                                    if (seriesWrapper.TryGetProperty("seriesId", out var seriesIdProp))
                                    {
                                        seriesId = seriesIdProp.GetInt32();
                                    }
                                }

                                if (series.TryGetProperty("seriesAdWrapper", out var wrapper) &&
                                    wrapper.TryGetProperty("matches", out var matchArray))
                                {
                                    foreach (var m in matchArray.EnumerateArray())
                                    {
                                        if (m.TryGetProperty("matchInfo", out var matchInfo))
                                        {
                                            var team1 = matchInfo.GetProperty("team1").GetProperty("teamName").GetString() ?? "";
                                            var team2 = matchInfo.GetProperty("team2").GetProperty("teamName").GetString() ?? "";
                                            var matchDesc = matchInfo.GetProperty("matchDesc").GetString() ?? "";
                                            var cricbuzzMatchId = matchInfo.GetProperty("matchId").ToString();

                                            string venue = "Unknown Venue";
                                            if (matchInfo.TryGetProperty("venueInfo", out var venueInfo))
                                            {
                                                if (venueInfo.TryGetProperty("ground", out var ground))
                                                {
                                                    venue = ground.GetString() ?? "Unknown Ground";
                                                    matchesWithVenue++;
                                                }
                                                else if (venueInfo.TryGetProperty("city", out var city))
                                                {
                                                    venue = city.GetString() ?? "Unknown City";
                                                }
                                            }

                                            // Change the SyncMatches method to handle null startDate:
                                            var startMillis = matchInfo.GetProperty("startDate").GetString();
                                            if (string.IsNullOrEmpty(startMillis))
                                            {
                                                continue; // Skip invalid matches
                                            }
                                            var startDate = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(startMillis)).DateTime;

                                            var match = new Match
                                            {
                                                CricbuzzMatchId = cricbuzzMatchId,
                                                TeamA = team1,
                                                TeamB = team2,
                                                MatchDesc = matchDesc,
                                                Venue = venue,
                                                StartDate = startDate,
                                                Status = "Upcoming",
                                                SeriesId = seriesId // Store the series ID
                                            };

                                            await _matchService.SaveIfNotExists(match);
                                            matchesProcessed++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            TempData["SyncResult"] = $"Successfully processed {matchesProcessed} matches. {matchesWithVenue} had venue information.";
            return RedirectToAction("AllMatches");
        }

        [HttpGet]
        public async Task<IActionResult> AllMatches()
        {
            if (!IsAdmin()) return Unauthorized();
            var matches = await _matchService.GetAll();
            return View(matches);
        }

        // CONTEST MANAGEMENT
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

            var match = await _matchService.GetById(contest.MatchId);
            if (match != null)
            {
                contest.TeamA = match.TeamA;
                contest.TeamB = match.TeamB;
                contest.StartDate = match.StartDate;

                // ✅ Add this to store Cricbuzz Match ID
                contest.CricbuzzMatchId = match.CricbuzzMatchId;

                // (Optional) set venue if needed
                // contest.Venue = match.Venue;
            }

            await _contestService.Create(contest);
            return RedirectToAction("AllContests");
        }


        // USERS MANAGEMENT
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

        [HttpPost]
        public async Task<IActionResult> BanUser(string userId)
        {
            if (!IsAdmin()) return Unauthorized();
            await _userService.BanUser(userId, 30);
            return RedirectToAction("AllUsers");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (!IsAdmin()) return Unauthorized();
            await _userService.DeleteUser(userId);
            return RedirectToAction("AllUsers");
        }

        // FANTASY TEAMS MANAGEMENT
        [HttpGet]
        public async Task<IActionResult> AllFantasyTeams()
        {
            if (!IsAdmin()) return Unauthorized();
            var teams = await _teamService.GetAllTeams();
            return View(teams);
        }

        // PLAYER SYNC + ROLE MANAGEMENT
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
                var team1Name = team1.GetProperty("teamName").GetString() ?? "";
                var team1Players = team1.GetProperty("players");

                foreach (var p in team1Players.EnumerateArray())
                {
                    players.Add(new Player
                    {
                        CricbuzzMatchId = matchId,
                        CricbuzzPlayerId = p.GetProperty("id").ToString(),
                        Name = p.GetProperty("name").GetString() ?? "",
                        Team = team1Name,
                        Role = ""
                    });
                }

                var team2 = matchInfo.GetProperty("team2");
                var team2Name = team2.GetProperty("teamName").GetString() ?? "";
                var team2Players = team2.GetProperty("players");

                foreach (var p in team2Players.EnumerateArray())
                {
                    players.Add(new Player
                    {
                        CricbuzzMatchId = matchId,
                        CricbuzzPlayerId = p.GetProperty("id").ToString(),
                        Name = p.GetProperty("name").GetString() ?? "",
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

        [HttpGet]
        public async Task<IActionResult> EditContest(string id)
        {
            if (!IsAdmin()) return Unauthorized();
            var contest = await _contestService.GetById(id);
            if (contest == null) return NotFound();
            return View(contest);
        }

        [HttpPost]
        public async Task<IActionResult> EditContest(Contest updated)
        {
            if (!IsAdmin()) return Unauthorized();
            await _contestService.Update(updated.Id, updated);
            return RedirectToAction("AllContests");
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (!IsAdmin()) return Unauthorized();
            var contest = await _contestService.GetById(id);
            if (contest == null) return NotFound();
            return View(contest);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteContest(string id)
        {
            if (!IsAdmin()) return Unauthorized();
            await _contestService.Delete(id);
            return RedirectToAction("AllContests");
        }

        [HttpPost("/api/users/sync")]
        [AllowAnonymous]
        public async Task<IActionResult> SyncFirebaseUser([FromBody] FirebaseUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email required.");

            var existing = await _userService.GetByEmail(dto.Email);
            if (existing != null)
            {
                // Make sure to use dto.Uid (not dto.FirebaseUid)
                if (string.IsNullOrEmpty(existing.FirebaseUid) && !string.IsNullOrEmpty(dto.Uid))
                {
                    existing.FirebaseUid = dto.Uid;
                    await _userService.Update(existing);
                }
                return Ok(existing);
            }

            var newUser = new User
            {
                Username = dto.Username ?? dto.Email.Split('@')[0],
                Email = dto.Email,
                FirebaseUid = dto.Uid,
                WalletBalance = 1000,
                IsEmailConfirmed = true,
                Role = "User",
                IsBannedUntil = null
            };

            await _userService.RegisterWithoutPassword(newUser);
            return Ok(newUser);
        }

        [HttpPost]
        public async Task<IActionResult> UnbanUser(string userId)
        {
            if (!IsAdmin()) return Unauthorized();

            var user = await _userService.GetById(userId);
            if (user == null)
            {
                return NotFound();
            }

            await _userService.UnbanUser(userId);

            return RedirectToAction("AllUsers");
        }

        [HttpGet("/api/users/wallet")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWallet([FromQuery] string email)
        {
            var user = await _userService.GetByEmail(email);
            if (user == null)
                return NotFound("User not found");

            return Ok(new { wallet = user.WalletBalance });
        }

        [HttpGet("/api/users/get-by-email")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var user = await _userService.GetByEmail(email);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> Leaderboard()
        {
            if (!IsAdmin()) return Unauthorized();

            var users = await _userService.GetAllUsers();
            var entries = await _contestEntryService.GetAll();

            var leaderboard = users.Select(u => new LeaderboardEntry
            {
                Username = u.Username,
                Email = u.Email,
                JoinedContests = entries.Count(e => e.Username == u.Username)
            }).OrderByDescending(l => l.JoinedContests).ToList();

            return View(leaderboard);
        }

        [HttpGet("/api/users/{uid}/balance")]
        [AllowAnonymous]
        [EnableCors("AllowReact")]
        public async Task<IActionResult> GetUserBalanceByUid(string uid)
        {
            var origin = Request.Headers["Origin"].ToString();

            if (!string.IsNullOrEmpty(origin) &&
                (origin == "https://cricxi.vercel.app" || origin == "http://localhost:5173"))
            {
                Response.Headers["Access-Control-Allow-Origin"] = origin;
                Response.Headers["Vary"] = "Origin";
            }

            Response.Headers["Access-Control-Allow-Credentials"] = "true";

            var user = await _userService.GetByUid(uid);
            if (user == null)
                return NotFound("User not found");

            return Ok(new { balance = user.WalletBalance });
        }







        [HttpPost("/api/users/sync-uid")]
        [AllowAnonymous]
        public async Task<IActionResult> SyncFirebaseUid([FromBody] FirebaseUidSyncDto dto)
        {
            var user = await _userService.GetByEmail(dto.Email);
            if (user == null) return NotFound("User not found");

            user.FirebaseUid = dto.FirebaseUid;
            await _userService.Update(user);

            return Ok(user);
        }
        [HttpGet("/api/users/{uid}/teams")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserTeamsByMatch(string uid, [FromQuery] string matchId)
        {
            var user = await _userService.GetByUid(uid);
            if (user == null)
                return NotFound("User not found");

            var teams = await _teamService.GetByUserAndMatch(user.Username, matchId);
            return Ok(teams);
        }


    }
}
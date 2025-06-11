using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRICXI.Controllers
{
    public class FantasyTeamController : Controller
    {
        private readonly FantasyTeamService _fantasyTeamService;
        private readonly PlayerService _playerService;

        public FantasyTeamController(FantasyTeamService fantasyTeamService, PlayerService playerService)
        {
            _fantasyTeamService = fantasyTeamService;
            _playerService = playerService;
        }

        [HttpGet]
        public async Task<IActionResult> Create(string matchId)
        {
            var players = await _playerService.GetPlayersByMatch(matchId);
            ViewBag.MatchId = matchId;
            return View(players);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string matchId, string teamName, string captainId, string viceCaptainId, List<string> selectedPlayers)
        {
            var username = HttpContext.Session.GetString("Username");

            if (selectedPlayers.Count != 11)
            {
                TempData["Error"] = "You must select exactly 11 players.";
                return RedirectToAction("Create", new { matchId });
            }

            var playersFromDb = await _playerService.GetPlayersByMatch(matchId);
            var selections = playersFromDb
                .Where(p => selectedPlayers.Contains(p.Id))
                .Select(p => new PlayerSelection
                {
                    PlayerId = p.Id,
                    PlayerName = p.Name,
                    Role = p.Role,
                    Team = p.Team
                }).ToList();

            // ✅ Role validations (Dream11 rules)
            if (!IsValidTeam(selections))
            {
                TempData["Error"] = "Team combination invalid. Please follow Dream11 role rules.";
                return RedirectToAction("Create", new { matchId });
            }

            var team = new FantasyTeam
            {
                Username = username,
                MatchId = matchId,
                TeamName = teamName,
                Players = selections,
                CaptainId = captainId,
                ViceCaptainId = viceCaptainId
            };

            await _fantasyTeamService.Create(team);
            TempData["Success"] = "Team created successfully!";
            return RedirectToAction("Index", "Home");
        }

        // ✅ Dream11 role validations
        private bool IsValidTeam(List<PlayerSelection> players)
        {
            int batsman = players.Count(p => p.Role == "Batsman");
            int bowler = players.Count(p => p.Role == "Bowler");
            int allrounder = players.Count(p => p.Role == "AllRounder");
            int keeper = players.Count(p => p.Role == "WicketKeeper");

            return
                batsman >= 3 && batsman <= 6 &&
                bowler >= 3 && bowler <= 6 &&
                allrounder >= 1 && allrounder <= 4 &&
                keeper >= 1 && keeper <= 2;
        }
    }
}

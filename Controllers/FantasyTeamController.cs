using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace CRICXI.Controllers
{
    public class FantasyTeamController : Controller
    {
        private readonly FantasyTeamService _fantasyTeamService;

        public FantasyTeamController(FantasyTeamService fantasyTeamService)
        {
            _fantasyTeamService = fantasyTeamService;
        }

        public IActionResult Create(string matchId, string contestId)
        {
            ViewBag.MatchId = matchId;
            ViewBag.ContestId = contestId;

            // You would replace this with API-fetched real players
            ViewBag.Players = GetMockPlayers();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string matchId, string contestId, List<string> selectedPlayers, string captainId, string viceCaptainId)
        {
            if (selectedPlayers.Count != 11)
            {
                ViewBag.Error = "You must select exactly 11 players.";
                ViewBag.Players = GetMockPlayers();
                return View();
            }

            var team = new FantasyTeam
            {
                UserId = HttpContext.Session.GetString("UserId"), // ensure session contains this
                MatchId = matchId,
                ContestId = contestId,
                PlayerIds = selectedPlayers,
                CaptainId = captainId,
                ViceCaptainId = viceCaptainId
            };

            await _fantasyTeamService.Create(team);
            return RedirectToAction("Index", "Home");
        }

        private List<Player> GetMockPlayers()
        {
            return new List<Player>
            {
                new Player { Id = "1", Name = "Virat Kohli", Role = "Batsman" },
                new Player { Id = "2", Name = "Rohit Sharma", Role = "Batsman" },
                new Player { Id = "3", Name = "Jasprit Bumrah", Role = "Bowler" },
                new Player { Id = "4", Name = "Hardik Pandya", Role = "All-Rounder" },
                new Player { Id = "5", Name = "MS Dhoni", Role = "Wicket-Keeper" },
                new Player { Id = "6", Name = "Shubman Gill", Role = "Batsman" },
                new Player { Id = "7", Name = "Kuldeep Yadav", Role = "Bowler" },
                new Player { Id = "8", Name = "Axar Patel", Role = "All-Rounder" },
                new Player { Id = "9", Name = "KL Rahul", Role = "Wicket-Keeper" },
                new Player { Id = "10", Name = "Suryakumar Yadav", Role = "Batsman" },
                new Player { Id = "11", Name = "Mohammed Siraj", Role = "Bowler" },
                new Player { Id = "12", Name = "Ravindra Jadeja", Role = "All-Rounder" }
            };
        }
    }
}

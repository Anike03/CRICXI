using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRICXI.Controllers
{
    [ApiController]
    [Route("api/team")]
    public class FantasyTeamController : ControllerBase
    {
        private readonly FantasyTeamService _teamService;

        public FantasyTeamController(FantasyTeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTeam([FromBody] FantasyTeam team)
        {
            Console.WriteLine("Received team:");
            Console.WriteLine($"Username: {team.Username}");
            Console.WriteLine($"MatchId: {team.MatchId}");
            Console.WriteLine($"TeamName: {team.TeamName}");
            Console.WriteLine($"Players Count: {team.Players?.Count}");

            if (string.IsNullOrWhiteSpace(team.Username) || team.Players == null || !team.Players.Any())
                return BadRequest("Invalid team data.");

            var canCreate = await _teamService.CanCreateTeam(team.Username, team.MatchId);
            if (!canCreate)
                return BadRequest("Maximum 3 teams already created for this match.");

            await _teamService.CreateTeamAsync(team);
            return Ok(team);
        }

        [HttpGet("match/{matchId}")]
        public async Task<IActionResult> GetTeamsByMatch(string matchId)
        {
            var username = User?.Identity?.Name ?? HttpContext.Request.Headers["Username"].ToString();
            if (string.IsNullOrEmpty(username))
                return BadRequest("Username is required.");

            var teams = await _teamService.GetByUserAndMatch(username, matchId);
            return Ok(teams);
        }
    }
}

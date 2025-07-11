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

        // POST: api/team/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateTeam([FromBody] FantasyTeam team)
        {
            if (string.IsNullOrWhiteSpace(team.Username) || team.Players == null || !team.Players.Any())
                return BadRequest("Invalid team data.");

            await _teamService.CreateTeamAsync(team);
            return Ok(team);
        }

        // GET: api/team/match/{matchId}
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

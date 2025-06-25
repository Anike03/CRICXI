using Microsoft.AspNetCore.Mvc;
using CRICXI.Models;
using CRICXI.Services;

namespace CRICXI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FantasyTeamController : ControllerBase
    {
        private readonly FantasyTeamService _fantasyTeamService;

        public FantasyTeamController(FantasyTeamService fantasyTeamService)
        {
            _fantasyTeamService = fantasyTeamService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTeam([FromBody] FantasyTeam team)
        {
            if (team == null || team.Players == null || team.Players.Count == 0)
                return BadRequest("Invalid team data");

            await _fantasyTeamService.Create(team);
            return Ok("Team Saved Successfully");
        }
        [HttpGet("user/{username}")]
        public async Task<IActionResult> GetUserTeams(string username)
        {
            var teams = await _fantasyTeamService.GetByUser(username);
            return Ok(teams);
        }

    }
}

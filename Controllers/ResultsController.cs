using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRICXI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly ContestEntryService _entryService;
        private readonly UserService _userService;

        public ResultsController(ContestEntryService entryService, UserService userService)
        {
            _entryService = entryService;
            _userService = userService;
        }

        // ✅ GET /api/results/{contestId}
        [HttpGet("{contestId}")]
        public async Task<IActionResult> GetResults(string contestId)
        {
            var entries = await _entryService.GetByContest(contestId);
            if (entries == null || entries.Count == 0)
                return NotFound("No entries found for this contest");

            var results = new List<object>();

            foreach (var entry in entries)
            {
                var user = await _userService.GetByUsername(entry.Username);
                results.Add(new
                {
                    Username = entry.Username,
                    Email = user?.Email ?? "(unknown)",
                    TeamId = entry.TeamId,
                    Score = entry.Score
                });
            }

            var sorted = results.OrderByDescending(r => ((int)r.GetType().GetProperty("Score").GetValue(r))).ToList();
            return Ok(sorted);
        }
    }
}

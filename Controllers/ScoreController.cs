using Microsoft.AspNetCore.Mvc;
using CRICXI.Services;

namespace CRICXI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScoreController : ControllerBase
    {
        private readonly ScoreCalculatorService _scoreService;

        public ScoreController(ScoreCalculatorService scoreService)
        {
            _scoreService = scoreService;
        }

        // ✅ Calculate and save scores for all teams in a match
        //    URL: /api/score/calculate/50942
        [HttpGet("calculate/{matchId}")]
        public async Task<IActionResult> CalculateAndSave(string matchId)
        {
            try
            {
                var scores = await _scoreService.CalculateAndSaveScores(matchId);
                return Ok(new
                {
                    message = "Scores calculated and stored successfully.",
                    scores
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error: {ex.Message}" });
            }
        }

        // ✅ Get only scores (read-only, no DB update)
        //    URL: /api/score/view/50942
        [HttpGet("view/{matchId}")]
        public async Task<IActionResult> ViewOnly(string matchId)
        {
            try
            {
                var scores = await _scoreService.CalculateMatchScores(matchId);
                return Ok(scores);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error: {ex.Message}" });
            }
        }
    }
}

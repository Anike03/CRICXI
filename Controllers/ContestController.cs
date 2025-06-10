using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRICXI.Controllers
{
    public class ContestController : Controller
    {
        private readonly ContestService _contestService;
        private readonly MatchService _matchService;
        private readonly ContestEntryService _entryService;

        public ContestController(
            ContestService contestService,
            MatchService matchService,
            ContestEntryService entryService)
        {
            _contestService = contestService;
            _matchService = matchService;
            _entryService = entryService;
        }

        public async Task<IActionResult> Index()
        {
            var contests = await _contestService.GetAll();
            return View(contests);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string matchId)
        {
            var match = await _matchService.GetById(matchId);
            ViewBag.Match = match;

            return View(new Contest { MatchId = matchId });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Contest contest)
        {
            if (!ModelState.IsValid)
            {
                var match = await _matchService.GetById(contest.MatchId);
                ViewBag.Match = match;
                return View(contest);
            }

            await _contestService.Create(contest);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(string id)
        {
            var contest = await _contestService.GetById(id);
            return View(contest);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, Contest updated)
        {
            await _contestService.Update(id, updated);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(string id)
        {
            var contest = await _contestService.GetById(id);
            return View(contest);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _contestService.Delete(id);
            return RedirectToAction("Index");
        }

        // ✅ Join a contest - GET
        [HttpGet]
        public async Task<IActionResult> Join(string contestId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Auth");

            var contest = await _contestService.GetById(contestId);
            var match = await _matchService.GetById(contest.MatchId);

            ViewBag.Match = match;
            ViewBag.Contest = contest;

            return View(); // Views/Contest/Join.cshtml
        }

        // ✅ Join a contest - POST
        [HttpPost]
        public async Task<IActionResult> Join(string contestId, string teamId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Auth");

            var alreadyJoined = await _entryService.HasUserJoined(contestId, username);
            if (alreadyJoined)
            {
                TempData["Error"] = "You've already joined this contest.";
                return RedirectToAction("Index");
            }

            var contest = await _contestService.GetById(contestId);

            var entry = new ContestEntry
            {
                ContestId = contestId,
                MatchId = contest.MatchId,
                Username = username,
                TeamId = teamId
            };

            await _entryService.Add(entry);

            TempData["Success"] = "You have successfully joined the contest!";
            return RedirectToAction("Index");
        }
    }
}

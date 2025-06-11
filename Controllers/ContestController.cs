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
        private readonly UserService _userService;
        private readonly FantasyTeamService _fantasyTeamService;

        public ContestController(
            ContestService contestService,
            MatchService matchService,
            ContestEntryService entryService,
            UserService userService,
            FantasyTeamService fantasyTeamService)
        {
            _contestService = contestService;
            _matchService = matchService;
            _entryService = entryService;
            _userService = userService;
            _fantasyTeamService = fantasyTeamService;
        }

        //---------------------- CLIENT SIDE ----------------------

        // ✅ Show all contests (client side)
        public async Task<IActionResult> Index()
        {
            var contests = await _contestService.GetAll();
            return View(contests);
        }

        // ✅ Client joins contest (GET)
        [HttpGet]
        public async Task<IActionResult> Join(string contestId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Auth");

            var contest = await _contestService.GetById(contestId);
            if (contest == null) return NotFound();

            var match = await _matchService.GetById(contest.MatchId);
            if (match == null) return NotFound();

            var teams = await _fantasyTeamService.GetByUserAndMatch(username, match.Id);

            ViewBag.Match = match;
            ViewBag.Contest = contest;
            ViewBag.Teams = teams;

            return View();
        }

        // ✅ Client joins contest (POST)
        [HttpPost]
        public async Task<IActionResult> Join(string contestId, string teamId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Auth");

            var contest = await _contestService.GetById(contestId);
            if (contest == null) return NotFound();

            var user = await _userService.GetByUsername(username);
            if (user == null) return Unauthorized();

            var alreadyJoined = await _entryService.HasUserJoined(contestId, username);
            if (alreadyJoined)
            {
                TempData["Error"] = "You have already joined this contest.";
                return RedirectToAction("Index");
            }

            // ✅ Wallet balance check
            if (user.WalletBalance < contest.EntryFee)
            {
                TempData["Error"] = $"Insufficient wallet balance. You need ${contest.EntryFee - user.WalletBalance} more.";
                return RedirectToAction("Index");
            }

            // ✅ Deduct entry fee
            await _userService.UpdateWallet(user.Id, contest.EntryFee, addFunds: false);

            var entry = new ContestEntry
            {
                ContestId = contestId,
                MatchId = contest.MatchId,
                Username = username,
                TeamId = teamId
            };

            await _entryService.Add(entry);
            TempData["Success"] = "Successfully joined the contest!";
            return RedirectToAction("Index");
        }

        //---------------------- ADMIN SIDE ----------------------

        // ✅ Show all contests (admin)
        [HttpGet]
        public async Task<IActionResult> AdminContests()
        {
            if (!IsAdmin()) return Unauthorized();

            var contests = await _contestService.GetAll();
            return View(contests);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string matchId)
        {
            if (!IsAdmin()) return Unauthorized();

            var match = await _matchService.GetById(matchId);
            if (match == null) return NotFound();

            ViewBag.Match = match;
            return View(new Contest { MatchId = matchId });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Contest contest)
        {
            if (!IsAdmin()) return Unauthorized();

            await _contestService.Create(contest);
            return RedirectToAction("AdminContests");
        }

        //---------------------- SECURITY ----------------------

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }
    }
}

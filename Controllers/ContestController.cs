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

        // ✅ API: All contests (used by React frontend)
        [HttpGet]
        [Route("api/contests/all")]
        public async Task<IActionResult> GetAllContests()
        {
            var contests = await _contestService.GetAll();
            var enriched = new List<object>();

            foreach (var c in contests)
            {
                var joinedCount = await _entryService.GetJoinedCount(c.Id);
                enriched.Add(new
                {
                    id = c.Id,
                    name = c.Name,
                    matchId = c.CricbuzzMatchId, // ✅ Send external ID
                    teamA = c.TeamA,
                    teamB = c.TeamB,
                    entryFee = c.EntryFee,
                    totalPrize = c.TotalPrize,
                    maxParticipants = c.MaxParticipants,
                    startDate = c.StartDate,
                    joined = joinedCount
                });
            }

            return Ok(enriched);
        }

        // ✅ API: Get contests by Cricbuzz Match ID
        [HttpGet]
        [Route("api/contests/by-match/{matchId}")]
        public async Task<IActionResult> GetContestsByMatch(string matchId)
        {
            var contests = await _contestService.GetUpcomingByMatchId(matchId);
            var enriched = new List<object>();

            foreach (var c in contests)
            {
                var joinedCount = await _entryService.GetJoinedCount(c.Id);
                enriched.Add(new
                {
                    id = c.Id,
                    name = c.Name,
                    cricbuzzMatchId = c.CricbuzzMatchId,   // ✅ External ID
                    mongoMatchId = c.MatchId,              // ✅ Internal MongoDB ID
                    teamA = c.TeamA,
                    teamB = c.TeamB,
                    entryFee = c.EntryFee,
                    totalPrize = c.TotalPrize,
                    maxParticipants = c.MaxParticipants,
                    startDate = c.StartDate,
                    joined = joinedCount
                });

            }

            return Ok(enriched);
        }

        // ✅ Admin Razor view
        public async Task<IActionResult> Index()
        {
            var contests = await _contestService.GetAll();
            return View(contests);
        }

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

            var match = await _matchService.GetById(contest.MatchId);
            if (match != null)
            {
                contest.TeamA = match.TeamA;
                contest.TeamB = match.TeamB;
                contest.StartDate = match.StartDate;

                // ✅ Store both IDs
                contest.CricbuzzMatchId = match.CricbuzzMatchId;
                contest.MatchId = match.Id; // MongoDB internal ID
            }

            await _contestService.Create(contest);
            return RedirectToAction("AdminContests");
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var contest = await _contestService.GetById(id);
            if (contest == null) return NotFound();
            var entries = await _entryService.GetByContest(id);
            ViewBag.Entries = entries;
            return View(contest);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var contest = await _contestService.GetById(id);
            if (contest == null) return NotFound();
            return View(contest);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, Contest updated)
        {
            if (id != updated.Id) return BadRequest();
            await _contestService.Update(id, updated);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _contestService.Delete(id);
            return RedirectToAction("Index");
        }

        // ✅ Join contest view
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

        // ✅ Submit join
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

            if (user.WalletBalance < contest.EntryFee)
            {
                TempData["Error"] = $"Insufficient wallet balance. You need ₹{contest.EntryFee - user.WalletBalance} more.";
                return RedirectToAction("Index");
            }

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

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }
        [HttpGet]
        [Route("api/contests/{id}")]
        public async Task<IActionResult> GetContestById(string id)
        {
            var contest = await _contestService.GetById(id);
            if (contest == null)
                return NotFound();

            return Ok(contest);
        }
        // ✅ API: Join contest (for React frontend)
        [HttpPost]
        [Route("api/contests/join")]
        public async Task<IActionResult> JoinContest([FromBody] JoinContestRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.TeamId))
                return BadRequest(new { success = false, message = "Invalid join request." });

            var contest = await _contestService.GetById(request.ContestId);
            if (contest == null)
                return NotFound(new { success = false, message = "Contest not found." });

            var user = await _userService.GetByUid(request.UserId);
            if (user == null)
                return NotFound(new { success = false, message = "User not found." });

            // Check wallet balance
            if (user.WalletBalance < contest.EntryFee)
                return BadRequest(new { success = false, message = "Insufficient wallet balance." });

            // Check if user already joined
            var alreadyJoined = await _entryService.HasUserJoined(contest.Id, user.Username);
            if (alreadyJoined)
                return BadRequest(new { success = false, message = "User already joined this contest." });

            // Deduct balance
            var success = await _userService.UpdateWallet(user.Id, contest.EntryFee, addFunds: false);
            if (!success)
                return BadRequest(new { success = false, message = "Failed to deduct balance." });

            // Add entry
            var entry = new ContestEntry
            {
                ContestId = contest.Id,
                MatchId = contest.MatchId,
                Username = user.Username,
                TeamId = request.TeamId
            };
            await _entryService.Add(entry);

            return Ok(new { success = true, message = "Successfully joined contest." });
        }

        public class JoinContestRequest
        {
            public string ContestId { get; set; } = null!;
            public string UserId { get; set; } = null!;
            public string TeamId { get; set; } = null!;
            public decimal EntryFee { get; set; }
        }


    }
}

using System.Diagnostics;
using CRICXI.Models;
using Microsoft.AspNetCore.Mvc;
using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private readonly MatchService _matchService;

    public HomeController(MatchService matchService)
    {
        _matchService = matchService;
    }

    public async Task<IActionResult> Index()
    {
        var matches = await _matchService.GetAll();
        var upcoming = matches.Where(m => m.Status == "Upcoming").ToList();
        return View(upcoming);
    }

    public IActionResult Privacy() => View();
}

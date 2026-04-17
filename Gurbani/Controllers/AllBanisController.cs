using Microsoft.AspNetCore.Mvc;
using Gurbani.Models;
using Gurbani.Services;

namespace Gurbani.Controllers;

// Handles the All Banis section — lists all 29 available Banis and lets users read any of them
public class AllBanisController(GurbaniService gurbaniService) : Controller
{
    // Handles GET /AllBanis — shows the full list of all 29 Banis
    public async Task<IActionResult> Index()
    {
        var banis = await gurbaniService.GetAllBanisAsync();
        var vm = new AllBanisViewModel { Banis = banis };
        return View(vm);
    }

    // Handles GET /AllBanis/Read?id=12 — reads any Bani by its ID
    public async Task<IActionResult> Read(int id)
    {
        var vm = await gurbaniService.GetBaniAsync(id);
        if (vm == null) ViewBag.Error = "Could not load this Bani. Please try again.";
        return View("~/Views/Bani/Read.cshtml", vm); // Reuses the same Bani reader view
    }
}

using Microsoft.AspNetCore.Mvc;
using Gurbani.Services;

namespace Gurbani.Controllers;

// Handles all routes related to the Daily Hukamnama
public class HukamnamaController(GurbaniService gurbaniService) : Controller
{
    // Handles GET /Hukamnama/Index
    public async Task<IActionResult> Index()
    {
        // Fetch today's Hukamnama (from DB cache or API)
        var vm = await gurbaniService.GetHukamnamaAsync();

        // If something went wrong, show an error message on the page
        if (vm == null) ViewBag.Error = "Could not load today's Hukamnama. Please try again.";

        return View(vm);
    }
}

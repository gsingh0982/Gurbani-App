using Microsoft.AspNetCore.Mvc;
using Gurbani.Services;

namespace Gurbani.Controllers;

// Handles all routes related to reading Guru Granth Sahib
public class GurbaniController(GurbaniService gurbaniService) : Controller
{
    // Handles GET /Gurbani/Read?ang=1
    // "ang" defaults to 1 if the user doesn't provide one
    public async Task<IActionResult> Read(int ang = 1)
    {
        // Make sure the ang number stays within valid range (1 to 1430)
        ang = Math.Clamp(ang, 1, 1430);

        // Fetch the ang data (from DB cache or API)
        var vm = await gurbaniService.GetAngAsync(ang);

        // If something went wrong, show an error message on the page
        if (vm == null) ViewBag.Error = "Could not load this ang. Please try again.";

        return View(vm);
    }
}

using Microsoft.AspNetCore.Mvc;
using Gurbani.Services;

namespace Gurbani.Controllers;

// Handles the Panj Baani section — selection page and individual Bani reader
public class BaniController(GurbaniService gurbaniService) : Controller
{
    // The 5 daily prayers with their API IDs — hardcoded since they never change
    private static readonly Dictionary<int, string> PanjBaani = new()
    {
        { 1, "Jap Ji Sahib" },
        { 2, "Jaap Sahib" },
        { 3, "Tav Prasad Savaiye" },
        { 4, "Benti Chaupai Sahib" },
        { 5, "Anand Sahib" }
    };

    // Handles GET /Bani — shows the Panj Baani selection page
    public IActionResult Index()
    {
        return View(PanjBaani);
    }

    // Handles GET /Bani/Read?id=1 — reads a specific Bani
    public async Task<IActionResult> Read(int id)
    {
        // Only allow the 5 Panj Baani IDs on this page
        if (!PanjBaani.ContainsKey(id)) return RedirectToAction("Index");

        var vm = await gurbaniService.GetBaniAsync(id);
        if (vm == null) ViewBag.Error = "Could not load this Bani. Please try again.";

        return View(vm);
    }
}

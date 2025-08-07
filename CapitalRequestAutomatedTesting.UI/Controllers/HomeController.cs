using CapitalRequestAutomatedTesting.UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CapitalRequestAutomatedTesting.UI.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("UI loaded at {Time}", DateTime.UtcNow);

        var name = HttpContext.User.Identity?.Name;
        var authenticated =  Content($"User: {name ?? "Not Authenticated"}");


        Debug.WriteLine($"Identity Name: {HttpContext.User.Identity?.Name}");

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}


using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ScenarioController : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<JsonResult> Scenarios()
    {
        var scenarios = new List<object>
        {
            new { id = "SCN001", name = "Scenario 1" },
            new { id = "SCN002", name = "Scenario 2" },
            new { id = "SCN003", name = "Scenario 3" }
        };

        return Json(scenarios);
    }

    [HttpPost]
    public IActionResult RunSelected(List<string> selectedScenarioIds)
    {
        // Add logic to handle the selected scenarios
        ViewBag.Message = $"You selected: {string.Join(", ", selectedScenarioIds)}";
        return View("Index");
    }
}

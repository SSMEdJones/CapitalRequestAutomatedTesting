using CapitalRequestAutomatedTesting.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CapitalRequestAutomatedTesting.UI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class DiTestController : ControllerBase
    {
        private readonly IWorkflowControllerService _service;

        public DiTestController(IWorkflowControllerService service)
        {
            _service = service;
        }

        [HttpGet("check")]
        public IActionResult Check()
        {
            return Ok("Service resolved successfully!");
        }
    }

}

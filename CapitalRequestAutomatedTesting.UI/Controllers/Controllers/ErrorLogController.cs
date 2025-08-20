using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CapitalRequestAutomatedTesting.UI.Services;

namespace CapitalRequestAutomatedTesting.UI.Controllers
{
    [Authorize] // Make sure only authorized users can see the error logs
    public class ErrorLogController : Controller
    {
        private readonly IErrorLogService _errorLogService;
        
        public ErrorLogController(IErrorLogService errorLogService)
        {
            _errorLogService = errorLogService;
        }
        
        public async Task<IActionResult> Index(int page = 1, int pageSize = 50)
        {
            var logs = await _errorLogService.GetRecentErrorLogsAsync(pageSize * page);
            var count = await _errorLogService.GetErrorCountAsync();
            
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)count / pageSize);
            
            return View(logs.Skip((page - 1) * pageSize).Take(pageSize).ToList());
        }
        
        public async Task<IActionResult> Details(int id)
        {
            var log = await _errorLogService.GetErrorLogByIdAsync(id);
            if (log == null)
            {
                return NotFound();
            }
            
            return View(log);
        }
        
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }
            
            var logs = await _errorLogService.SearchErrorLogsAsync(query);
            ViewBag.Query = query;
            
            return View("Index", logs);
        }
    }
}
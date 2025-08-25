using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CapitalRequestAutomatedTesting.UI.Services;
using CapitalRequestAutomatedTesting.UI.Models;

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
            var totalCount = await _errorLogService.GetErrorCountAsync();
            var logs = await _errorLogService.GetRecentErrorLogsAsync(pageSize * page);
            
            var viewModel = new ErrorLogListViewModel
            {
                Logs = logs.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
            
            return View(viewModel);
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
        
        public async Task<IActionResult> Search(string query, int page = 1, int pageSize = 50)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }
            
            var logs = await _errorLogService.SearchErrorLogsAsync(query);
            
            var viewModel = new ErrorLogListViewModel
            {
                Logs = logs.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = logs.Count,
                SearchQuery = query
            };
            
            return View("Index", viewModel);
        }
    }
}
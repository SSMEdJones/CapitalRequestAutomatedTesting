using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CapitalRequestAutomatedTesting.UI.Controllers
{
    [Route("Error")]
    public class ErrorController : Controller
    {
        private readonly IErrorLogService _errorLogService;
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(IErrorLogService errorLogService, ILogger<ErrorController> logger)
        {
            _errorLogService = errorLogService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? id, int page = 1, int pageSize = 50)
        {
            // If id is numeric, just delegate to Details
            if (!string.IsNullOrWhiteSpace(id) && int.TryParse(id, out var errorId))
            {
                return RedirectToAction(nameof(Details), new { id = errorId });
            }

            var totalCount = await _errorLogService.GetErrorCountAsync();
            var logs = await _errorLogService.GetRecentErrorLogsAsync(pageSize * page);

            var viewModel = new ErrorLogListViewModel
            {
                Logs = logs.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View("Errors",viewModel);
        }


        [Route("Error/{statusCode}")]
        public IActionResult StatusCodePage(int statusCode)
        {
            var viewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode
            };

            _logger.LogWarning("Status code {StatusCode} generated for request {RequestId}",
                statusCode, viewModel.RequestId);

            return View("StatusCode", viewModel);
        }

        [HttpGet("Errors")]
        public async Task<IActionResult> Errors(int page = 1, int pageSize = 50)
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

        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var errorLog = await _errorLogService.GetErrorLogByIdAsync(id);
            if (errorLog == null)
            {
                errorLog = new ErrorLogViewModel();
            }

            // If not found or not an integer, return generic error
            return View(errorLog);
        }

        [HttpGet("Search")]
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
//using CapitalRequestAutomatedTesting.UI.Services;
//using Microsoft.AspNetCore.Mvc;

//namespace CapitalRequestAutomatedTesting.UI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class EventsController : ControllerBase
//    {
//        private readonly IServerSentEventsService _sseService;

//        public EventsController(IServerSentEventsService sseService)
//        {
//            _sseService = sseService;
//        }

//        [HttpGet("scenario-progress/{connectionId}")]
//        public async Task GetScenarioProgress(string connectionId, CancellationToken cancellationToken)
//        {
//            Response.Headers.Add("Content-Type", "text/event-stream");
//            Response.Headers.Add("Cache-Control", "no-cache");
//            Response.Headers.Add("Connection", "keep-alive");
//            Response.Headers.Add("Access-Control-Allow-Origin", "*");

//            await _sseService.AddConnectionAsync(connectionId, Response);

//            try
//            {
//                // Keep connection alive until cancelled
//                while (!cancellationToken.IsCancellationRequested)
//                {
//                    await Task.Delay(1000, cancellationToken);
//                }
//            }
//            catch (OperationCanceledException)
//            {
//                // Client disconnected - normal behavior
//            }
//            finally
//            {
//                _sseService.RemoveConnection(connectionId);
//            }

//            return;
//        }
//    }
//}
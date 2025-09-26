using CapitalRequestAutomatedTesting.UI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public class ScenarioHeartbeatService : BackgroundService
    {
        private readonly IHubContext<ScenarioProgressHub> _hubContext;

        public ScenarioHeartbeatService(IHubContext<ScenarioProgressHub> hubContext)
        {
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _hubContext.Clients.All.SendAsync("Heartbeat", DateTime.UtcNow);
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }

}

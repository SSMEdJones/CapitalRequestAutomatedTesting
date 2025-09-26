using CapitalRequestAutomatedTesting.UI.Services.Actual;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace CapitalRequestAutomatedTesting.UI.Hubs
{
    public class ScenarioProgressHub : Hub
    {
        private readonly ILogger<ScenarioProgressHub> _logger;
        public ScenarioProgressHub(ILogger<ScenarioProgressHub> logger)
        {
            _logger = logger;
            Debug.WriteLine("ScenarioProgressHub constructed");
        }

        public async Task JoinScenarioGroup(string connectionId)
        {
            var groupName = $"scenario-{connectionId}";
            await Clients.All.SendAsync("Heartbeat", DateTime.UtcNow);

        }
    }

}
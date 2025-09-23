using Microsoft.AspNetCore.SignalR;

namespace CapitalRequestAutomatedTesting.UI.Hubs
{
    public class ScenarioProgressHub : Hub
    {
        public async Task JoinScenarioGroup(string connectionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"scenario-{connectionId}");
        }
    }
}
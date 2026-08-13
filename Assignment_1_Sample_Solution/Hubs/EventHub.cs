using Microsoft.AspNetCore.SignalR;

namespace Assignment_1_Sample_Solution.Hubs
{
    public class EventHub : Hub
    {
        public async Task JoinEventGroup(int eventId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"event-{eventId}");
        }

        public async Task LeaveEventGroup(int eventId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"event-{eventId}");
        }
    }
}
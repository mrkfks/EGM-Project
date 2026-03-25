using EGM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace EGM.Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var role     = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            var cityStr  = Context.User?.FindFirst("cityId")?.Value;

            if (role == Roles.IlYoneticisi && int.TryParse(cityStr, out var cityId))
                await Groups.AddToGroupAsync(Context.ConnectionId, NotificationGroupNames.CityManagers(cityId));

            if (role == Roles.BaskanlikYoneticisi)
                await Groups.AddToGroupAsync(Context.ConnectionId, NotificationGroupNames.HQManagers);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var role    = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            var cityStr = Context.User?.FindFirst("cityId")?.Value;

            if (role == Roles.IlYoneticisi && int.TryParse(cityStr, out var cityId))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, NotificationGroupNames.CityManagers(cityId));

            if (role == Roles.BaskanlikYoneticisi)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, NotificationGroupNames.HQManagers);

            await base.OnDisconnectedAsync(exception);
        }
    }
}

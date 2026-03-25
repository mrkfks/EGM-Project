using EGM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace EGM.Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private static string? GetRole(HubCallerContext ctx)
            => ctx.User?.FindFirst(ClaimTypes.Role)?.Value
            ?? ctx.User?.FindFirst("role")?.Value;

        public override async Task OnConnectedAsync()
        {
            var role    = GetRole(Context);
            var cityStr = Context.User?.FindFirst("cityId")?.Value;

            if ((role == Roles.IlPersoneli || role == Roles.IlYoneticisi)
                && int.TryParse(cityStr, out var cityId))
                await Groups.AddToGroupAsync(Context.ConnectionId, NotificationGroupNames.City(cityId));

            if (role == Roles.BaskanlikPersoneli || role == Roles.BaskanlikYoneticisi)
                await Groups.AddToGroupAsync(Context.ConnectionId, NotificationGroupNames.HQ);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var role    = GetRole(Context);
            var cityStr = Context.User?.FindFirst("cityId")?.Value;

            if ((role == Roles.IlPersoneli || role == Roles.IlYoneticisi)
                && int.TryParse(cityStr, out var cityId))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, NotificationGroupNames.City(cityId));

            if (role == Roles.BaskanlikPersoneli || role == Roles.BaskanlikYoneticisi)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, NotificationGroupNames.HQ);

            await base.OnDisconnectedAsync(exception);
        }
    }
}

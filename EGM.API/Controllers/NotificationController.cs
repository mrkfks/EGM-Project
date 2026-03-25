using EGM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGM.API.Controllers;

[ApiController]
[Route("api/notification")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly IInAppNotificationService _notifService;
    private readonly ICurrentUserService       _currentUser;

    public NotificationController(
        IInAppNotificationService notifService,
        ICurrentUserService currentUser)
    {
        _notifService = notifService;
        _currentUser  = currentUser;
    }

    /// <summary>Kullanıcının son 50 bildirimini döner.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var items = await _notifService.GetUserNotificationsAsync(userId);

        var result = items.Take(50).Select(n => new
        {
            id        = n.Id,
            title     = n.Title,
            message   = n.Message,
            riskScore = n.RiskScore,
            isRead    = n.IsRead,
            type      = n.Type.ToString(),
            createdAt = n.CreatedAt
        });

        return Ok(result);
    }

    /// <summary>Tek bir bildirimi okundu olarak işaretler.</summary>
    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        await _notifService.MarkAsReadAsync(id, userId);
        return NoContent();
    }

    /// <summary>Kullanıcının tüm bildirimlerini okundu olarak işaretler.</summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        await _notifService.MarkAllAsReadAsync(userId);
        return NoContent();
    }
}

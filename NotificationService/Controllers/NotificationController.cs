using Microsoft.AspNetCore.Mvc;
using UserService.DatabaseContext;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/notification")]
public class NotificationController : ControllerBase
{
    private readonly NotificationContext _context;

    public NotificationController(NotificationContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetAllNotifications()
    {
        var notifications = _context.Notifications.ToList();
        return Ok(notifications);
    }
}

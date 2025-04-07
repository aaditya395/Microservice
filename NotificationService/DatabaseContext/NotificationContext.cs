using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace UserService.DatabaseContext
{
    public class NotificationContext : DbContext
    {
        public NotificationContext(DbContextOptions<NotificationContext> options) : base(options) { }
        public DbSet<Notification> Notifications { get; set; }
    }
}

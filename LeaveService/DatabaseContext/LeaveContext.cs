using LeaveService.Models;
using Microsoft.EntityFrameworkCore;

namespace UserService.DatabaseContext
{
    public class LeaveContext : DbContext
    {
        public LeaveContext(DbContextOptions<LeaveContext> options) : base(options) { }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<EmployeeLeaveBalance> LeaveBalances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeLeaveBalance>()
                .HasKey(e => e.EmployeeId);  // Explicitly set primary key

            base.OnModelCreating(modelBuilder);
        }
    }
}

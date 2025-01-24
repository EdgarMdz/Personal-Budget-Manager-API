using Microsoft.EntityFrameworkCore;
using PersonalBudgetManager.Api.DataContext.Entities;

namespace PersonalBudgetManager.Api.DataContext
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId).ValueGeneratedOnAdd();

                entity.Property(e => e.PasswordHash).HasMaxLength(100);

                entity.Property(e => e.Salt).HasMaxLength(50);

                entity.Property(e => e.Username).HasMaxLength(50);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

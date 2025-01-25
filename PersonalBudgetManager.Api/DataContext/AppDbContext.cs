using Microsoft.EntityFrameworkCore;
using PersonalBudgetManager.Api.DataContext.Entities;

namespace PersonalBudgetManager.Api.DataContext
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        DbSet<User> Users { get; set; }
        DbSet<Category> Categories { get; set; }
        DbSet<Income> Incomes { get; set; }
        DbSet<Expense> Expenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("UserId").ValueGeneratedOnAdd();

                entity.Property(e => e.PasswordHash).HasMaxLength(100);

                entity.Property(e => e.Salt).HasMaxLength(50);

                entity.Property(e => e.Username).HasMaxLength(50);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("CategoryId").ValueGeneratedOnAdd();

                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
                // Configure foreign key relationship
                entity
                    .HasOne(e => e.User) // One-to-Many relationship
                    .WithMany(u => u.Categories) // User has many Categories
                    .HasForeignKey(e => e.UserId) // FK in Categories
                    .OnDelete(DeleteBehavior.Restrict); // Optional: Cascade restric
            });

            modelBuilder.Entity<Expense>(entity =>
            {
                entity.ToTable("Expenses");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("ExpenseId");

                entity.Property(e => e.Amount).HasColumnType("decimal(10,2)");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity
                    .HasOne(e => e.User)
                    .WithMany(u => u.Expenses)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity
                    .HasOne(e => e.Category)
                    .WithMany(c => c.Expenses)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Income>(entity =>
            {
                entity.ToTable("Incomes");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("IncomeId").ValueGeneratedOnAdd();

                entity.Property(e => e.Amount).HasColumnType("Decimal(10,2)");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity
                    .HasOne(e => e.User)
                    .WithMany(u => u.Incomes)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity
                    .HasOne(e => e.Category)
                    .WithMany(c => c.Incomes)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

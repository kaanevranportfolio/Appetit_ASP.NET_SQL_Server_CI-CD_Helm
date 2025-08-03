using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantMenuAPI.Models;

namespace RestaurantMenuAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<RestaurantSettings> RestaurantSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision
            modelBuilder.Entity<MenuItem>()
                .Property(m => m.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            // Configure relationships
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Table)
                .WithMany(t => t.Reservations)
                .HasForeignKey(r => r.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Reservation)
                .WithOne(r => r.Order)
                .HasForeignKey<Order>(o => o.ReservationId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MenuItem>()
                .HasOne(mi => mi.Category)
                .WithMany(c => c.MenuItems)
                .HasForeignKey(mi => mi.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.MenuItem)
                .WithMany(mi => mi.OrderItems)
                .HasForeignKey(oi => oi.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure indexes
            modelBuilder.Entity<Table>()
                .HasIndex(t => t.TableNumber)
                .IsUnique();

            modelBuilder.Entity<RestaurantSettings>()
                .HasIndex(rs => rs.SettingKey)
                .IsUnique();

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Appetizers", Description = "Start your meal with our delicious appetizers" },
                new Category { Id = 2, Name = "Main Courses", Description = "Our signature main dishes" },
                new Category { Id = 3, Name = "Desserts", Description = "Sweet endings to your meal" },
                new Category { Id = 4, Name = "Beverages", Description = "Refreshing drinks and cocktails" }
            );

            // Seed Tables
            modelBuilder.Entity<Table>().HasData(
                new Table { Id = 1, TableNumber = "T01", Capacity = 2 },
                new Table { Id = 2, TableNumber = "T02", Capacity = 4 },
                new Table { Id = 3, TableNumber = "T03", Capacity = 4 },
                new Table { Id = 4, TableNumber = "T04", Capacity = 6 },
                new Table { Id = 5, TableNumber = "T05", Capacity = 8 }
            );

            // Seed Restaurant Settings
            modelBuilder.Entity<RestaurantSettings>().HasData(
                new RestaurantSettings { Id = 1, SettingKey = SettingKeys.MaxReservationsPerDay, SettingValue = "50", Description = "Maximum reservations allowed per day" },
                new RestaurantSettings { Id = 2, SettingKey = SettingKeys.MaxReservationsPerUser, SettingValue = "3", Description = "Maximum active reservations per user" },
                new RestaurantSettings { Id = 3, SettingKey = SettingKeys.ReservationTimeSlotDuration, SettingValue = "120", Description = "Reservation time slot duration in minutes" },
                new RestaurantSettings { Id = 4, SettingKey = SettingKeys.OpeningTime, SettingValue = "11:00", Description = "Restaurant opening time" },
                new RestaurantSettings { Id = 5, SettingKey = SettingKeys.ClosingTime, SettingValue = "22:00", Description = "Restaurant closing time" },
                new RestaurantSettings { Id = 6, SettingKey = SettingKeys.BookingAdvanceDays, SettingValue = "30", Description = "How many days in advance bookings are allowed" }
            );
        }
    }
}

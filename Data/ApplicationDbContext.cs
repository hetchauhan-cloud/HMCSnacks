using Microsoft.EntityFrameworkCore;
using HMCSnacks.Models;

namespace HMCSnacks.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<UserOTP> UserOTPs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<State>(entity =>
            {
                entity.ToTable("states");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("state_id");
                entity.Property(e => e.StateName).HasColumnName("state_name");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            });

            modelBuilder.Entity<City>(entity =>
            {
                entity.ToTable("cities");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("city_id");
                entity.Property(e => e.CityName).HasColumnName("city_name");
                entity.Property(e => e.StateId).HasColumnName("state_id");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.CreatedDate).HasColumnName("created_date");

                entity.HasOne(e => e.State)
                      .WithMany()
                      .HasForeignKey(e => e.StateId);
            });

            modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany() // If Product does not have a navigation to OrderItems
            .HasForeignKey(oi => oi.ProductId);
        }


    }
}

using Exam.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Exam.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Artists> Artists => Set<Artists>();
        public DbSet<Customers> Customers => Set<Customers>();
        public DbSet<Genres> Genres => Set<Genres>();
        public DbSet<Plates> Plates => Set<Plates>();
        public DbSet<PromotionPlates> PromotionPlates => Set<PromotionPlates>();
        public DbSet<Promotions> Promotions => Set<Promotions>();
        public DbSet<Publisher> Publishers => Set<Publisher>();
        public DbSet<Reservations> Reservations => Set<Reservations>();
        public DbSet<SaleItems> SaleItems => Set<SaleItems>();
        public DbSet<Sales> Sales => Set<Sales>();
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();
        public DbSet<Users> Users => Set<Users>();

        // Creates a database context instance.
        public ApplicationContext()
        {
        }

        // Configures the SQL Server connection when options were not supplied externally.
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            string? connectionString =
                config.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);
        }

        // Defines entity relationships, constraints, indexes, and seed data.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PromotionPlates>()
                .HasKey(x => new
                {
                    x.PromotionId,
                    x.PlateId
                });

            modelBuilder.Entity<PromotionPlates>()
                .HasOne(x => x.Promotion)
                .WithMany(x => x.PromotionPlates)
                .HasForeignKey(x => x.PromotionId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Users>(entity =>
            {
                entity.Property(x => x.Login).HasMaxLength(64);
                entity.Property(x => x.PasswordHash).HasMaxLength(512);
                entity.Property(x => x.Role).HasMaxLength(32);
                entity.HasIndex(x => x.Login).IsUnique();
            });

            modelBuilder.Entity<Plates>(entity =>
            {
                entity.Property(x => x.Title).HasMaxLength(200);
                entity.Property(x => x.RowVersion).IsRowVersion();
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Plates_Quantity", "[Quantity] >= 0");
                    t.HasCheckConstraint("CK_Plates_TrackCount", "[TrackCount] > 0");
                    t.HasCheckConstraint("CK_Plates_ReleaseYear", "[ReleaseYear] BETWEEN 1877 AND 2100");
                    t.HasCheckConstraint("CK_Plates_Prices", "[CostPrise] >= 0 AND [SalePrise] >= 0");
                });
            });

            modelBuilder.Entity<Artists>(entity =>
                entity.Property(x => x.Name).HasMaxLength(150));
            modelBuilder.Entity<Genres>(entity =>
                entity.Property(x => x.Name).HasMaxLength(100));
            modelBuilder.Entity<Publisher>(entity =>
                entity.Property(x => x.Name).HasMaxLength(150));
            modelBuilder.Entity<Customers>(entity =>
            {
                entity.Property(x => x.Name).HasMaxLength(150);
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Customers_TotalSpent", "[TotalSpent] >= 0"));
            });

            modelBuilder.Entity<Promotions>(entity =>
            {
                entity.Property(x => x.Name).HasMaxLength(150);
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Promotions_Discount", "[DiscountPercent] > 0 AND [DiscountPercent] <= 100");
                    t.HasCheckConstraint("CK_Promotions_Dates", "[EndDate] >= [StartDate]");
                });
            });

            modelBuilder.Entity<Reservations>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_Reservations_Quantity", "[Quantity] > 0");
                t.HasCheckConstraint("CK_Reservations_Fulfilled", "[FulfilledQuantity] >= 0 AND [FulfilledQuantity] <= [Quantity]");
                t.HasCheckConstraint("CK_Reservations_Dates", "[ExpiresAt] >= [ReservedAt]");
                t.HasCheckConstraint("CK_Reservations_Status", "[Status] BETWEEN 1 AND 4");
            });

            modelBuilder.Entity<SaleItems>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_SaleItems_Quantity", "[Quantity] > 0");
                t.HasCheckConstraint("CK_SaleItems_UnitPrice", "[UnitPrise] >= 0");
                t.HasCheckConstraint("CK_SaleItems_Discount", "[DiscountPercent] >= 0 AND [DiscountPercent] <= 100");
            });
            modelBuilder.Entity<Sales>().ToTable(t =>
                t.HasCheckConstraint("CK_Sales_TotalAmount", "[TotalAmount] >= 0"));
            modelBuilder.Entity<StockMovement>(entity =>
            {
                entity.Property(x => x.Reason).HasMaxLength(500);
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_StockMovements_Quantity", "[QuantityChange] <> 0");
                    t.HasCheckConstraint("CK_StockMovements_Type", "[MovementType] BETWEEN 1 AND 3");
                });
            });

            modelBuilder.Entity<Plates>()
                .HasOne(x => x.Artist).WithMany(x => x.Plates)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Plates>()
                .HasOne(x => x.Genre).WithMany(x => x.Plates)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Plates>()
                .HasOne(x => x.Publisher).WithMany(x => x.Plates)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Reservations>()
                .HasOne(x => x.Plate).WithMany(x => x.Reservations)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SaleItems>()
                .HasOne(x => x.Plate).WithMany(x => x.SaleItems)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<StockMovement>()
                .HasOne(x => x.Plate).WithMany(x => x.StockMovements)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Sales>()
                .HasOne(x => x.Customer).WithMany(x => x.Sales)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Reservations>()
                .HasOne(x => x.Customer).WithMany(x => x.Reservations)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PromotionPlates>()
                .HasOne(x => x.Plate)
                .WithMany(x => x.PromotionPlates)
                .HasForeignKey(x => x.PlateId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

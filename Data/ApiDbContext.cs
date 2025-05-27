using CWSERVER.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Data
{
    public class ApiDbContext : IdentityDbContext<User>
    {
        public ApiDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Store> Stores { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Store>().ToTable("Store");
            modelBuilder.Entity<Product>().ToTable("Product");
            modelBuilder.Entity<Category>().ToTable("Category");
            modelBuilder.Entity<Employee>().ToTable("Employee");
            modelBuilder.Entity<Customer>().ToTable("Customer");
            modelBuilder.Entity<Order>().ToTable("Order");
            modelBuilder.Entity<OrderItem>().ToTable("OrderItem");
            modelBuilder.Entity<RefreshToken>().ToTable("RefreshToken");

            modelBuilder.Entity<Product>()
                .Property(p => p.ProductId)
                .UseIdentityColumn(seed: 100000, increment: 1);
            modelBuilder.Entity<ProductImage>().ToTable("ProductImage");
            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.AdditionalImages)
                .HasForeignKey(pi => pi.ProductId);

           
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<IdentityRole>().ToTable("Role");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRole");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaim");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogin");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserToken");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaim");
        }
    }
}

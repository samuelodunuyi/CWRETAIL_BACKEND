using CW_RETAIL.Models.Core;
using CW_RETAIL.Models.Industries;
using Microsoft.EntityFrameworkCore;

namespace CW_RETAIL.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        
        // Restaurant Industry Models
        public DbSet<RestaurantProduct> RestaurantProducts { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        
        // Order Models
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User and UserRole relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            // Configure Employee relationships
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Store)
                .WithMany()
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configure RestaurantProduct as TPH inheritance
            modelBuilder.Entity<RestaurantProduct>().ToTable("RestaurantProducts");
            
            // Configure Recipe relationships
            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Recipes)
                .HasForeignKey(r => r.ProductId);
                
            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.Store)
                .WithMany()
                .HasForeignKey(r => r.StoreId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configure RecipeIngredient relationships
            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Recipe)
                .WithMany(r => r.Ingredients)
                .HasForeignKey(ri => ri.RecipeId);
                
            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Ingredient)
                .WithMany()
                .HasForeignKey(ri => ri.IngredientId);
                
            // Configure Order relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Store)
                .WithMany()
                .HasForeignKey(o => o.StoreId);
                
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .IsRequired(false);
                
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            // Seed UserRoles
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { Id = UserRole.SuperAdmin, Name = "SuperAdmin" },
                new UserRole { Id = UserRole.StoreAdmin, Name = "StoreAdmin" },
                new UserRole { Id = UserRole.Employee, Name = "Employee" },
                new UserRole { Id = UserRole.Customer, Name = "Customer" }
            );
        }
    }
}
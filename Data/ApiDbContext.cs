using CWSERVER.Models.Core.Entities;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Data
{
    public class ApiDbContext(DbContextOptions options) : IdentityDbContext<User>(options)
    {
        public DbSet<Store> Stores { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<ProductImage> ProductImages { get; set; }

        // New Industry Models DBSet 
        public DbSet<AuditLogs> AuditLogs { get; set; }
        public DbSet<Categories> Categoriess { get; set; }
        public DbSet<Ingredients> Ingredientss { get; set; }
        public DbSet<NutritionalInfo> NutritionalInfos { get; set; }
        public DbSet<Permission> Permissionss { get; set; }
        public DbSet<Products> Productss { get; set; }
        public DbSet<ProductVariants> ProductVariantss { get; set; }
        public DbSet<Profiles> Profiless { get; set; }
        public DbSet<RecipeIngredients> RecipeIngredientss { get; set; }
        public DbSet<Recipes> Recipess { get; set; }
        public DbSet<Stores> Storess { get; set; }
        public DbSet<UserRoles> UserRoless { get; set; }

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

            // New Industry Models DBSet 
            modelBuilder.Entity<AuditLogs>().ToTable("AuditLogs");
            modelBuilder.Entity<Categories>().ToTable("Categories");
            modelBuilder.Entity<Ingredients>().ToTable("Ingredients");
            modelBuilder.Entity<NutritionalInfo>().ToTable("NutritionalInfo");
            modelBuilder.Entity<Permission>().ToTable("Permissions");
            modelBuilder.Entity<Products>().ToTable("Products");
            modelBuilder.Entity<ProductVariants>().ToTable("ProductVariants");
            modelBuilder.Entity<Profiles>().ToTable("Profiles");
            modelBuilder.Entity<RecipeIngredients>().ToTable("RecipeIngredients");
            modelBuilder.Entity<Recipes>().ToTable("Recipes");
            modelBuilder.Entity<Stores>().ToTable("Stores");
            modelBuilder.Entity<UserRoles>().ToTable("UserRoles");

            modelBuilder.Entity<AuditLogs>()
                .HasKey(a => a.AuditLogId);
            modelBuilder.Entity<Categories>()
                .HasKey(a => a.CategoryId);
            modelBuilder.Entity<Ingredients>()
                .HasKey(a => a.IngredientId);
            modelBuilder.Entity<NutritionalInfo>()
                .HasKey(a => a.NutritionalInfoId);
            modelBuilder.Entity<Permission>()
                .HasKey(a => a.PermissionId);
            modelBuilder.Entity<Products>()
                .HasKey(a => a.ProductId);
            modelBuilder.Entity<ProductVariants>()
                .HasKey(a => a.ProductVariantId);
            modelBuilder.Entity<Profiles>()
                .HasKey(a => a.ProfileId);
            modelBuilder.Entity<RecipeIngredients>()
                .HasKey(a => a.RecipeIngredientsId);
            modelBuilder.Entity<Recipes>()
                .HasKey(a => a.RecipeId);
            modelBuilder.Entity<Stores>()
                .HasKey(a => a.StoresId);
            modelBuilder.Entity<UserRoles>()
                .HasKey(a => a.UserRolesId);

            modelBuilder.Entity<Product>()
                .Property(p => p.ProductId)
                .UseIdentityColumn(seed: 100000, increment: 1);

            modelBuilder.Entity<Product>()
                .Property(p => p.ProductPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.ProductOriginalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductImage>().ToTable("ProductImage");

            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.AdditionalImages)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure foreign key relationships with proper constraints
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Store)
                .WithMany()
                .HasForeignKey(p => p.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Store)
                .WithMany()
                .HasForeignKey(c => c.StoreId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Store)
                .WithMany()
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Store)
                .WithMany()
                .HasForeignKey(o => o.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.OriginalPriceAtOrder)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.PriceAtOrder)
                .HasPrecision(18, 2);

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

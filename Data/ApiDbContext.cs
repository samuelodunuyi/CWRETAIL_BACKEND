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
        public DbSet<AuditLogs> AuditLogss { get; set; }
        public DbSet<Categories> Categoriess { get; set; }
        public DbSet<Ingredients> Ingredientss { get; set; }
        public DbSet<NutritionalInfo> NutritionalInfos { get; set; }
        public DbSet<Permissions> Permissionss { get; set; }
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
            modelBuilder.Entity<Permissions>().ToTable("Permissions");
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
            modelBuilder.Entity<Permissions>()
                .HasKey(a => a.PermissionId);
            modelBuilder.Entity<Products>()
                .HasKey(a => a.ProductId);
            modelBuilder.Entity<ProductVariants>()
                .HasKey(a => a.ProductVariantId);
            modelBuilder.Entity<Profiles>()
                .HasKey(a => a.ProfileId);
            modelBuilder.Entity<RecipeIngredients>()
                .HasKey(a => a.RecipeId);
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
                .HasForeignKey(pi => pi.ProductId);

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

using CWSERVER.Data;
using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.DTOs.ProductVariants;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Respository.Industry.Restaurant
{
    public class ProductVariantsRepository(ApiDbContext dbContext): IProductVariants
    {
        private readonly ApiDbContext _dbContext = dbContext;

        public async Task<ProductVariants?> CreateProductVariants(ProductVariants productVariants)
        {
            await _dbContext.ProductVariantss.AddAsync(productVariants);
            await _dbContext.SaveChangesAsync();
            return productVariants;
        }

        public async Task<ProductVariants?> DeleteProductVariantsAsync(int id)
        {
            var variants = await _dbContext.ProductVariantss.FirstOrDefaultAsync(v => v.ProductVariantId == id);

            if (variants == null) return null;

            _dbContext.ProductVariantss.Remove(variants);
            return variants;
        }

        public async Task<List<ProductVariants>> GetAllProductVariantsAsync()
        {
            return await _dbContext.ProductVariantss.ToListAsync();
        }

        public async Task<ProductVariants?> GetProductVariantsByIdAsync(int id)
        {
            var variants = await _dbContext.ProductVariantss.FirstOrDefaultAsync(v => v.ProductVariantId == id);

            if (variants == null) return null;
            return variants;
        }

        public async Task<ProductVariants?> UpdateProductVariantsAsync(int id, UpdateProductVariants updateProductVariants)
        {
            var variants = await _dbContext.ProductVariantss.FirstOrDefaultAsync(v => v.ProductVariantId == id);

            if (variants == null) return null;

            variants.ProductId = updateProductVariants.ProductId;
            variants.VariantName = updateProductVariants.VariantName;
            variants.VariantType = updateProductVariants.VariantType;
            variants.PriceModifier = updateProductVariants.PriceModifier;
            variants.IsDefault = updateProductVariants.IsDefault;
            variants.IsAvailable = updateProductVariants.IsAvailable;
            variants.DisplayOrder = updateProductVariants.DisplayOrder;
            variants.UpdatedAt = updateProductVariants.UpdatedAt;

            await _dbContext.SaveChangesAsync();
            return variants;
        }
    }
}

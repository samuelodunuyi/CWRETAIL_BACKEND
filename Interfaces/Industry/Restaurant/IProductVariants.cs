using CWSERVER.Models.Industries.Restaurant.DTOs.ProductVariants;
using CWSERVER.Models.Industries.Restaurant.Entities;

namespace CWSERVER.Interfaces.Industry.Restaurant
{
    public interface IProductVariants
    {
        Task<List<ProductVariants>> GetAllProductVariantsAsync();
        Task<ProductVariants?> GetProductVariantsByIdAsync(int id);
        Task<ProductVariants?> CreateProductVariants(ProductVariants productVariants);
        Task<ProductVariants?> UpdateProductVariantsAsync(int id, UpdateProductVariants updateProductVariants);  
        Task<ProductVariants?> DeleteProductVariantsAsync(int id);
    }
}

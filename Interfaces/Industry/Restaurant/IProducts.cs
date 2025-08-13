using CWSERVER.Models.Industries.Restaurant.DTOs.Products;
using CWSERVER.Models.Industries.Restaurant.Entities;

namespace CWSERVER.Interfaces.Industry.Restaurant
{
    public interface IProducts
    {
        Task<List<Products>> GetAllProductsAsync();
        Task<Products?> GetProductsByIdAsync(int id);
        Task<Products?> CreateProductsAsync(Products products);
        Task<Products?> UpdateProductsAsync(int id, UpdateProductsDTO updateProductsDTO);
        Task<Products?> DeleteProductsAsync(int id);
    }
}

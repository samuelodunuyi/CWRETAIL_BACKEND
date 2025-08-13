using CWSERVER.Data;
using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.DTOs.Products;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Respository.Industry.Restaurant
{
    public class ProductsRepository(ApiDbContext dbContext) : IProducts
    {
        private readonly ApiDbContext _dbContext = dbContext;
        public async Task<Products?> CreateProductsAsync(Products products)
        {
            await _dbContext.Productss.AddAsync(products);
            await _dbContext.SaveChangesAsync();
            return products;
        }

        public async Task<Products?> DeleteProductsAsync(int id)
        {
            var product = await _dbContext.Productss.FirstOrDefaultAsync(pro => pro.ProductId == id);

            if (product == null)
                return null;

            _dbContext.Productss.Remove(product);
            await _dbContext.SaveChangesAsync();
            return product;
        }

        public async Task<List<Products>> GetAllProductsAsync()
        {
            return await _dbContext.Productss.ToListAsync();
        }

        public async Task<Products?> GetProductsByIdAsync(int id)
        {
            var product = await _dbContext.Productss.FirstOrDefaultAsync(pro => pro.ProductId == id);

            if (product == null) return null;

            return product;
        }

        public async Task<Products?> UpdateProductsAsync(int id, UpdateProductsDTO updateProductsDTO)
        {
            var product = await _dbContext.Productss.FirstOrDefaultAsync(pro => pro.ProductId == id);

            if (product == null) return null;

            product.Name = updateProductsDTO.Name;
            product.Description = updateProductsDTO.Description;
            product.SKU = updateProductsDTO.SKU;
            product.Barcode = updateProductsDTO.Barcode;
            product.CategoryId = updateProductsDTO.CategoryId;
            product.StoreId = updateProductsDTO.StoreId;
            product.BasePrice = updateProductsDTO.BasePrice;
            product.CostPrice = updateProductsDTO.CostPrice;
            product.CurrentStock = updateProductsDTO.CurrentStock;
            product.MinimumStockLevel = updateProductsDTO.MinimumStockLevel;
            product.MaximumStockLevel = updateProductsDTO.MaximumStockLevel;
            product.UnitOfMeasure = updateProductsDTO.UnitOfMeasure;
            product.IsRecipe = updateProductsDTO.IsRecipe;
            product.PrepTimeMinutes = updateProductsDTO.PrepTimeMinutes;
            product.CookingTimeMinutes = updateProductsDTO.CookingTimeMinutes;
            product.Allergens = updateProductsDTO.Allergens;
            product.SpiceLevel = updateProductsDTO.SpiceLevel;
            product.IsVegetarian = updateProductsDTO.IsVegetarian;
            product.IsVegan = updateProductsDTO.IsVegan;
            product.IsGlutenFree = updateProductsDTO.IsGlutenFree;
            product.Status = updateProductsDTO.Status;
            product.ImageUrl = updateProductsDTO.ImageUrl;
            product.UpdatedAt = updateProductsDTO.UpdatedAt;

            await _dbContext.SaveChangesAsync();
            return product;
        }
    }
}

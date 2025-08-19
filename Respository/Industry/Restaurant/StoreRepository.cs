using CWSERVER.Data;
using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.DTOs.Stores;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Respository.Industry.Restaurant
{
    public class StoreRepository(ApiDbContext dbContext): IStores
    {
        private readonly ApiDbContext _dbContext = dbContext;

        public async Task<Stores?> CreateStoresAsync(Stores stores)
        {
            await _dbContext.Storess.AddAsync(stores);
            await _dbContext.SaveChangesAsync();
            return stores;
        }

        public async Task<Stores?> DeleteStoresByIdAsync(int id)
        {
            var store = await _dbContext.Storess.FirstOrDefaultAsync(s => s.StoresId == id);

            if (store == null) return null;

            _dbContext.Storess.Remove(store);
            await _dbContext.SaveChangesAsync();
            return store;
        }

        public async Task<List<Stores>> GetAllStoresAsync()
        {
            return await _dbContext.Storess.ToListAsync();
        }

        public async Task<Stores?> GetStoresByIdAsync(int id)
        {
            var store = await _dbContext.Storess.FirstOrDefaultAsync(s => s.StoresId == id);

            if (store == null) return null;
            return store;
        }

        public async Task<Stores?> UpdateStoresAsync(int id, UpdateStoreDTO updateStoreDTO)
        {
            var store = await _dbContext.Storess.FirstOrDefaultAsync(s => s.StoresId == id);

            if (store == null) return null;

            store.Name = updateStoreDTO.Name;
            store.Location = updateStoreDTO.Location;
            store.Phone = updateStoreDTO.Phone;
            store.Email = updateStoreDTO.Email;
            store.Status = updateStoreDTO.Status;
            store.UpdatedAt = updateStoreDTO.UpdatedAt;

            await _dbContext.SaveChangesAsync();
            return store;
        }
    }
}

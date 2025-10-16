using CWSERVER.Models.Industries.Restaurant.DTOs.Stores;
using CWSERVER.Models.Industries.Restaurant.Entities;

namespace CWSERVER.Interfaces.Industry.Restaurant
{
    public interface IStores
    {
        Task<List<Stores>> GetAllStoresAsync();
        Task<Stores?> GetStoresByIdAsync(int id);
        Task<Stores?> CreateStoresAsync(Stores stores);
        Task<Stores?> UpdateStoresAsync(int id, UpdateStoreDTO updateStoreDTO);
        Task<Stores?> DeleteStoresByIdAsync(int id);
    }
}

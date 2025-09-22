using CWSERVER.Data;
using CWSERVER.Models.Core.Entities;
using CWSERVER.Models.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Controllers.Core
{
    [Route("api/core/[controller]")]
    [ApiController]
    public class StoreController(ApiDbContext dbContext) : ControllerBase
    {
        private readonly ApiDbContext dbContext = dbContext;

        [HttpGet]
        public async Task<IActionResult> GetAllStores([FromQuery] string? name)
        {
            var query = dbContext.Stores.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(s => s.StoreName!.Contains(name));

            var stores = await query.ToListAsync();
            var storeResponseDtos = stores.Select(s => new StoreResponseDTO
            {
                StoreId = s.StoreId,
                StoreName = s.StoreName,
                StoreRep = s.StoreRep,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedBy = s.UpdatedBy
            }).ToList();

            return Ok(storeResponseDtos);
        }

      
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStoreById(int id)
        {
            var store = await dbContext.Stores.FirstOrDefaultAsync(s => s.StoreId == id);
            if (store == null) return NotFound();

            var storeResponseDto = new StoreResponseDTO
            {
                StoreId = store.StoreId,
                StoreName = store.StoreName,
                StoreRep = store.StoreRep,
                CreatedAt = store.CreatedAt,
                UpdatedAt = store.UpdatedAt,
                CreatedBy = store.CreatedBy,
                UpdatedBy = store.UpdatedBy
            };

            return Ok(storeResponseDto);
        }

       
        [HttpPost]
        public async Task<IActionResult> CreateStore([FromBody] StoreCreateDTO storeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var store = new Store
            {
                StoreName = storeDto.StoreName,
                StoreRep = storeDto.StoreRep,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System" // TODO: Get from authenticated user
            };

            dbContext.Stores.Add(store);
            await dbContext.SaveChangesAsync();

            var storeResponseDto = new StoreResponseDTO
            {
                StoreId = store.StoreId,
                StoreName = store.StoreName,
                StoreRep = store.StoreRep,
                CreatedAt = store.CreatedAt,
                UpdatedAt = store.UpdatedAt,
                CreatedBy = store.CreatedBy,
                UpdatedBy = store.UpdatedBy
            };

            return CreatedAtAction(nameof(GetStoreById), new { id = store.StoreId }, storeResponseDto);
        }

      
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStore(int id, [FromBody] StoreUpdateDTO storeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var store = await dbContext.Stores.FirstOrDefaultAsync(s => s.StoreId == id);
            if (store == null) return NotFound();

            store.StoreName = storeDto.StoreName;
            store.StoreRep = storeDto.StoreRep;
            store.UpdatedAt = DateTime.UtcNow;
            store.UpdatedBy = "System"; // TODO: Get from authenticated user

            await dbContext.SaveChangesAsync();

            var storeResponseDto = new StoreResponseDTO
            {
                StoreId = store.StoreId,
                StoreName = store.StoreName,
                StoreRep = store.StoreRep,
                CreatedAt = store.CreatedAt,
                UpdatedAt = store.UpdatedAt,
                CreatedBy = store.CreatedBy,
                UpdatedBy = store.UpdatedBy
            };

            return Ok(storeResponseDto);
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStore(int id)
        {
            var store = await dbContext.Stores.FirstOrDefaultAsync(s => s.StoreId == id);
            if (store == null) return NotFound();

            dbContext.Stores.Remove(store);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}

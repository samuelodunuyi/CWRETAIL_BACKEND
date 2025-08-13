using CWSERVER.Data;
using CWSERVER.Models.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController(ApiDbContext dbContext) : ControllerBase
    {
        private readonly ApiDbContext dbContext = dbContext;

        [HttpGet]
        public IActionResult GetAllStores([FromQuery] string? name)
        {
            var query = dbContext.Stores.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(s => s.StoreName!.Contains(name));

            return Ok(query.ToList());
        }

      
        [HttpGet("{id}")]
        public IActionResult GetStoreById(int id)
        {
            var store = dbContext.Stores.Find(id);
            if (store == null) return NotFound();
            return Ok(store);
        }

       
        [HttpPost]
        public IActionResult CreateStore([FromBody] Store store)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dbContext.Stores.Add(store);
            dbContext.SaveChanges();
            return CreatedAtAction(nameof(GetStoreById), new { id = store.StoreId }, store);
        }

      
        [HttpPut("{id}")]
        public IActionResult UpdateStore(int id, [FromBody] Store updatedStore)
        {
            if (id != updatedStore.StoreId)
                return BadRequest("Store ID mismatch.");

            var store = dbContext.Stores.Find(id);
            if (store == null) return NotFound();

            dbContext.Entry(store).CurrentValues.SetValues(updatedStore);
            dbContext.SaveChanges();
            return NoContent();
        }

        
        [HttpDelete("{id}")]
        public IActionResult DeleteStore(int id)
        {
            var store = dbContext.Stores.Find(id);
            if (store == null) return NotFound();

            dbContext.Stores.Remove(store);
            dbContext.SaveChanges();
            return NoContent();
        }
    }
}

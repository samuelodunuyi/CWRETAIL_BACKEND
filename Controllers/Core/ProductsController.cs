using CW_RETAIL.Data;
using CW_RETAIL.Models.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace CW_RETAIL.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int? storeId = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool? showInWeb = null,
            [FromQuery] string? search = null)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Store)
                .AsQueryable();

            // Base filters
            if (storeId.HasValue)
            {
                query = query.Where(p => p.StoreId == storeId.Value);
            }
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p => p.ProductName.Contains(term) || (p.Barcode != null && p.Barcode.Contains(term)) || (p.SKU != null && p.SKU.Contains(term)));
            }

            // Enforce visibility and permissions
            var isAuthenticated = User?.Identity?.IsAuthenticated == true;
            if (!isAuthenticated)
            {
                // Anonymous users can only see active products that are shown on the web
                query = query.Where(p => p.IsActive && p.ShowInWeb);
            }
            else
            {
                var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
                var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
                int roleValue;
                int.TryParse(currentUserRole, out roleValue);

                if (roleValue == UserRole.StoreAdmin || currentUserRole == UserRole.StoreAdmin.ToString())
                {
                    var store = await _context.Stores.FirstOrDefaultAsync(s => s.StoreAdmin == currentUserEmail);
                    if (store == null)
                    {
                        return Forbid();
                    }

                    // Restrict to the admin's store
                    query = query.Where(p => p.StoreId == store.StoreId);

                    // If a different storeId was requested, forbid
                    if (storeId.HasValue && storeId.Value != store.StoreId)
                    {
                        return Forbid();
                    }
                }
                else if (roleValue == UserRole.Employee || currentUserRole == UserRole.Employee.ToString())
                {
                    var storeIds = await _context.Employees
                        .Where(e => e.User.Email == currentUserEmail)
                        .Select(e => e.StoreId)
                        .ToListAsync();
                    if (storeIds == null || storeIds.Count == 0)
                    {
                        return Forbid();
                    }
                    query = query.Where(p => storeIds.Contains(p.StoreId));

                    if (storeId.HasValue && !storeIds.Contains(storeId.Value))
                    {
                        return Forbid();
                    }
                }
                else if (roleValue == UserRole.Customer || currentUserRole == UserRole.Customer.ToString())
                {
                    // Customers only see public products
                    query = query.Where(p => p.IsActive && p.ShowInWeb);
                }
                // SuperAdmin sees all; no extra restriction

                // Apply explicit filters only for authenticated users
                if (isActive.HasValue)
                {
                    query = query.Where(p => p.IsActive == isActive.Value);
                }
                if (showInWeb.HasValue)
                {
                    query = query.Where(p => p.ShowInWeb == showInWeb.Value);
                }
            }

            // Shape public response: only product-related columns
            var products = await query.Select(p => new ProductPublicDTO
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                SKU = p.SKU,
                Barcode = p.Barcode,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.CategoryName : null,
                StoreId = p.StoreId,
                BasePrice = p.BasePrice,
                CurrentStock = p.CurrentStock,
                UnitOfMeasure = p.UnitOfMeasure,
                ImageUrl = p.ImageUrl,
                AdditionalImages = p.AdditionalImages,
                ShowInWeb = p.ShowInWeb,
                ShowInPOS = p.ShowInPOS,
                IsActive = p.IsActive,
                Store = new StoreBasicInfoDTO
                {
                    StoreId = p.StoreId,
                    StoreName = p.Store != null ? p.Store.StoreName : null,
                    StoreType = p.Store != null ? p.Store.StoreType : null,
                    IsActive = p.Store != null && p.Store.IsActive
                }
            }).ToListAsync();

            return Ok(products);
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProduct(int id)
        {
            var isAuthenticated = User?.Identity?.IsAuthenticated == true;
            var query = _context.Products
                .Include(x => x.Category)
                .Include(x => x.Store)
                .Where(x => x.ProductId == id)
                .AsQueryable();

            if (!isAuthenticated)
            {
                query = query.Where(x => x.IsActive && x.ShowInWeb);
            }
            else
            {
                var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
                var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
                int roleValue;
                int.TryParse(currentUserRole, out roleValue);

                if (roleValue == UserRole.StoreAdmin || currentUserRole == UserRole.StoreAdmin.ToString())
                {
                    var store = await _context.Stores.FirstOrDefaultAsync(s => s.StoreAdmin == currentUserEmail);
                    if (store == null)
                    {
                        return Forbid();
                    }
                    query = query.Where(x => x.StoreId == store.StoreId);
                }
                else if (roleValue == UserRole.Employee || currentUserRole == UserRole.Employee.ToString())
                {
                    var storeIds = await _context.Employees
                        .Where(e => e.User.Email == currentUserEmail)
                        .Select(e => e.StoreId)
                        .ToListAsync();
                    if (storeIds == null || storeIds.Count == 0)
                    {
                        return Forbid();
                    }
                    query = query.Where(x => storeIds.Contains(x.StoreId));
                }
                else if (roleValue == UserRole.Customer || currentUserRole == UserRole.Customer.ToString())
                {
                    query = query.Where(x => x.IsActive && x.ShowInWeb);
                }
                // SuperAdmin sees all
            }

            var p = await query.FirstOrDefaultAsync();

            if (p == null)
            {
                return NotFound();
            }

            var dto = new ProductPublicDTO
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                SKU = p.SKU,
                Barcode = p.Barcode,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.CategoryName : null,
                StoreId = p.StoreId,
                BasePrice = p.BasePrice,
                CurrentStock = p.CurrentStock,
                UnitOfMeasure = p.UnitOfMeasure,
                ImageUrl = p.ImageUrl,
                AdditionalImages = p.AdditionalImages,
                ShowInWeb = p.ShowInWeb,
                ShowInPOS = p.ShowInPOS,
                IsActive = p.IsActive,
                Store = new StoreBasicInfoDTO
                {
                    StoreId = p.StoreId,
                    StoreName = p.Store != null ? p.Store.StoreName : null,
                    StoreType = p.Store != null ? p.Store.StoreType : null,
                    IsActive = p.Store != null && p.Store.IsActive
                }
            };

            return Ok(dto);
        }
    }
}
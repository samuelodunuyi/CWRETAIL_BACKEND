namespace CW_RETAIL.Models.Core
{
    public class CategoryPublicDTO
    {
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public string? CategoryIcon { get; set; }
        public int? StoreId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
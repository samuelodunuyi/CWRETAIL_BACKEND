using System.ComponentModel.DataAnnotations;

namespace CWSERVER.Models.Entities
{
    public class Store
    {
        public int StoreId { get; set; }
        [Required]
        public string StoreName { get; set; }

        public string? StoreRep { get; set; }
    }
}

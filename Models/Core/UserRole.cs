using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CW_RETAIL.Models.Core
{
    public class UserRole
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        
        // Role constants
        public const int SuperAdmin = 0;
        public const int StoreAdmin = 1;
        public const int Employee = 2;
        public const int Customer = 3;
    }
}
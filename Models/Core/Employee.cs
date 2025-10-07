using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CW_RETAIL.Models.Core
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required] 
        public int UserId { get; set; } 
        public virtual User? User { get; set; }

        [ForeignKey("Store")]
        public int StoreId { get; set; }
        public virtual Store? Store { get; set; }
    }
}
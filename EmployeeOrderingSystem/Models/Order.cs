using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeOrderingSystem.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public string Status { get; set; } // "Pending", "Preparing", etc.

        public DateTime? StatusLastUpdated { get; set; }
        public DateTime? ExpectedDelivery { get; set; }

        // Navigation: Order has many items
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}

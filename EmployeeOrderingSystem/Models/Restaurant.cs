using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EmployeeOrderingSystem.Models
{
    public class Restaurant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string LocationDescription { get; set; }
        public string ContactNumber { get; set; }

        // Navigation: Restaurant can have many menu items
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}

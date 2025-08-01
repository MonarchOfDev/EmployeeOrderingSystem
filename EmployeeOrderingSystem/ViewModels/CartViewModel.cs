using System.Collections.Generic;
using System.Linq;

namespace EmployeeOrderingSystem.ViewModels
{
    public class CartViewModel
    {
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal CartTotal => Items.Sum(i => i.Total);
    }
}

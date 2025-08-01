using System;
using System.Collections.Generic;

namespace EmployeeOrderingSystem.ViewModels
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime? StatusLastUpdated { get; set; }
        public DateTime? ExpectedDelivery { get; set; }
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
    }
}

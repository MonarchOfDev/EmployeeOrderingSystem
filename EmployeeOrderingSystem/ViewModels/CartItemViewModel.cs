namespace EmployeeOrderingSystem.ViewModels
{
    public class CartItemViewModel
    {
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Quantity * UnitPrice;
    }
}

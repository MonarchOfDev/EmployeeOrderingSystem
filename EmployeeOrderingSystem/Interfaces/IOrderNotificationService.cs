namespace EmployeeOrderingSystem.Interfaces
{
    public interface IOrderNotificationService
    {
        Task NotifyOrderPlacedAsync(string email, int orderId);
    }
}

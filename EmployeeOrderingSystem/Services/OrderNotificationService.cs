using EmployeeOrderingSystem.Interfaces;

namespace EmployeeOrderingSystem.Services
{
    public class OrderNotificationService : IOrderNotificationService
    {
        public async Task NotifyOrderPlacedAsync(string email, int orderId)
        {
            // Replace with actual email logic or logging as needed
            Console.WriteLine($"Notification: Order {orderId} placed by {email}");
            await Task.CompletedTask;
        }
    }
}

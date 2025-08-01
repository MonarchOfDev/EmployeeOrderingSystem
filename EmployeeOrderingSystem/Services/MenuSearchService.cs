using EmployeeOrderingSystem.Interfaces;
using EmployeeOrderingSystem.Models;

namespace EmployeeOrderingSystem.Services
{
    public class MenuSearchService : IMenuSearchService
    {
        public IEnumerable<MenuItem> SearchMenuItems(IEnumerable<MenuItem> menuItems, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return menuItems;

            keyword = keyword.ToLowerInvariant();
            return menuItems.Where(m =>
                (m.Name?.ToLowerInvariant().Contains(keyword) ?? false) ||
                (m.Description?.ToLowerInvariant().Contains(keyword) ?? false));
        }
    }
}

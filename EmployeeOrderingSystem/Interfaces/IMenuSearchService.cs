using EmployeeOrderingSystem.Models;

namespace EmployeeOrderingSystem.Interfaces
{
    public interface IMenuSearchService
    {
        IEnumerable<MenuItem> SearchMenuItems(IEnumerable<MenuItem> menuItems, string keyword);
    }
}

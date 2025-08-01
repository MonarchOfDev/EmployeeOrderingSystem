using EmployeeOrderingSystem.Interfaces;

namespace EmployeeOrderingSystem.Services
{
    public class BonusService : IBonusService
    {
        public decimal CalculateBonus(decimal depositAmount)
        {
            // R500 bonus for each full R250 deposited
            if (depositAmount < 250) return 0;
            var bonusBlocks = (int)(depositAmount / 250);
            return bonusBlocks * 500m;
        }
    }
}

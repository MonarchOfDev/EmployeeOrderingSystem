namespace EmployeeOrderingSystem.Interfaces
{
    public interface IBonusService
    {
        /// <summary>
        /// Calculates bonus for deposit.
        /// </summary>
        /// <param name="depositAmount">Deposit amount.</param>
        /// <returns>Bonus to apply.</returns>
        decimal CalculateBonus(decimal depositAmount);
    }
}

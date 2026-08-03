namespace Dsw2026Tpi.Domain.Interfaces;

//Provee información sobre feriados y días no laborales en los que no deben generarse disponibilidades.

public interface IHolidayProvider
{
    bool IsNonWorkingDay(DateOnly date);
}
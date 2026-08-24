using System.Globalization;
using System.Text.Json;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Data.Providers;


// Obtiene los feriados y días no laborales desde Sources/holidays.json.

public sealed class HolidayProvider : IHolidayProvider
{
    private readonly HashSet<DateOnly> _nonWorkingDays;

    public HolidayProvider()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "Sources", "holidays.json");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("No se encontró el archivo de feriados", filePath);
        }

        var json = File.ReadAllText(filePath);

        var entries = JsonSerializer.Deserialize<List<HolidayEntry>>(json,new JsonSerializerOptions{PropertyNameCaseInsensitive = true})?? new List<HolidayEntry>();

        _nonWorkingDays = new HashSet<DateOnly>();

        foreach (var entry in entries)
        {
            if (!DateOnly.TryParseExact(entry.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                throw new InvalidOperationException($"La fecha '{entry.Date}' del archivo holidays.json es inválida. Debe utilizar el formato yyyy-MM-dd");
            }

            if (!_nonWorkingDays.Add(date))
            {
                throw new InvalidOperationException($"La fecha '{entry.Date}' está repetida en el archivo holidays.json.");
            }
        }
    }

    public bool IsNonWorkingDay(DateOnly date)
    {
        return _nonWorkingDays.Contains(date);
    }

    private sealed class HolidayEntry
    {
        public string Date { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
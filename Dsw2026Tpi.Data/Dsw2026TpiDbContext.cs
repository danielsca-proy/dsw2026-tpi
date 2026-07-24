using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Dsw2026Tpi.Data;

public class Dsw2026TpiDbContext: DbContext
{
    public Dsw2026TpiDbContext(DbContextOptions<Dsw2026TpiDbContext> options):
        base(options)
    {
    }

    public DbSet<Dsw2026Tpi.Domain.Entities.AvailabilityRule> AvailabilityRules { get; set; }
    public DbSet<Dsw2026Tpi.Domain.Entities.AvailabilitySlot> AvailabilitySlots { get; set; }
    public DbSet<Dsw2026Tpi.Domain.Entities.Appointment> Appointments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    
}

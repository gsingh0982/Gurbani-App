using Microsoft.EntityFrameworkCore;
using Gurbani.Models;

namespace Gurbani.Data;

// DbContext is Entity Framework's way of talking to the database
// This class tells EF what tables exist and how they are structured
public class GurbaniDbContext(DbContextOptions<GurbaniDbContext> options) : DbContext(options)
{
    public DbSet<CachedAng> CachedAngs { get; set; }             // Table: CachedAngs
    public DbSet<CachedHukamnama> CachedHukamnamas { get; set; } // Table: CachedHukamnamas
    public DbSet<CachedBani> CachedBanis { get; set; }           // Table: CachedBanis

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // AngNo is set manually (1–1430), not auto-generated
        modelBuilder.Entity<CachedAng>().HasKey(a => a.AngNo);
        modelBuilder.Entity<CachedAng>().Property(a => a.AngNo).ValueGeneratedNever();

        // Date is the PK for Hukamnama (one per day)
        modelBuilder.Entity<CachedHukamnama>().HasKey(h => h.Date);

        // BaniId is set manually (1–29), not auto-generated
        modelBuilder.Entity<CachedBani>().HasKey(b => b.BaniId);
        modelBuilder.Entity<CachedBani>().Property(b => b.BaniId).ValueGeneratedNever();
    }
}

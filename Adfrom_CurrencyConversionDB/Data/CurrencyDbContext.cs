using Adfrom_CurrencyConversionDB.Models;
using Microsoft.EntityFrameworkCore;

namespace Adfrom_CurrencyConversionDB.Data
{
    public class CurrencyDbContext : DbContext
    {
        public CurrencyDbContext(DbContextOptions<CurrencyDbContext> options) : base(options) { }

        public DbSet<CurrencyRates> CurrencyRates { get; set; }
        public DbSet<CurrencyConversionRecord> CurrencyConversions { get; set; }

    }
}

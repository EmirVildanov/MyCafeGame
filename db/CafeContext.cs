using Microsoft.EntityFrameworkCore;

namespace MyCafe.db
{
    public sealed class CafeContext : DbContext
    {
        public DbSet<Plantation> plantation { get; set; }
        public DbSet<Buyer> buyer { get; set; }
        public DbSet<AmericanPort> american_port { get; set; }
        public DbSet<EuropeanPort> european_port { get; set; }
        public DbSet<PlantationToPortDelivery> plantation_to_port_delivery { get; set; }
        public DbSet<PortToPortDelivery> port_to_port_delivery { get; set; }
        public DbSet<ShipAmericaToEuropeCost> ship_america_to_europe_costs { get; set; }
        public DbSet<Ship> ship { get; set; }
        public CafeContext() { Database.EnsureCreated(); }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var host = "localhost";
            var port = 5432;
            var db = "postgres";
            var usr = "postgres";
            var pswd = 1861;
            var link = $"Host={host};Port={port};Database={db};Username={usr};Password={pswd}";
            optionsBuilder.UseNpgsql(link);
        }
    }
}
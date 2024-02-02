using Microsoft.EntityFrameworkCore;

namespace CloudWeather.Temperature.DataAccess
{
    public class TemperatureDbContext : DbContext
    {

        public TemperatureDbContext() { }


        public TemperatureDbContext(DbContextOptions<TemperatureDbContext> options) : base(options)
        {

        }


        public DbSet<Temperature> Temperature { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            SnakeCaseIdentiTyTableNames(modelBuilder);
        }

        private static void SnakeCaseIdentiTyTableNames(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Temperature>(b => b.ToTable("temperature"));
        }

    }


}


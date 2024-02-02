using Microsoft.EntityFrameworkCore;

namespace CloudWeather.Report.DataAccess
{
    public class WeatherReportDbContext : DbContext
    {
        public WeatherReportDbContext() { }


        public WeatherReportDbContext(DbContextOptions<WeatherReportDbContext> options) : base(options)
        {

        }


        public DbSet<WeatherReport> WeatherReport { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            SnakeCaseIdentiTyTableNames(modelBuilder);
        }

        private static void SnakeCaseIdentiTyTableNames(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeatherReport>(b => b.ToTable("weather_report"));
        }

    }
}


using CloudWeather.Precipitation.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<PrecipDbContext>
(
    opts =>
    {
        opts.EnableSensitiveDataLogging();
        opts.EnableDetailedErrors();
        opts.UseNpgsql(builder.Configuration.GetConnectionString("AppDb"));
    }, ServiceLifetime.Transient
);


var app = builder.Build();


app.MapGet("observation/{zip}", async (string zip, [FromQuery] int? days, PrecipDbContext db) => {

    if(days == null || days < 1 || days > 30  )
    {
        return Results.BadRequest("Informe o parâmetro 'days' entre 1 a 30 ");
    } 


    var startDate = DateTime.UtcNow - TimeSpan.FromDays(days.Value);
    var results = await db.Precipitation
                     .Where(p => p.ZipCode == zip && p.CreatedOn > startDate)
                     .ToListAsync();

    return Results.Ok(results);
});


app.Run();


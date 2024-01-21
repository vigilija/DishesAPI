using DishesAPI.DbContexts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DishesDbContext>(o => o.UseSqlite(
    builder.Configuration["ConnectionStrings:DishesDBConnectionString"]));


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/dishes", async (DishesDbContext dishesDbContext) =>
{
    return await dishesDbContext.Dishes.ToListAsync();
});

app.MapGet("/dishes/{dishId:guid}", async (DishesDbContext dishesDbContext, Guid dishId) =>
{
    return await dishesDbContext.Dishes.FirstOrDefaultAsync(d => d.Id == dishId);
});

app.MapGet("/dishes/{dishName}", async (DishesDbContext dishesDbContext, string dishName) =>
{
    return await dishesDbContext.Dishes.FirstOrDefaultAsync(d => d.Name == dishName);
});

app.MapGet("/disches/{dishId}", async (DishesDbContext dischDbContext, Guid dishId) =>
{
    return (await dischDbContext.Dishes
    .Include(d => d.Ingredients)
    .FirstOrDefaultAsync(d => d.Id == dishId))?.Ingredients;
});

//migrates and recreates DB on each run
using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<DishesDbContext>();
    context.Database.EnsureCreated();
    context.Database.Migrate();
};

app.Run();

//internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}

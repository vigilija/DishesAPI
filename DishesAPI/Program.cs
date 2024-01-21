using AutoMapper;
using DishesAPI.DbContexts;
using DishesAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DishesDbContext>(o => o.UseSqlite(
    builder.Configuration["ConnectionStrings:DishesDBConnectionString"]));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/dishes", async (DishesDbContext dishesDbContext, IMapper mapper) =>
{
    return mapper.Map<IEnumerable<DishDto>> (await dishesDbContext.Dishes.ToListAsync());
});

app.MapGet("/dishes/{dishId:guid}", async (DishesDbContext dishesDbContext, IMapper mapper, Guid dishId) =>
{   
    return mapper.Map<DishDto>( await dishesDbContext.Dishes.FirstOrDefaultAsync(d => d.Id == dishId));
});

app.MapGet("/dishes/{dishName}", async (DishesDbContext dishesDbContext, IMapper mapper, string dishName) =>
{
    return mapper.Map<DishDto>(await dishesDbContext.Dishes.FirstOrDefaultAsync(d => d.Name == dishName));
});

app.MapGet("/disches/{dishId}/ingrediets", async (DishesDbContext dischDbContext, IMapper mapper, Guid dishId) =>
{
    return mapper.Map<IEnumerable<IngredientDto>>((await dischDbContext.Dishes
    .Include(d => d.Ingredients)
    .FirstOrDefaultAsync(d => d.Id == dishId))?.Ingredients);
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

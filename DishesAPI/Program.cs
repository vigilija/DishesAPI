using AutoMapper;
using DishesAPI.DbContexts;
using DishesAPI.Entities;
using DishesAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DishesDbContext>(o => o.UseSqlite(
    builder.Configuration["ConnectionStrings:DishesDBConnectionString"]));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

//---Get actions to get data about dishes
app.MapGet("/dishes", async Task<Ok<IEnumerable<DishDto>>> (DishesDbContext dishesDbContext,
    ClaimsPrincipal claimsPrincipal,
    IMapper mapper,
    [FromQuery] string? name) =>
{
    Console.WriteLine($"User authenticated? {claimsPrincipal.Identity?.IsAuthenticated}");

    return TypedResults.Ok(mapper.Map<IEnumerable<DishDto>>(await dishesDbContext
        .Dishes
        .Where(d => name == null || d.Name.Contains(name))
        .ToListAsync()));
});

app.MapGet("/dishes/{dishId:guid}", async Task<Results<NotFound, Ok<DishDto>>> (DishesDbContext dishesDbContext,
    IMapper mapper,
    Guid dishId) =>
{
    var dishEntry = await dishesDbContext
        .Dishes
        .FirstOrDefaultAsync(d => d.Id == dishId);

    if (dishEntry != null) return TypedResults.NotFound();

    return TypedResults.Ok(mapper.Map<DishDto>(dishEntry));
});

app.MapGet("/dishes/{dishName}", async Task<Results<NotFound, Ok<DishDto>>>
    (DishesDbContext dishesDbContext,
    IMapper mapper,
    string dishName) =>
{
    var dishByName = await dishesDbContext
    .Dishes
    .FirstOrDefaultAsync(n => n.Name == dishName);

    if (dishByName != null) return TypedResults.NotFound();

    return TypedResults.Ok(mapper.Map<DishDto>(await dishesDbContext
            .Dishes
            .FirstOrDefaultAsync(d => d.Name == dishName)));
});

app.MapGet("/disches/{dishId}/ingrediets", async Task<Results<NotFound, Ok<IEnumerable<IngredientDto>>>>
    (DishesDbContext dishDbContext,
    IMapper mapper,
    Guid dishId) =>
{
    var dishEntity = await dishDbContext
    .Dishes
    .FirstOrDefaultAsync(d => d.Id == dishId);

    if (dishEntity == null) return TypedResults.NotFound();

    return TypedResults.Ok(mapper.Map<IEnumerable<IngredientDto>>
        ((await dishDbContext
        .Dishes
        .Include(d => d.Ingredients)
        .FirstOrDefaultAsync(d => d.Id == dishId))?.Ingredients));
});

//---Post action to create resaurces
app.MapPost("/dishes", async (DishesDbContext dischesDbContext,
    IMapper mapper,
    [FromBody] DishForCreationDto dishForCreationDto) =>
{
    var dishEntity = mapper.Map<Dish>(dishForCreationDto);
    dischesDbContext.Add(dishForCreationDto);
    await dischesDbContext.SaveChangesAsync();

    var dishToReturn = mapper.Map<DishDto>(dishEntity);
    return TypedResults.Ok(dishToReturn);

});

//---migrates and recreates DB on each run
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

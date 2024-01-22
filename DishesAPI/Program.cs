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

var dishesEndpoints = app.MapGroup("/dishes");
var dishWithGuidIdEndpoints = dishesEndpoints.MapGroup("/{dishId:guid}");
var ingredientsEndpoints = dishWithGuidIdEndpoints.MapGroup("/ingredients");


//---Get actions to get data about dishes
dishesEndpoints.MapGet("", async Task<Ok<IEnumerable<DishDto>>> (DishesDbContext dishesDbContext,
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

dishWithGuidIdEndpoints.MapGet("", async Task<Results<NotFound, Ok<DishDto>>> (DishesDbContext dishesDbContext,
    IMapper mapper,
    Guid dishId) =>
{
    var dishEntry = await dishesDbContext
        .Dishes
        .FirstOrDefaultAsync(d => d.Id == dishId);

    if (dishEntry != null) return TypedResults.NotFound();

    return TypedResults.Ok(mapper.Map<DishDto>(dishEntry));
}).WithName("GetDish");

dishesEndpoints.MapGet("/{dishName}", async Task<Results<NotFound, Ok<DishDto>>>
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

ingredientsEndpoints.MapGet("", async Task<Results<NotFound, Ok<IEnumerable<IngredientDto>>>>
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
dishesEndpoints.MapPost("", async Task<CreatedAtRoute<DishDto>>
    (DishesDbContext dischesDbContext,
    IMapper mapper,
    [FromBody] DishForCreationDto dishForCreationDto
   // LinkGenerator linkGenerator,
    //HttpContext httpContext
    ) =>
{
    var dishEntity = mapper.Map<Dish>(dishForCreationDto);

    dischesDbContext.Add(dishEntity);

    await dischesDbContext.SaveChangesAsync();

    var dishToReturn = mapper.Map<DishDto>(dishEntity);

    return TypedResults.CreatedAtRoute(
        dishToReturn,
        "GetDish",
        new { dishId = dishToReturn.Id });

    //var linkToDIsh = linkGenerator.GetUriByName(httpContext, "GetDish", new {dishId = dishToReturn.Id});
    //return TypedResults.Created(linkToDIsh, dishToReturn);
});

dishWithGuidIdEndpoints.MapPut("", async Task<Results<NotFound, NoContent>> 
    (DishesDbContext dischesDbContext,
    IMapper mapper,
    Guid dishId,
    DishForUpdateDto dishToUpdateDto) =>
{
    var dishEntity = await dischesDbContext
    .Dishes
    .FirstOrDefaultAsync(d => d.Id == dishId);

    if (dishEntity == null) return TypedResults.NotFound();

    mapper.Map(dishToUpdateDto, dishEntity);

    await dischesDbContext.SaveChangesAsync();

    return TypedResults.NoContent();

});

dishWithGuidIdEndpoints.MapDelete("", async Task<Results<NotFound,NoContent>>
    (DishesDbContext dishesDbContext, 
    Guid dishId) => 
{
    var dishEntity = await dishesDbContext.Dishes.FirstOrDefaultAsync(d => d.Id == dishId);
    if (dishEntity == null)
    {
        return TypedResults.NotFound();
    }

    dishesDbContext.Dishes.Remove(dishEntity);
    await dishesDbContext.SaveChangesAsync();

    return TypedResults.NoContent();
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

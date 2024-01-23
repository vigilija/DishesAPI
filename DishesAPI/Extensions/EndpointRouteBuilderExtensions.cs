using DishesAPI.EndpointFilters;
using DishesAPI.EndpointHandlers;
using System.Reflection;

namespace DishesAPI.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static void RegisterDishesEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var dishesEndpoints = endpointRouteBuilder.MapGroup("/dishes");
        var dishWithGuidIdEndpoints = dishesEndpoints.MapGroup("/{dishId:guid}");
        //try
        var dishWithGuidIdEndpointsAndLockFilters = endpointRouteBuilder.MapGroup("/dishes/{dishId:guid}")
            .AddEndpointFilter(new DishIsLockedFilter(new Guid("fd630a57-2352-4731-b25c-db9cc7601b16")))
            .AddEndpointFilter(new DishIsLockedFilter(new Guid("eacc5169-b2a7-41ad-92c3-dbb1a5e7af06")));

        dishesEndpoints.MapGet("", DishesHandlers.GetDishesAsync);
        dishWithGuidIdEndpoints.MapGet("", DishesHandlers.GetDishByIdAsync)
            .WithName("GetDish");
        dishesEndpoints.MapGet("/{dishName}", DishesHandlers.GetDishByNameAsync);

        dishesEndpoints.MapPost("", DishesHandlers.CreateDishAsync)
            .AddEndpointFilter<ValidateAnnotationsFilter>();

        dishWithGuidIdEndpointsAndLockFilters.MapPut("", DishesHandlers.UpdateDishAsync);

        dishWithGuidIdEndpointsAndLockFilters.MapDelete("", DishesHandlers.DeleteDishAsync)
            .AddEndpointFilter<LogNotFoundResponseFilter>();
    }

    public static void RegisterIngredientsEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var ingredientsEndpoints = endpointRouteBuilder.MapGroup("/dishes/{dishId:guid}/ingredients");

        ingredientsEndpoints.MapGet("", IngredientsHandlers.GetIngredientsAsync);

        ingredientsEndpoints.MapPost("", () => {
            throw new NotImplementedException();
        });
    }
}

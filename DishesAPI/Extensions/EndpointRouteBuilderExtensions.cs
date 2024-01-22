using DishesAPI.EndpointHandlers;
using System.Reflection;

namespace DishesAPI.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static void RegisterDishesEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var dishesEndpoints = endpointRouteBuilder.MapGroup("/dishes");
        var dishWithGuidIdEndpoints = dishesEndpoints.MapGroup("/{dishId:guid}");

        dishesEndpoints.MapGet("", DishesHandlers.GetDishesAsync);
        dishWithGuidIdEndpoints.MapGet("", DishesHandlers.GetDishByIdAsync)
            .WithName("GetDish");
        dishesEndpoints.MapGet("/{dishName}", DishesHandlers.GetDishByNameAsync);
        dishesEndpoints.MapPost("", DishesHandlers.CreateDishAsync);
        dishWithGuidIdEndpoints.MapPut("", DishesHandlers.UpdateDishAsync)
            .AddEndpointFilter(async (context, next) =>
            {
                var dishId = context.GetArgument<Guid>(2);
                var rendangId = new Guid("fd630a57-2352-4731-b25c-db9cc7601b16");
                if (dishId == rendangId)
                {
                    return TypedResults.Problem(new()
                    {
                        Status = 400,
                        Title = "Dish is perfect, no need to change",
                        Detail = "You cannot update perfection."
                    });
                }

                var resul = await next.Invoke(context);
                return resul;
            });
        dishWithGuidIdEndpoints.MapDelete("", DishesHandlers.DeleteDishAsync);
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


namespace DishesAPI.EndpointFilters;

public class RendangDishIsLockedFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        Guid dishId;
        if (context.HttpContext.Request.Method == "PUT")
        {
            dishId = context.GetArgument<Guid>(2);
        }
        else if (context.HttpContext.Request.Method == "DELETE")
        {
            dishId=context.GetArgument<Guid>(1);
        }
        else
        {
            throw new NotSupportedException("This filter is not supported for this scenario.");
        }

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
    }
}

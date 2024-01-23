using DishesAPI.Models;
using MiniValidation;

namespace DishesAPI.EndpointFilters
{
    public class ValidateAnnotationsFilter : IEndpointFilter
    { 
        public async ValueTask<object?> InvokeAsync (EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var dishForCreationDto = context.GetArgument<DishForCreationDto>(2);

            if (!MiniValidator.TryValidate(dishForCreationDto, out var validationError))
            {
                return TypedResults.ValidationProblem(validationError);
            }
            return await next(context);
        }
    }
}

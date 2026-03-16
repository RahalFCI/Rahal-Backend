using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.API.Filters
{
    public class ValidationActionFilter : IAsyncActionFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationActionFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Loop through all action arguments
            foreach (var argument in context.ActionArguments)
            {
                if (argument.Value == null) continue;

                var argumentType = argument.Value.GetType();

                // Get the validator for this type
                var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
                var validator = _serviceProvider.GetService(validatorType) as IValidator;

                if (validator != null)
                {
                    // Create validation context
                    var context2 = new ValidationContext<object>(argument.Value);
                    var validationResult = await validator.ValidateAsync(context2);

                    if (!validationResult.IsValid)
                    {
                        // Add errors to ModelState
                        foreach (var error in validationResult.Errors)
                        {
                            context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                        }

                        // Return BadRequest with validation errors
                        context.Result = new BadRequestObjectResult(new
                        {
                            errors = validationResult.Errors.Select(e => new
                            {
                                property = e.PropertyName,
                                message = e.ErrorMessage
                            })
                        });
                        return;
                    }
                }
            }

            await next();
        }
    }
}

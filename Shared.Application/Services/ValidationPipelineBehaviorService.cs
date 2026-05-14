using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Services
{
    public class ValidationPipelineBehaviorService<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull

    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationPipelineBehaviorService(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            if (!_validators.Any()) return await next();

            var context = new ValidationContext<TRequest>(request);

            var failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
                throw new ValidationException(failures);

            return await next();
        }
    }
}

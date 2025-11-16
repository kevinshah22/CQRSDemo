using CQRSDemo.Application.Abstractions;
using CQRSDemo.Domain.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CQRSDemo.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Count > 0)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count > 0)
        {
            var errorMessage = failures
                .Select(f => $"{f.PropertyName}: {f.ErrorMessage}")
                .Aggregate((current, next) => $"{current}; {next}");

            _logger.LogWarning("Validation failed for {RequestType}: {Errors}", 
                typeof(TRequest).Name, errorMessage);

            throw new FluentValidation.ValidationException(errorMessage);
        }

        _logger.LogDebug("Validation passed for {RequestType}", typeof(TRequest).Name);

        return await next();
    }
}

public interface IPipelineBehavior<TRequest, TResponse>
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

using CQRSDemo.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace CQRSDemo.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        _logger.LogInformation("Processing {RequestType} request: {@Request}", requestName, request);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();
            
            _logger.LogInformation(
                "Completed {RequestType} request in {ElapsedMilliseconds}ms: {@Response}", 
                requestName, stopwatch.ElapsedMilliseconds, response);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex, 
                "Failed {RequestType} request after {ElapsedMilliseconds}ms: {@Request}", 
                requestName, stopwatch.ElapsedMilliseconds, request);

            throw;
        }
    }
}

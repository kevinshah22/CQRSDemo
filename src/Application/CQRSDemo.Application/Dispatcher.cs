using CQRSDemo.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CQRSDemo.Application;

public class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> Dispatch<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        
        // Try to find a command handler first
        var commandHandlerType = typeof(ICommandHandler<,>).MakeGenericType(requestType, typeof(TResult));
        var handler = _serviceProvider.GetService(commandHandlerType);
        
        // If no command handler, try to find a query handler
        if (handler == null)
        {
            var queryHandlerType = typeof(IQueryHandler<,>).MakeGenericType(requestType, typeof(TResult));
            handler = _serviceProvider.GetService(queryHandlerType);
        }

        if (handler == null)
        {
            throw new InvalidOperationException($"No handler found for request type {requestType.Name}");
        }

        // Get the Handle method using reflection
        var handleMethod = handler.GetType().GetMethod("Handle", new[] { requestType, typeof(CancellationToken) });
        if (handleMethod == null)
        {
            throw new InvalidOperationException($"Handle method not found on handler type {handler.GetType().Name}");
        }

        var result = await (Task<TResult>)handleMethod.Invoke(handler, new object[] { request, cancellationToken })!;

        return result;
    }

    public async Task Dispatch(IRequest request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        
        // Find a command handler
        var commandHandlerType = typeof(ICommandHandler<>).MakeGenericType(requestType);
        var handler = _serviceProvider.GetService(commandHandlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No handler found for request type {requestType.Name}");
        }

        // Get the Handle method using reflection
        var handleMethod = handler.GetType().GetMethod("Handle", new[] { requestType, typeof(CancellationToken) });
        if (handleMethod == null)
        {
            throw new InvalidOperationException($"Handle method not found on handler type {handler.GetType().Name}");
        }

        await (Task)handleMethod.Invoke(handler, new object[] { request, cancellationToken })!;
    }
}

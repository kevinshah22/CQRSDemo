namespace CQRSDemo.Application.Abstractions;

public interface IDispatcher
{
    Task<TResult> Dispatch<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);
    Task Dispatch(IRequest request, CancellationToken cancellationToken = default);
}

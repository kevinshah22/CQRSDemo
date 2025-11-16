namespace CQRSDemo.Application.Abstractions;

public interface IQuery<out TResult> : IRequest<TResult>
{
}

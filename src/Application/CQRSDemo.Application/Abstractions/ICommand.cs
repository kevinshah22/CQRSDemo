namespace CQRSDemo.Application.Abstractions;

public interface ICommand<out TResult> : IRequest<TResult>
{
}

public interface ICommand : ICommand<Unit>
{
}

public interface IRequest<out TResult>
{
}

public interface IRequest : IRequest<Unit>
{
}

public readonly struct Unit
{
    public static readonly Unit Value = new();
}

using CQRSDemo.Application;
using CQRSDemo.Application.Abstractions;
using CQRSDemo.Application.Behaviors;
using CQRSDemo.Application.Commands.Customers;
using CQRSDemo.Application.Commands.Orders;
using CQRSDemo.Application.DTOs;
using CQRSDemo.Application.Queries.Customers;
using CQRSDemo.Application.Queries.Orders;
using CQRSDemo.Application.Validators.Commands;
using CQRSDemo.Domain.Repositories;
using CQRSDemo.Infrastructure.Data;
using CQRSDemo.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CQRSDemo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddApplication();

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        services.AddScoped<ICommandHandler<CreateCustomerCommand, Guid>, CreateCustomerCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateCustomerCommand>, UpdateCustomerCommandHandler>();
        services.AddScoped<ICommandHandler<CreateOrderCommand, Guid>, CreateOrderCommandHandler>();

        services.AddScoped<IQueryHandler<GetCustomerByIdQuery, CustomerDto?>, GetCustomerByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetAllCustomersQuery, IReadOnlyList<CustomerDto>>, GetAllCustomersQueryHandler>();
        services.AddScoped<IQueryHandler<GetOrdersByCustomerIdQuery, IReadOnlyList<OrderDto>>, GetOrdersByCustomerIdQueryHandler>();

        services.AddValidatorsFromAssemblyContaining<CreateCustomerCommandValidator>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }
}

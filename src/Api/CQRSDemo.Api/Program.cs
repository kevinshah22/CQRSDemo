using CQRSDemo.Application.DTOs;
using CQRSDemo.Application.Commands.Customers;
using CQRSDemo.Application.Commands.Orders;
using CQRSDemo.Application.Queries.Customers;
using CQRSDemo.Application.Queries.Orders;
using CQRSDemo.Domain.Exceptions;
using CQRSDemo.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace CQRSDemo.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration));

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=localhost;Initial Catalog=CQRSDemoDb;Integrated Security=True;TrustServerCertificate=True";

        builder.Services.AddInfrastructure(connectionString);

        builder.Services.AddProblemDetails();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseExceptionHandler();

        app.Use(async (context, next) =>
        {
            try
            {
                await next(context);
            }
            catch (ValidationException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Validation failed",
                    message = ex.Message
                });
            }
            catch (NotFoundException ex)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Resource not found",
                    message = ex.Message
                });
            }
            catch (DomainException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Domain error",
                    message = ex.Message
                });
            }
            catch (Exception)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Internal server error",
                    message = "An unexpected error occurred. Please try again later."
                });
            }
        });

        app.MapControllers();

        app.Run();
    }
}

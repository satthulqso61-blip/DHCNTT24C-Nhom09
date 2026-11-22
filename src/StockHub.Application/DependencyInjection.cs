using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using StockHub.Application.Common.Behaviors;
using StockHub.Application.Features.Auth.Services;

namespace StockHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<Features.Warehouses.Services.IWarehouseService, Features.Warehouses.Services.WarehouseService>();
        services.AddScoped<Features.Products.Services.IProductService, Features.Products.Services.ProductService>();

        return services;
    }
}

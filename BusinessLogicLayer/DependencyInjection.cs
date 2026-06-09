using BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicAccessLayer(this IServiceCollection services)
    {
        //Add Data Access layer services into the Ioc container 
        services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<OrderItemAddRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<OrderUpdateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<OrderItemUpdateRequestValidator>();
        return services;
    }
}
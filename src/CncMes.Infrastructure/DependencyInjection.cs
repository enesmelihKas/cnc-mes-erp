using CncMes.Application.Pricing;
using CncMes.Application.Production;
using CncMes.Application.Reporting;
using CncMes.Application.Sales;
using CncMes.Infrastructure.Identity;
using CncMes.Infrastructure.Persistence;
using CncMes.Infrastructure.Pricing;
using CncMes.Infrastructure.Production;
using CncMes.Infrastructure.Reporting;
using CncMes.Infrastructure.Sales;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CncMes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        string stlStorageDirectory)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services
            .AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                options.User.RequireUniqueEmail = false;
            })
            .AddRoles<AppRole>()
            .AddRoleManager<RoleManager<AppRole>>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddSingleton(new StlStorageOptions { BaseDirectory = stlStorageDirectory });

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<IMaterialService, MaterialService>();
        services.AddScoped<IStlAnalysisService, StlAnalysisService>();
        services.AddScoped<IProfitLossService, ProfitLossService>();
        services.AddSingleton<IProfitLossReportPdf, ProfitLossReportPdf>();

        return services;
    }
}

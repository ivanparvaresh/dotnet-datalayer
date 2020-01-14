using System;
using Dotnet.DataLayer;
using Microsoft.EntityFrameworkCore;
using Dotnet.DataLayer.EntityFramework;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDatabaseContext<TDatabaseContext>(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder> optionsAction = null,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TDatabaseContext : DatabaseContext<TDatabaseContext>
        {
            services.AddDbContext<TDatabaseContext>(optionsAction, contextLifetime, optionsLifetime);
            services.Add(new ServiceDescriptor(typeof(IDatabaseContext), p => p.GetService<TDatabaseContext>(), contextLifetime));
            services.Add(new ServiceDescriptor(typeof(IDatabaseContext<TDatabaseContext>), p => p.GetService<TDatabaseContext>(), contextLifetime));
            return services;
        }
    }
}
using System;
using MongoDB.Driver;
using Dotnet.DataLayer;
using Dotnet.DataLayer.MongoDb;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDatabaseContext<TDatabaseContext>(
            this IServiceCollection services,
            MongoUrl mongoUrl,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TDatabaseContext : DatabaseContext<TDatabaseContext>
        {
            services.Add(
                new ServiceDescriptor(
                    typeof(DbContextOptions<TDatabaseContext>),
                    p => new DbContextOptions<TDatabaseContext>(mongoUrl),
                    optionsLifetime
                )
            );
            services.Add(
                new ServiceDescriptor(
                    typeof(TDatabaseContext),
                    typeof(TDatabaseContext),
                    contextLifetime
                )
            );
            services.Add(new ServiceDescriptor(typeof(IDatabaseContext), p => p.GetService<TDatabaseContext>(), contextLifetime));
            services.Add(new ServiceDescriptor(typeof(IDatabaseContext<TDatabaseContext>), p => p.GetService<TDatabaseContext>(), contextLifetime));
            return services;
        }
    }
}
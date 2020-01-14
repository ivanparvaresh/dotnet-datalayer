using Dotnet.DataLayer;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDatabaseContext<TDatabaseContext>(this IServiceCollection services)
            where TDatabaseContext : class, IDatabaseContext<TDatabaseContext>
        {
            services.AddScoped<TDatabaseContext>();
            services.AddScoped<IDatabaseContext>(p => p.GetService<TDatabaseContext>());
            services.AddScoped<IDatabaseContext<TDatabaseContext>>(p => p.GetService<TDatabaseContext>());
            return services;
        }
    }
}
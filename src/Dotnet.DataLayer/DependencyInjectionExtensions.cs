using Dotnet.DataLayer;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDatasource<TDatasource>(this IServiceCollection services)
            where TDatasource : class, IDatasource
        {
            services.AddScoped<TDatasource>();
            services.AddScoped<IDatasource>(p => p.GetService<TDatasource>());
            return services;
        }
        public static IServiceCollection AddDatasource<TDatasource, TSession>(this IServiceCollection services)
            where TDatasource : class, IDatasource
            where TSession : class, ISession<TDatasource>
        {
            services.AddScoped<TDatasource>();
            services.AddScoped<IDatasource>(p => p.GetService<TDatasource>());
            services.AddScoped<TSession>();
            services.AddScoped<ISession>(p => p.GetService<TSession>());
            services.AddScoped<ISession<TDatasource>>(p => p.GetService<TSession>());
            return services;
        }
    }
}
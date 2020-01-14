using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Dotnet.DataLayer.MongoDb;
using MongoDB.Driver;

namespace Dotnet.DataLayer.Test.MognoDb
{
    public class DependencyInjectionExtensionsTest
    {
        [Fact]
        public void AddDatabaseContext_Should_Register_As_ScopedService()
        {
            // PREPARATION
            var services = new ServiceCollection();


            // Execution
            var mongoUrl = new MongoUrl("mongodb://username:password@host:1234/test");
            services.AddDatabaseContext<TestDbContext>(mongoUrl);

            // ASSERTION
            Assert.Contains(services, s => s.ServiceType == typeof(TestDbContext) && s.Lifetime == ServiceLifetime.Scoped);
            Assert.Contains(services, s => s.ServiceType == typeof(IDatabaseContext) && s.Lifetime == ServiceLifetime.Scoped);
            Assert.Contains(services, s => s.ServiceType == typeof(IDatabaseContext<TestDbContext>) && s.Lifetime == ServiceLifetime.Scoped);
        }
        [Theory]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Singleton)]
        public void AddDatabaseContext_Should_Register_As_Custom_Scope(ServiceLifetime contextScope, ServiceLifetime optionsScope)
        {
            // PREPARATION
            var services = new ServiceCollection();


            // Execution
            var mongoUrl = new MongoUrl("mongodb://username:password@host:1234/test");
            services.AddDatabaseContext<TestDbContext>(mongoUrl, contextScope, optionsScope);

            // ASSERTION
            // ASSERTION
            Assert.Contains(services, s => s.ServiceType == typeof(TestDbContext) && s.Lifetime == contextScope);
            Assert.Contains(services, s => s.ServiceType == typeof(IDatabaseContext) && s.Lifetime == contextScope);
            Assert.Contains(services, s => s.ServiceType == typeof(IDatabaseContext<TestDbContext>) && s.Lifetime == contextScope);
            Assert.Contains(services, s => s.ServiceType == typeof(DbContextOptions<TestDbContext>) && s.Lifetime == optionsScope);
        }

        [Fact]
        public void AddDatabaseContext_Should_Be_Resolveable()
        {
            // PREPARATION
            var services = new ServiceCollection();

            // Execution
            services.AddDatabaseContext<TestDbContext>(new MongoUrl("mongodb://username:password@host:1234/test"));
            var provider = services.BuildServiceProvider();
            var dbContext = provider.GetService<TestDbContext>();

            // ASSERTION
            Assert.NotNull(dbContext);
        }

        // Internal Classes
        public class TestDbContext : DatabaseContext<TestDbContext>
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
            { }
        }
    }
}
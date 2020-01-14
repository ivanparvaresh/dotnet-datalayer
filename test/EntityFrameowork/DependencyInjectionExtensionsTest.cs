using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Dotnet.DataLayer.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Dotnet.DataLayer.Test.EntityFrameowork
{
    public class DependencyInjectionExtensionsTest
    {
        [Fact]
        public void AddDatabaseContext_Should_Register_As_ScopedService()
        {
            // PREPARATION
            var services = new ServiceCollection();


            // Execution
            services.AddDatabaseContext<TestDbContext>();

            // ASSERTION
            Assert.Contains(services, s => s.ServiceType == typeof(TestDbContext) && s.Lifetime == ServiceLifetime.Scoped);
            Assert.Contains(services, s => s.ServiceType == typeof(IDatabaseContext) && s.Lifetime == ServiceLifetime.Scoped);
            Assert.Contains(services, s => s.ServiceType == typeof(IDatabaseContext<TestDbContext>) && s.Lifetime == ServiceLifetime.Scoped);
        }
        [Fact]
        public void AddDatabaseContext_Should_Register_Allow_Configure_Context()
        {
            // PREPARATION
            var services = new ServiceCollection();


            // Execution
            var hasExecuted = false;
            services.AddDatabaseContext<TestDbContext>(
                builder =>
                {
                    hasExecuted = true;
                }
            );
            var provider = services.BuildServiceProvider();
            provider.GetService<TestDbContext>();


            // ASSERTION
            Assert.True(hasExecuted);
        }
        [Theory]
        [InlineData(ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Transient)]
        public void AddDatabaseContext_Should_Register_As_Custom_Scope(ServiceLifetime scope)
        {
            // PREPARATION
            var services = new ServiceCollection();


            // Execution
            services.AddDatabaseContext<TestDbContext>(
                contextLifetime: scope
            );

            // ASSERTION
            // ASSERTION
            Assert.Contains(services, s => s.ServiceType == typeof(TestDbContext) && s.Lifetime == scope);
            Assert.Contains(services, s => s.ServiceType == typeof(IDatabaseContext) && s.Lifetime == scope);
            Assert.Contains(services, s => s.ServiceType == typeof(IDatabaseContext<TestDbContext>) && s.Lifetime == scope);
        }

        [Fact]
        public void AddDatabaseContext_Should_Be_Resolveable()
        {
            // PREPARATION
            var services = new ServiceCollection();
            services.AddDatabaseContext<TestDbContext>();
            var provider = services.BuildServiceProvider();


            // Execution
            var dbContext = provider.GetService<TestDbContext>();

            // ASSERTION
            Assert.NotNull(dbContext);
        }

        // Internal Classes
        public class TestDbContext : DatabaseContext<TestDbContext>
        {
            public TestDbContext(DbContextOptions options) : base(options)
            { }
        }
    }
}
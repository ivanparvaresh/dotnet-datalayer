using Xunit;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Dotnet.DataLayer.Test.Extensions
{
    public class DependencyInjectionExtensionsTest
    {
        [Fact]
        public void AddDatabaseContext_Should_Register_DataSource_And_Session_As_Service()
        {
            // PREPARATION
            var services = new ServiceCollection();


            // Execution
            services.AddDatabaseContext<TestDbContext>();

            // ASSERTION
            Assert.Contains(services, s => s.ServiceType == typeof(TestDbContext));
            Assert.Contains(services, s => s.ServiceType == typeof(IDatabaseContext));
            Assert.Contains(services, s => s.ServiceType == typeof(IDatabaseContext<TestDbContext>));
        }

        // Internal Classes
        public class TestDbContext : IDatabaseContext<TestDbContext>
        {
            public Task BeginTransactionAsync()
            {
                throw new NotImplementedException();
            }

            public Task CommitAsync()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public Task RollbackAsync()
            {
                throw new NotImplementedException();
            }
        }
    }
}
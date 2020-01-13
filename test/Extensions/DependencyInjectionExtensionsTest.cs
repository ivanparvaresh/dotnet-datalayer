using Xunit;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Dotnet.DataLayer.Test.Extensions
{
    public class DependencyInjectionExtensionsTest
    {
        [Fact]
        public void AddDotnetDataLayer_Should_Register_DataSource_As_Service()
        {
            // PREPARATION
            var services = new ServiceCollection();


            // Execution
            services.AddDatasource<TestDataSource>();

            // ASSERTION
            Assert.Contains(services, s => s.ServiceType == typeof(TestDataSource));
            Assert.Contains(services, s => s.ServiceType == typeof(IDatasource));
        }
        [Fact]
        public void AddDotnetDataLayer_Should_Register_DataSource_And_Session_As_Service()
        {
            // PREPARATION
            var services = new ServiceCollection();


            // Execution
            services.AddDatasource<TestDataSource, TestDataSession>();

            // ASSERTION
            Assert.Contains(services, s => s.ServiceType == typeof(TestDataSource));
            Assert.Contains(services, s => s.ServiceType == typeof(IDatasource));
            Assert.Contains(services, s => s.ServiceType == typeof(TestDataSession));
            Assert.Contains(services, s => s.ServiceType == typeof(ISession));
            Assert.Contains(services, s => s.ServiceType == typeof(ISession<TestDataSource>));
        }

        // Internal Classes
        public class TestDataSource : IDatasource
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
        public class TestDataSession : ISession<TestDataSource>
        {
            public TestDataSource Datasource => throw new NotImplementedException();

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
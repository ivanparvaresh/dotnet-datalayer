using System;
using Xunit;
using Mongo2Go;

namespace Dotnet.DataLayer.Test.MognoDb
{
    [Collection("Database collection")]
    public class Mongo2GoRunnerFixture : IDisposable
    {
        public Mongo2Go.MongoDbRunner Runner { get; private set; }
        public string ConnectionString { get; set; }

        public Mongo2GoRunnerFixture()
        {
            this.Runner = MongoDbRunner.StartForDebugging(
                singleNodeReplSet: true
            );
            var originalConnectionString = this.Runner.ConnectionString;
            this.ConnectionString = originalConnectionString.Replace("?", "test?");
        }

        public void Dispose()
        {
            this.Runner.Dispose();
        }
    }
}
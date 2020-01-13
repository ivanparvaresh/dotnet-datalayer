using Xunit;
using System;
using Mongo2Go;
using System.Linq;
using MongoDB.Driver;
using Dotnet.DataLayer.MongoDb;


namespace Dotnet.DataLayer.Test.MognoDb
{
    [Collection("Database collection")]
    public class MongoDbDatasourceTest : IClassFixture<Mongo2GoRunnerFixture>
    {
        private Mongo2GoRunnerFixture mongo2GoRunnerFixture;

        public MongoDbDatasourceTest(Mongo2GoRunnerFixture mongo2GoRunnerFixture)
        {
            this.mongo2GoRunnerFixture = mongo2GoRunnerFixture ?? throw new ArgumentNullException(nameof(mongo2GoRunnerFixture));
        }

        [Fact]
        public void Constructor_Should_Check_Null_Values()
        {
            Assert.Throws<ArgumentNullException>(() => new MongoDbDatasource(null));
        }

        [Fact]
        public void Constructor_Should_Open_A_Connection_To_The_Database()
        {
            // PREPARATION
            var options = new MongoDbDatasourceOptions(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));
            var ds = new MongoDbDatasource(options);
            ds.Database.DropCollection("SampleEntity");
            ds.Database.Client.DropDatabase("test");


            // ASSERTION
            Assert.NotNull(ds.Client);
            Assert.NotNull(ds.Database);
            var collection = ds.Database.GetCollection<SampleEntity>("SampleEntity");
            collection.InsertOne(new SampleEntity()
            {
                _id = Guid.NewGuid(),
                Name = "Test"
            });
            var record = collection.AsQueryable().SingleOrDefault();
            Assert.Equal("Test", record.Name);

            // CLEANUP
            ds.Dispose();
        }
        [Fact]
        public void GetDatabase_Should_Check_DatabaseName()
        {
            var mongoUrl = new MongoUrl("mongodb://username:password@host:1234");
            var options = new MongoDbDatasourceOptions(mongoUrl);
            Assert.Throws<ArgumentNullException>(() => new MongoDbDatasource(options));
        }

        [Fact]
        public void Dispose_Should_Dispose_The_Cluster()
        {
            // PRPEARATION
            var options = new MongoDbDatasourceOptions(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));
            var ds = new MongoDbDatasource(options);

            // EXECUTION
            ds.Dispose();

            // ASSERTION
            Assert.Null(ds.Client);
            Assert.Null(ds.Database);

            // CLEANUP
            ds.Dispose();
        }

        [Fact]
        public void OnModelCreating_Should_Call_On_Constructor()
        {
            // PREPARATION
            var mongoUrl = new MongoUrl("mongodb://username:password@host:1234/test");
            var options = new MongoDbDatasourceOptions(mongoUrl);

            // EXECUTION
            var ds = new SampleDataSource(options);

            // ASSERTION
            Assert.True(ds.OnModelCreatingExecuted);
        }

        // Internal Classes
        private class SampleEntity
        {
            public Guid _id { get; set; }
            public string Name { get; set; }
        }
        private class SampleDataSource : MongoDbDatasource
        {
            public bool OnModelCreatingExecuted;
            public SampleDataSource(MongoDbDatasourceOptions options) : base(options) { }

            protected override void OnModelCreating()
            {
                this.OnModelCreatingExecuted = true;
            }
        }

    }
}
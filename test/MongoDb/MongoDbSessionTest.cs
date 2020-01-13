using Xunit;
using System;
using Mongo2Go;
using System.Linq;
using MongoDB.Driver;
using Dotnet.DataLayer.MongoDb;
using System.Threading.Tasks;

namespace Dotnet.DataLayer.Test.MognoDb
{
    [Collection("Database collection")]
    public class MongoDbSessionTest : IClassFixture<Mongo2GoRunnerFixture>
    {
        private Mongo2GoRunnerFixture mongo2GoRunnerFixture;

        public MongoDbSessionTest(Mongo2GoRunnerFixture mongo2GoRunnerFixture)
        {
            this.mongo2GoRunnerFixture = mongo2GoRunnerFixture ?? throw new ArgumentNullException(nameof(mongo2GoRunnerFixture));
        }


        [Fact]
        public void Constructor_Should_Check_Null_Values()
        {
            Assert.Throws<ArgumentNullException>(() => new MongoDbSession<MongoDbDatasource>(null));
        }

        [Fact]
        public void Constructor_Should_Start_A_Session()
        {
            // PREPARATION
            var options = new MongoDbDatasourceOptions(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));
            var ds = new MongoDbDatasource(options);
            ds.Database.DropCollection("SampleEntity");
            ds.Database.Client.DropDatabase("test");

            // EXECUTION
            var session = new MongoDbSession<MongoDbDatasource>(ds);

            // ASSERTION
            Assert.NotNull(session.SessionHandler);
            Assert.True(session.SessionHandler.IsInTransaction);

            // CLEANUP
            session.Dispose();
            ds.Dispose();
        }

        [Fact]
        public async Task CommitAsync_Should_Commit_The_Transaction()
        {
            // PREPARATION
            var options = new MongoDbDatasourceOptions(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));
            var ds = new MongoDbDatasource(options);
            ds.Database.DropCollection("SampleEntity");
            ds.Database.Client.DropDatabase("test");
            ds.Database.CreateCollection("SampleEntity");

            var session = new SampleSession(ds);

            // EXECUTION
            session.SampleEntities.InsertOne(session.SessionHandler, new SampleEntity() { Name = "TEST", _id = Guid.NewGuid() });
            await session.CommitAsync();

            // ASSERTION
            var result = ds.Database.GetCollection<SampleEntity>("SampleEntity").AsQueryable().ToList();
            Assert.Collection(result, t =>
            {
                Assert.Equal("TEST", t.Name);
            });

            // CLEANUP
            session.Dispose();
            ds.Dispose();
        }

        [Fact]
        public async Task RollbackAsync_Should_Revert_The_Changes()
        {
            // PREPARATION
            var options = new MongoDbDatasourceOptions(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));
            var ds = new MongoDbDatasource(options);
            ds.Database.DropCollection("SampleEntity");
            ds.Database.Client.DropDatabase("test");
            ds.Database.CreateCollection("SampleEntity");
            var session = new SampleSession(ds);

            // EXECUTION
            session.SampleEntities.InsertOne(session.SessionHandler, new SampleEntity() { Name = "TEST", _id = Guid.NewGuid() });
            await session.RollbackAsync();

            // ASSERTION
            var count = ds.Database.GetCollection<SampleEntity>("SampleEntity").AsQueryable().Count();
            Assert.Equal(0, count);

            // CLEANUP
            session.Dispose();
            ds.Dispose();
        }

        [Fact]
        public void Dispose_Should_Revert_The_Changes_If_Open_Transaction_Exists()
        {
            // PREPARATION
            var options = new MongoDbDatasourceOptions(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));
            var ds = new MongoDbDatasource(options);
            ds.Database.DropCollection("SampleEntity");
            ds.Database.Client.DropDatabase("test");
            ds.Database.CreateCollection("SampleEntity");
            var session = new SampleSession(ds);

            // EXECUTION
            session.SampleEntities.InsertOne(session.SessionHandler, new SampleEntity() { Name = "TEST", _id = Guid.NewGuid() });
            session.Dispose();

            // ASSERTION
            var count = ds.Database.GetCollection<SampleEntity>("SampleEntity").AsQueryable().Count();
            Assert.Equal(0, count);

            // CLEANUP
            ds.Dispose();
        }

        [Fact]
        public void Dispose_Should_Close_The_Session()
        {
            // PREPARATION
            var options = new MongoDbDatasourceOptions(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));
            var ds = new MongoDbDatasource(options);
            ds.Database.DropCollection("SampleEntity");
            ds.Database.Client.DropDatabase("test");
            ds.Database.CreateCollection("SampleEntity");
            var session = new SampleSession(ds);

            // EXECUTION
            session.Dispose();

            // ASSERTION
            Assert.Null(session.SessionHandler);

            // CLEANUP
            ds.Dispose();
        }

        // Internal Classes
        private class SampleSession : MongoDbSession<MongoDbDatasource>
        {
            public IMongoCollection<SampleEntity> SampleEntities { get; private set; }

            public SampleSession(MongoDbDatasource datasource) : base(datasource)
            {
                this.SampleEntities = this.Datasource.Database.GetCollection<SampleEntity>("SampleEntity");
            }
        }
        public class SampleEntity
        {
            public Guid _id { get; set; }
            public string Name { get; set; }
        }
    }
}
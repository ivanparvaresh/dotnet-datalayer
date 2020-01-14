using Xunit;
using System;
using System.Linq;
using MongoDB.Driver;
using Dotnet.DataLayer.MongoDb;
using System.Threading.Tasks;

namespace Dotnet.DataLayer.Test.MognoDb
{
    [Collection("Database collection")]
    public class DataContextTest : IClassFixture<Mongo2GoRunnerFixture>
    {
        private Mongo2GoRunnerFixture mongo2GoRunnerFixture;

        public DataContextTest(Mongo2GoRunnerFixture mongo2GoRunnerFixture)
        {
            this.mongo2GoRunnerFixture = mongo2GoRunnerFixture ?? throw new ArgumentNullException(nameof(mongo2GoRunnerFixture));
        }


        [Fact]
        public void Constructor_Should_Check_Null_Values()
        {
            Assert.Throws<ArgumentNullException>(() => new TestDbContext(null));
        }

        [Fact]
        public void Constructor_Should_Open_A_Connection_To_The_Database()
        {
            // PREPARATION
            var options = new DbContextOptions<TestDbContext>(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));
            var dbContext = new TestDbContext(options);
            dbContext.Database.DropCollection("SampleEntity");
            dbContext.Database.Client.DropDatabase("test");


            // ASSERTION
            Assert.NotNull(dbContext.Client);
            Assert.NotNull(dbContext.Database);
            var collection = dbContext.Database.GetCollection<SampleEntity>("SampleEntity");
            collection.InsertOne(new SampleEntity()
            {
                _id = Guid.NewGuid(),
                Name = "Test"
            });
            var record = collection.AsQueryable().SingleOrDefault();
            Assert.Equal("Test", record.Name);

            // CLEANUP
            dbContext.Dispose();
        }
        [Fact]
        public void Constructor_Should_Call_CreateClient()
        {
            // PREPARATION
            var options = new DbContextOptions<TestDbContext>(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));

            // EXECUTION
            var dbContext = new TestDbContext(options);

            // ASSERTION
            Assert.True(dbContext.HasCreateClientExecuted);

            // CLEANUP
            dbContext.Dispose();
        }
        [Fact]
        public void Constructor_Should_Call_GetDatabase()
        {
            // PREPARATION
            var options = new DbContextOptions<TestDbContext>(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));

            // EXECUTION
            var dbContext = new TestDbContext(options);

            // ASSERTION
            Assert.True(dbContext.HasGetDatabaseExecuted);

            // CLEANUP
            dbContext.Dispose();
        }
        [Fact]
        public void Constructor_Should_Call_OnModelCreating()
        {
            // PREPARATION
            var mongoUrl = new MongoUrl("mongodb://username:password@host:1234/test");
            var options = new DbContextOptions<TestDbContext>(mongoUrl);

            // EXECUTION
            var dbContext = new TestDbContext(options);

            // ASSERTION
            Assert.True(dbContext.HasOnModelCreatingExecuted);
        }

        [Fact]
        public void GetDatabase_Should_Check_DatabaseName()
        {
            var mongoUrl = new MongoUrl("mongodb://username:password@host:1234");
            var options = new DbContextOptions<TestDbContext>(mongoUrl);
            Assert.Throws<ArgumentNullException>(() => new TestDbContext(options));
        }

        [Fact]
        public async Task BeginTransactionAsync_Should_Open_A_Transaction()
        {
            // PREPARATION
            var options = new DbContextOptions<TestDbContext>(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));
            var dbContext = new TestDbContext(options);
            dbContext.Database.DropCollection("SampleEntity");
            dbContext.Database.Client.DropDatabase("test");
            dbContext.Database.CreateCollection("SampleEntity");


            // EXECUTION
            await dbContext.BeginTransactionAsync();

            // ASSERTION
            Assert.NotNull(dbContext.SessionHandler);
            Assert.True(dbContext.SessionHandler.IsInTransaction);

            // CLEANUP
            dbContext.Dispose();
        }
        [Fact]
        public async Task BeginTransactionAsync_Should_Throw_If_Already_Transaction_Initiated()
        {
            // PREPARATION
            var options = new DbContextOptions<TestDbContext>(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));
            var dbContext = new TestDbContext(options);
            dbContext.Database.DropCollection("SampleEntity");
            dbContext.Database.Client.DropDatabase("test");
            dbContext.Database.CreateCollection("SampleEntity");
            await dbContext.BeginTransactionAsync();


            // EXECUTION AND ASSERTION
            await Assert.ThrowsAsync<InvalidOperationException>(() => dbContext.BeginTransactionAsync());

            // CLEANUP
            dbContext.Dispose();
        }



        [Fact]
        public async Task CommitAsync_Should_Commit_The_Transaction()
        {
            // PREPARATION
            var options = new DbContextOptions<TestDbContext>(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));
            var dbContext = new TestDbContext(options);
            dbContext.Database.DropCollection("SampleEntity");
            dbContext.Database.Client.DropDatabase("test");
            dbContext.Database.CreateCollection("SampleEntity");
            await dbContext.BeginTransactionAsync();

            // EXECUTION
            dbContext.SampleEntities.InsertOne(dbContext.SessionHandler, new SampleEntity() { Name = "TEST", _id = Guid.NewGuid() });
            await dbContext.CommitAsync();

            // ASSERTION
            var result = dbContext.Database.GetCollection<SampleEntity>("SampleEntity").AsQueryable().ToList();
            Assert.Collection(result, t =>
            {
                Assert.Equal("TEST", t.Name);
            });

            // CLEANUP
            dbContext.Dispose();
            dbContext.Dispose();
        }
        [Fact]
        public async Task CommitAsync_Should_Throw_If_No_Transaction_Initiated()
        {
            // PREPARATION
            var options = new DbContextOptions<TestDbContext>(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));
            var dbContext = new TestDbContext(options);
            dbContext.Database.DropCollection("SampleEntity");
            dbContext.Database.Client.DropDatabase("test");
            dbContext.Database.CreateCollection("SampleEntity");


            // EXECUTION AND ASSERTION
            await Assert.ThrowsAsync<InvalidOperationException>(() => dbContext.CommitAsync());

            // CLEANUP
            dbContext.Dispose();
        }

        [Fact]
        public async Task RollbackAsync_Should_Revert_The_Changes()
        {
            // PREPARATION
            var options = new DbContextOptions<TestDbContext>(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));
            var dbContext = new TestDbContext(options);
            dbContext.Database.DropCollection("SampleEntity");
            dbContext.Database.Client.DropDatabase("test");
            dbContext.Database.CreateCollection("SampleEntity");
            await dbContext.BeginTransactionAsync();

            // EXECUTION
            dbContext.SampleEntities.InsertOne(dbContext.SessionHandler, new SampleEntity() { Name = "TEST", _id = Guid.NewGuid() });
            await dbContext.RollbackAsync();

            // ASSERTION
            var count = dbContext.Database.GetCollection<SampleEntity>("SampleEntity").AsQueryable().Count();
            Assert.Equal(0, count);

            // CLEANUP
            dbContext.Dispose();
        }
        [Fact]
        public async Task RollbackAsync_Should_Throw_If_No_Transaction_Initiated()
        {
            // PREPARATION
            var options = new DbContextOptions<TestDbContext>(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));
            var dbContext = new TestDbContext(options);
            dbContext.Database.DropCollection("SampleEntity");
            dbContext.Database.Client.DropDatabase("test");
            dbContext.Database.CreateCollection("SampleEntity");


            // EXECUTION AND ASSERTION
            await Assert.ThrowsAsync<InvalidOperationException>(() => dbContext.RollbackAsync());

            // CLEANUP
            dbContext.Dispose();
        }

        [Fact]
        public void Dispose_Should_Dispose_All_Objects()
        {
            // PREPARATION
            var options = new DbContextOptions<TestDbContext>(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));
            var dbContext = new TestDbContext(options);
            dbContext.Database.DropCollection("SampleEntity");
            dbContext.Database.Client.DropDatabase("test");
            dbContext.Database.CreateCollection("SampleEntity");

            // EXECUTION
            dbContext.Dispose();

            // ASSERTION
            Assert.Null(dbContext.SessionHandler);
            Assert.Null(dbContext.Client);
            Assert.Null(dbContext.Database);
        }

        [Fact]
        public async void Dispose_Should_Rolleback_And_Dispose_If_Transaction_Exists()
        {
            // PREPARATION
            var options = new DbContextOptions<TestDbContext>(new MongoDB.Driver.MongoUrl(mongo2GoRunnerFixture.ConnectionString));
            var dbContext = new TestDbContext(options);
            dbContext.Database.DropCollection("SampleEntity");
            dbContext.Database.Client.DropDatabase("test");
            dbContext.Database.CreateCollection("SampleEntity");
            await dbContext.BeginTransactionAsync();
            dbContext.SampleEntities.InsertOne(dbContext.SessionHandler, new SampleEntity() { Name = "TEST", _id = Guid.NewGuid() });

            // EXECUTION
            dbContext.Dispose();

            // ASSERTION
            Assert.Null(dbContext.SessionHandler);
            Assert.Null(dbContext.Client);
            Assert.Null(dbContext.Database);

            var dbContext2 = new TestDbContext(options);
            var count = dbContext2.Database.GetCollection<SampleEntity>("SampleEntity").AsQueryable().Count();
            Assert.Equal(0, count);

            //CLEANUP
            dbContext2.Dispose();
        }

        [Fact]
        public void OnModelCreating_Should_Have_Model_Builder()
        {
        }

        // Internal Classes
        private class TestDbContext : DatabaseContext<TestDbContext>
        {
            public bool HasOnModelCreatingExecuted { get; private set; }
            public bool HasCreateClientExecuted { get; private set; }
            public bool HasGetDatabaseExecuted { get; private set; }
            public IMongoCollection<SampleEntity> SampleEntities { get; private set; }

            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
            {
                this.SampleEntities = this.Database.GetCollection<SampleEntity>("SampleEntity");
            }

            protected override IMongoClient CreateClient(DbContextOptions<TestDbContext> options)
            {
                this.HasCreateClientExecuted = true;
                return base.CreateClient(options);
            }
            protected override IMongoDatabase GetDatabase(DbContextOptions<TestDbContext> options, IMongoClient client)
            {
                this.HasGetDatabaseExecuted = true;
                return base.GetDatabase(options, client);
            }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                this.HasOnModelCreatingExecuted = true;
                if (builder == null) throw new ArgumentNullException(nameof(builder));
            }
        }
        public class SampleEntity
        {
            public Guid _id { get; set; }
            public string Name { get; set; }
        }
    }
}
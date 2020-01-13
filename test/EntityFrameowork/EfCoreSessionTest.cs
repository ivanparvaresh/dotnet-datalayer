using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using Dotnet.DataLayer.EntityFramework;

namespace Dotnet.DataLayer.Test.EntityFramework
{
    public class EfCoreSessionTest
    {
        [Fact]
        public void Constructor_Should_Check_Null_Values()
        {
            Assert.Throws<ArgumentNullException>(() => new TestSession(null));
        }
        [Fact]
        public async void Should_Open_A_Transaction_On_Construction()
        {
            // Preparation
            var sqliteConnection = new SqliteConnection("DataSource=:memory:");
            sqliteConnection.Open();

            // EXECUTION
            var dbContext = new TestDbContext(new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder().UseSqlite(sqliteConnection).Options);
            var session = new TestSession(dbContext);

            // ASSERTION
            Assert.NotNull(dbContext.Database.CurrentTransaction);
            Assert.NotNull(session.Transaction);

            // Cleaning
            await session.RollbackAsync();
            sqliteConnection.Close();
        }

        [Fact]
        public async void Should_Use_Open_A_Transaction_When_Transaction_Is_Already_Open()
        {
            // Preparation
            var sqliteConnection = new SqliteConnection("DataSource=:memory:");
            sqliteConnection.Open();

            // EXECUTION
            var dbContext = new TestDbContext(new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder().UseSqlite(sqliteConnection).Options);
            var transaction = await dbContext.Database.BeginTransactionAsync();
            var session = new TestSession(dbContext);

            // ASSERTION
            Assert.NotNull(dbContext.Database.CurrentTransaction);
            Assert.NotNull(session.Transaction);
            Assert.Equal(transaction.TransactionId, session.Transaction.TransactionId);

            // Cleaning
            await session.RollbackAsync();
            sqliteConnection.Close();
        }

        [Fact]
        public async void CommitTransaction_Should_Commit_A_Transaction()
        {
            // Preparation
            var sqliteConnection = new SqliteConnection("DataSource=:memory:");
            sqliteConnection.Open();
            var dbContext = new TestDbContext(new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder()
                .UseSqlite(sqliteConnection)
                .UseLoggerFactory(new LoggerFactory())
                .Options);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();
            var session = new TestSession(dbContext);

            // EXECUTION
            session.Samples.Add(new SampleEntity() { UniqueId = Guid.Empty, Name = "Test" });
            await session.CommitAsync();

            // ASSERTION
            var cmd = sqliteConnection.CreateCommand();
            cmd.CommandText = "SELECT * FROM sample";
            var DataAssetionHitted = false;
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (DataAssetionHitted)
                        Assert.False(DataAssetionHitted, "More that one record exists");


                    Assert.Equal("Test", (string)reader["Name"]);
                    DataAssetionHitted = true;
                }
            }
            if (!DataAssetionHitted)
                Assert.True(DataAssetionHitted, "No record found in database!");

            Assert.Null(dbContext.Database.CurrentTransaction);

            // Cleaning
            sqliteConnection.Close();
        }

        [Fact]
        public async void RollbackTransaction_Should_Revert_Changes()
        {
            // Preparation
            var sqliteConnection = new SqliteConnection("DataSource=:memory:");
            sqliteConnection.Open();
            var dbContext = new TestDbContext(new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder()
                .UseSqlite(sqliteConnection)
                .UseLoggerFactory(new LoggerFactory())
                .Options);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();

            var session = new TestSession(dbContext);

            // EXECUTION
            session.Samples.Add(new SampleEntity() { UniqueId = Guid.Empty, Name = "Test" });
            await session.RollbackAsync();

            // ASSERTION
            var cmd = sqliteConnection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM sample";
            var count = (long)cmd.ExecuteScalar();


            Assert.Null(dbContext.Database.CurrentTransaction);
            Assert.Equal((long)0, count);

            // Cleaning
            sqliteConnection.Close();
        }
        [Fact]
        public void Dispose_Should_Revert_Changes()
        {
            // Preparation
            var sqliteConnection = new SqliteConnection("DataSource=:memory:");
            sqliteConnection.Open();
            var dbContext = new TestDbContext(new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder()
                .UseSqlite(sqliteConnection)
                .UseLoggerFactory(new LoggerFactory())
                .Options);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();

            // EXECUTION
            var session = new TestSession(dbContext);
            session.Samples.Add(new SampleEntity() { Name = "TEST", UniqueId = Guid.NewGuid() });
            session.Dispose();

            // ASSERTION
            Assert.Throws<System.ObjectDisposedException>(() => session.Datasource.Database.ExecuteSqlRaw("SELECT 1"));

            // CLEANUP
            sqliteConnection.Close();
        }
        [Fact]
        public void Dispose_Should_Dispose_SessionObject()
        {
            // Preparation
            var sqliteConnection = new SqliteConnection("DataSource=:memory:");
            sqliteConnection.Open();
            var dbContext = new TestDbContext(new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder()
                .UseSqlite(sqliteConnection)
                .UseLoggerFactory(new LoggerFactory())
                .Options);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();

            var session = new TestSession(dbContext);

            // EXECUTION
            session.Dispose();

            // ASSERTION
            Assert.Throws<System.ObjectDisposedException>(() => session.Datasource.Database.ExecuteSqlRaw("SELECT 1"));

            // Cleanup
            sqliteConnection.Close();
        }

        //////////////////////////////////////////////////
        private class TestDbContext : EfCoreDatasource
        {
            public TestDbContext(DbContextOptions options) : base(options)
            { }
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ApplyConfiguration(new SampleEntityMapping());
            }
        }
        private class TestSession : EfCoreSession<TestDbContext>
        {
            public DbSet<SampleEntity> Samples { get; private set; }
            public TestSession(TestDbContext dbContext) : base(dbContext)
            {
                this.Samples = this.Datasource.Set<SampleEntity>();
            }
        }
        private class SampleEntity
        {
            public Guid UniqueId { get; set; }
            public string Name { get; set; }
        }
        private class SampleEntityMapping :
            Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<SampleEntity>
        {
            public void Configure(EntityTypeBuilder<SampleEntity> builder)
            {
                builder.ToTable("sample");
                builder.HasKey(s => s.UniqueId);
                builder.Property(s => s.UniqueId).HasColumnName("uid");
                builder.Property(s => s.Name).HasColumnName("Name");
            }
        }
    }
}
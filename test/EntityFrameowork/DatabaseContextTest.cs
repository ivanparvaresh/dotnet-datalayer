using Xunit;
using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Dotnet.DataLayer.EntityFramework;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnet.DataLayer.Test.EntityFramework
{
    public class DatabaseContextTest
    {
        [Fact]
        public void Constructor_Should_Check_Null_Values()
        {
            Assert.Throws<ArgumentNullException>(() => new TestDbContext(null));
        }

        [Fact]
        public async void BeginTransactionAsync_Should_Open_A_Transaction()
        {
            // Preparation
            var sqliteConnection = new SqliteConnection("DataSource=:memory:");
            sqliteConnection.Open();

            // EXECUTION
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder().UseSqlite(sqliteConnection).Options;
            var dbContext = new TestDbContext(options);
            await dbContext.BeginTransactionAsync();

            // ASSERTION
            Assert.NotNull(dbContext.Database.CurrentTransaction);
            Assert.NotNull(dbContext.Transaction);

            // Cleaning
            await dbContext.RollbackAsync();
            sqliteConnection.Close();
        }
        [Fact]
        public async void BeginTransactionAsync_Should_Throw_If_Already_Transaction_Has_Initiated()
        {
            // Preparation
            var sqliteConnection = new SqliteConnection("DataSource=:memory:");
            sqliteConnection.Open();

            // EXECUTION
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder().UseSqlite(sqliteConnection).Options;
            var dbContext = new TestDbContext(options);
            await dbContext.BeginTransactionAsync();

            // ASSERTION
            await Assert.ThrowsAsync<InvalidOperationException>(() => dbContext.BeginTransactionAsync());

            // Cleaning
            await dbContext.RollbackAsync();
            sqliteConnection.Close();
        }

        [Fact]
        public async void CommitAsync_Should_SaveChanges()
        {
            // Preparation
            var sqliteConnection = new SqliteConnection("DataSource=:memory:");
            sqliteConnection.Open();
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder().UseSqlite(sqliteConnection).Options;
            var dbContext = new TestDbContext(options);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();
            await dbContext.BeginTransactionAsync();


            // EXECUTION
            dbContext.Samples.Add(new SampleEntity() { UniqueId = Guid.Empty, Name = "Test" });
            await dbContext.CommitAsync();

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
        public async void CommitAsync_Should_Throws_If_No_Transaction_Initiated()
        {
            // Preparation
            var sqliteConnection = new SqliteConnection("DataSource=:memory:");
            sqliteConnection.Open();
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder().UseSqlite(sqliteConnection).Options;
            var dbContext = new TestDbContext(options);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();


            // EXECUTION AND ASSERTION
            await Assert.ThrowsAsync<InvalidOperationException>(() => dbContext.CommitAsync());

            // Cleaning
            sqliteConnection.Close();
        }


        [Fact]
        public async void RollbackAsync_Should_Revert_Changes()
        {
            // Preparation
            var sqliteConnection = new SqliteConnection("DataSource=:memory:");
            sqliteConnection.Open();
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder().UseSqlite(sqliteConnection).Options;
            var dbContext = new TestDbContext(options);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();
            await dbContext.BeginTransactionAsync();



            // EXECUTION
            dbContext.Samples.Add(new SampleEntity() { UniqueId = Guid.Empty, Name = "Test" });
            await dbContext.RollbackAsync();

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
        public async void RollbackAsync_Should_Throws_If_No_Transaction_Initiated()
        {
            // Preparation
            var sqliteConnection = new SqliteConnection("DataSource=:memory:");
            sqliteConnection.Open();
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder().UseSqlite(sqliteConnection).Options;
            var dbContext = new TestDbContext(options);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();


            // EXECUTION AND ASSERTION
            await Assert.ThrowsAsync<InvalidOperationException>(() => dbContext.RollbackAsync());

            // Cleaning
            sqliteConnection.Close();
        }


        [Fact]
        public async void Dispose_Should_Revert_Changes()
        {
            // Preparation
            var sqliteConnection = new SqliteConnection("DataSource=:memory:");
            sqliteConnection.Open();
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder().UseSqlite(sqliteConnection).Options;
            var dbContext = new TestDbContext(options);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();
            await dbContext.BeginTransactionAsync();

            // EXECUTION
            dbContext.Samples.Add(new SampleEntity() { Name = "TEST", UniqueId = Guid.NewGuid() });
            dbContext.Dispose();

            // ASSERTION
            Assert.Throws<System.ObjectDisposedException>(() => dbContext.Database.ExecuteSqlRaw("SELECT 1"));

            // CLEANUP
            sqliteConnection.Close();
        }

        //////////////////////////////////////////////////
        private class TestDbContext : DatabaseContext<TestDbContext>
        {
            public DbSet<SampleEntity> Samples { get; private set; }

            public TestDbContext(DbContextOptions options) : base(options)
            { }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ApplyConfiguration(new SampleEntityMapping());
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
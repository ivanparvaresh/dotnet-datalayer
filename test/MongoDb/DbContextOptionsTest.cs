using Xunit;
using System;
using MongoDB.Driver;
using Dotnet.DataLayer.MongoDb;


namespace Dotnet.DataLayer.Test.MognoDb
{
    [Collection("Database collection")]
    public class DbContextOptionsTest
    {
        [Fact]
        public void Constructor_Should_Check_Null_Values()
        {
            Assert.Throws<ArgumentNullException>(() => new TestDbContextOptions(null));
        }

        // INTERNAL CLASESS
        private class TestDbContextOptions : DbContextOptions
        {
            public TestDbContextOptions(MongoUrl mongoUrl) : base(mongoUrl)
            { }
        }
    }
}
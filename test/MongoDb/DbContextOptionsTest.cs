using Xunit;
using System;
using Mongo2Go;
using System.Linq;
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
            Assert.Throws<ArgumentNullException>(() => new DbContextOptions(null));
        }
    }
}
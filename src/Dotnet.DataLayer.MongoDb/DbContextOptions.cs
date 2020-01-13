using System;
using MongoDB.Driver;

namespace Dotnet.DataLayer.MongoDb
{
    public class DbContextOptions
    {
        public MongoUrl MongoUrl { get; private set; }

        public DbContextOptions(MongoUrl mongoUrl)
        {
            this.MongoUrl = mongoUrl ?? throw new ArgumentNullException(nameof(mongoUrl));
        }
    }
}
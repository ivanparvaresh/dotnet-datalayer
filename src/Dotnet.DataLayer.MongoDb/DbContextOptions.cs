using System;
using MongoDB.Driver;

namespace Dotnet.DataLayer.MongoDb
{
    public abstract class DbContextOptions
    {
        public MongoUrl MongoUrl { get; private set; }

        public DbContextOptions(MongoUrl mongoUrl)
        {
            this.MongoUrl = mongoUrl ?? throw new ArgumentNullException(nameof(mongoUrl));
        }
    }
    public class DbContextOptions<TDbContext> : DbContextOptions
        where TDbContext : DatabaseContext<TDbContext>
    {
        public DbContextOptions(MongoUrl mongoUrl) : base(mongoUrl)
        { }
    }
}
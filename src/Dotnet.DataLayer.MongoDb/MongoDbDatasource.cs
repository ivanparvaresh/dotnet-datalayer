using System;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson.Serialization;

namespace Dotnet.DataLayer.MongoDb
{
    public class MongoDbDatasource : IDatasource
    {
        public IMongoClient Client { get; private set; }
        public IMongoDatabase Database { get; private set; }

        public MongoDbDatasource([NotNullAttribute] MongoDbDatasourceOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            this.Client = this.CreateClient(options);
            this.Database = this.GetDatabase(options, this.Client);
            this.OnModelCreating();
        }

        protected internal virtual IMongoClient CreateClient(MongoDbDatasourceOptions options)
        {
            return new MongoClient(options.MongoUrl);
        }
        protected internal virtual IMongoDatabase GetDatabase(MongoDbDatasourceOptions options, IMongoClient client)
        {
            if (options.MongoUrl.DatabaseName == null) throw new ArgumentNullException(nameof(options.MongoUrl.DatabaseName));
            return client.GetDatabase(options.MongoUrl.DatabaseName);
        }
        protected internal virtual void OnModelCreating()
        { }
        public void Dispose()
        {
            this.Client = null;
            this.Database = null;
        }
    }
}
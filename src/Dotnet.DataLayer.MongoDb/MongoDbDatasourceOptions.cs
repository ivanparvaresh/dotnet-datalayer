using System;
using MongoDB.Driver;

namespace Dotnet.DataLayer.MongoDb
{
    public class MongoDbDatasourceOptions
    {
        public MongoUrl MongoUrl { get; private set; }

        public MongoDbDatasourceOptions(MongoUrl mongoUrl)
        {
            this.MongoUrl = mongoUrl ?? throw new ArgumentNullException(nameof(mongoUrl));
        }
    }
}
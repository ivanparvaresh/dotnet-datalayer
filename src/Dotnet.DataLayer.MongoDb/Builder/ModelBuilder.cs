using System;
using MongoDB.Bson.Serialization;

namespace Dotnet.DataLayer.MongoDb
{
    public class ModelBuilder
    {
        public void ApplyConfiguration<TEntity>(IEntityTypeConfiguration<TEntity> mapper) where TEntity : class
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            if (BsonClassMap.IsClassMapRegistered(typeof(TEntity))) return;
            BsonClassMap.RegisterClassMap<TEntity>((map) =>
            {
                mapper.Configure(map);
            });
        }
        public void Entity<TEntity>(Action<BsonClassMap<TEntity>> mapper) where TEntity : class
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            if (BsonClassMap.IsClassMapRegistered(typeof(TEntity))) return;
            BsonClassMap.RegisterClassMap<TEntity>((map) =>
            {
                mapper(map);
            });
        }
    }
}
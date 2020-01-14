using MongoDB.Bson.Serialization;

namespace Dotnet.DataLayer.MongoDb
{
    public interface IEntityTypeConfiguration<TEntity> where TEntity : class
    {
        void Configure(BsonClassMap<TEntity> builder);
    }
}
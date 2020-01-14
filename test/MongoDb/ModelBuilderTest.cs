using Xunit;
using System;
using System.Linq;
using MongoDB.Driver;
using Dotnet.DataLayer.MongoDb;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using System.Collections.Generic;
using System.Reflection;

namespace Dotnet.DataLayer.Test.MognoDb
{
    [Collection("Database collection")]
    public class ModelBuilderTest
    {
        [Fact]
        public void ApplyConfiguration_Should_Check_Mapper_NullValue()
        {
            // PREPARATION
            var builder = new ModelBuilder();

            // ASSERTION AND EXECUTION
            Assert.Throws<ArgumentNullException>(() => builder.ApplyConfiguration<SampleEntity>(null));
        }
        [Fact]
        public void Entity_Should_Check_Mapper_NullValue()
        {
            // PREPARATION
            var builder = new ModelBuilder();
            GetClassMap().Clear();

            // ASSERTION AND EXECUTION
            Assert.Throws<ArgumentNullException>(() => builder.Entity<SampleEntity>(null));
        }

        [Fact]
        public void ApplyConfiguration_Execute_Configuration_If_Has_Not_Registered_Yet()
        {
            // PREPARATION
            var builder = new ModelBuilder();
            GetClassMap().Clear();
            var mapper = new SampleEntotyMap();

            // EXECUTION
            builder.ApplyConfiguration(mapper);

            // ASSERTION
            Assert.True(mapper.ConfigureMethodExecuted);
        }
        [Fact]
        public void ApplyConfiguration_NOT_Execute_COnfiguration_If_Entity_Has_Already_Registered()
        {
            // PREPARATION
            var builder = new ModelBuilder();
            GetClassMap().Clear();
            builder.ApplyConfiguration(new SampleEntotyMap());

            // EXECUTION
            var mapper = new SampleEntotyMap();
            builder.ApplyConfiguration(new SampleEntotyMap());

            // ASSERTION
            Assert.False(mapper.ConfigureMethodExecuted);
        }

        [Fact]
        public void Entity_Execute_Configuration_If_Has_Not_Registered_Yet()
        {
            // PREPARATION
            var builder = new ModelBuilder();
            GetClassMap().Clear();
            var mapper = new SampleEntotyMap();

            // EXECUTION
            var executed = false;
            builder.Entity<SampleEntity>((c) => { executed = true; });

            // ASSERTION
            Assert.True(executed);
        }
        [Fact]
        public void Entity_NOT_Execute_COnfiguration_If_Entity_Has_Already_Registered()
        {
            // PREPARATION
            var builder = new ModelBuilder();
            GetClassMap().Clear();
            builder.ApplyConfiguration(new SampleEntotyMap());

            // EXECUTION
            var executed = false;
            builder.Entity<SampleEntity>((c) => { executed = true; });

            // ASSERTION
            Assert.False(executed);
        }



        // Internal Classes
        private class TestDbContext : DatabaseContext<TestDbContext>
        {
            public bool HasOnModelCreatingExecuted { get; private set; }
            public bool HasCreateClientExecuted { get; private set; }
            public bool HasGetDatabaseExecuted { get; private set; }
            public IMongoCollection<SampleEntity> SampleEntities { get; private set; }

            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
            {
                this.SampleEntities = this.Database.GetCollection<SampleEntity>("SampleEntity");
            }

            protected override IMongoClient CreateClient(DbContextOptions<TestDbContext> options)
            {
                this.HasCreateClientExecuted = true;
                return base.CreateClient(options);
            }
            protected override IMongoDatabase GetDatabase(DbContextOptions<TestDbContext> options, IMongoClient client)
            {
                this.HasGetDatabaseExecuted = true;
                return base.GetDatabase(options, client);
            }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                this.HasOnModelCreatingExecuted = true;
                if (builder == null) throw new ArgumentNullException(nameof(builder));
            }
        }
        public class SampleEntity
        {
            public Guid _id { get; set; }
            public string Name { get; set; }
        }

        public class SampleEntotyMap : IEntityTypeConfiguration<SampleEntity>
        {
            public bool ConfigureMethodExecuted;
            public void Configure(BsonClassMap<SampleEntity> builder)
            {
                this.ConfigureMethodExecuted = true;
            }
        }

        static Dictionary<Type, BsonClassMap> GetClassMap()
        {
            var cm = BsonClassMap.GetRegisteredClassMaps().First();
            var fi = typeof(BsonClassMap).GetField("__classMaps", BindingFlags.Static | BindingFlags.NonPublic);
            var classMaps = (Dictionary<Type, BsonClassMap>)fi.GetValue(cm);
            return classMaps;
        }
    }
}
using System;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Dotnet.DataLayer.MongoDb
{
    public class DatabaseContext<TDatabaseContext> : IDatabaseContext<TDatabaseContext>
        where TDatabaseContext : DatabaseContext<TDatabaseContext>,
                                 IDatabaseContext<TDatabaseContext>,
                                 IDatabaseContext
    {
        public IMongoClient Client { get; private set; }
        public IMongoDatabase Database { get; private set; }
        public IClientSessionHandle SessionHandler { get; private set; }
        private readonly ModelBuilder modelBuilder;

        public DatabaseContext([NotNullAttribute] DbContextOptions<TDatabaseContext> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            this.Client = this.CreateClient(options);
            this.Database = this.GetDatabase(options, this.Client);

            // we should run model creation once
            this.modelBuilder = new ModelBuilder();
            this.OnModelCreating(modelBuilder);
        }

        protected internal virtual IMongoClient CreateClient(DbContextOptions<TDatabaseContext> options)
        {
            return new MongoClient(options.MongoUrl);
        }
        protected internal virtual IMongoDatabase GetDatabase(DbContextOptions<TDatabaseContext> options, IMongoClient client)
        {
            if (options.MongoUrl.DatabaseName == null) throw new ArgumentNullException(nameof(options.MongoUrl.DatabaseName));
            return client.GetDatabase(options.MongoUrl.DatabaseName);
        }
        protected internal virtual void OnModelCreating(ModelBuilder builder)
        { }

        public Task BeginTransactionAsync()
        {
            if (this.SessionHandler != null)
                throw new InvalidOperationException("Already transaction initiated!");

            this.SessionHandler = this.Client.StartSession();
            this.SessionHandler.StartTransaction();
            return Task.CompletedTask;
        }
        public async Task CommitAsync()
        {
            if (this.SessionHandler == null)
                throw new InvalidOperationException("No transaction to commit");

            await this.SessionHandler.CommitTransactionAsync();
        }
        public async Task RollbackAsync()
        {
            if (this.SessionHandler == null)
                throw new InvalidOperationException("No transaction to commit");

            await this.SessionHandler.AbortTransactionAsync();
        }
        public void Dispose()
        {
            if (this.SessionHandler != null)
            {
                if (this.SessionHandler.IsInTransaction)
                    this.SessionHandler.AbortTransaction();
                this.SessionHandler.Dispose();
                this.SessionHandler = null;
            }
            this.Client = null;
            this.Database = null;
        }
    }
}
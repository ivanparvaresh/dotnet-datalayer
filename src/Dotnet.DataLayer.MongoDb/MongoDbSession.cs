using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;

namespace Dotnet.DataLayer.MongoDb
{
    public class MongoDbSession<TDatasource> : ISession<TDatasource>
        where TDatasource : MongoDbDatasource, IDatasource
    {
        public IClientSessionHandle SessionHandler { get; private set; }
        public TDatasource Datasource { get; private set; }

        public MongoDbSession([NotNullAttribute] TDatasource datasource)
        {
            this.Datasource = datasource ?? throw new ArgumentNullException(nameof(datasource));

            // start the session
            this.SessionHandler = datasource.Client.StartSession();
            this.SessionHandler.StartTransaction();
        }

        public async Task CommitAsync()
        {
            await this.SessionHandler.CommitTransactionAsync();
        }
        public async Task RollbackAsync()
        {
            await this.SessionHandler.AbortTransactionAsync();
        }
        public void Dispose()
        {
            if (this.SessionHandler.IsInTransaction)
                this.SessionHandler.AbortTransaction();

            this.SessionHandler.Dispose();
            this.SessionHandler = null;
        }
    }
}
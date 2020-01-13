using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dotnet.DataLayer.EntityFramework
{
    public class EfCoreSession<TDatasource> : ISession<TDatasource>
        where TDatasource : DbContext, IDatasource
    {
        public TDatasource Datasource { get; private set; }
        public IDbContextTransaction Transaction { get; private set; }

        public EfCoreSession([NotNullAttribute] TDatasource datasource)
        {
            this.Datasource = datasource ?? throw new ArgumentNullException(nameof(datasource));

            // start transaction
            this.Transaction = this.Datasource.Database.CurrentTransaction == null ?
                this.Datasource.Database.BeginTransaction(): 
                this.Datasource.Database.CurrentTransaction;
        }

        public async Task CommitAsync()
        {
            await this.Datasource.SaveChangesAsync();
            await this.Transaction.CommitAsync();
        }
        public async Task RollbackAsync()
        {
            await this.Transaction.RollbackAsync();
        }
        public void Dispose()
        {
            if (this.Transaction != null)
                this.Transaction.Rollback();

            this.Datasource.Dispose();
        }
    }
}
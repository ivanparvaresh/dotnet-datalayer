using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dotnet.DataLayer.EntityFramework
{
    public class DatabaseContext<TDatabaseContext> : DbContext, IDatabaseContext<TDatabaseContext>, IDatabaseContext
        where TDatabaseContext : DatabaseContext<TDatabaseContext>,
                                 IDatabaseContext<TDatabaseContext>,
                                 IDatabaseContext
    {
        public IDbContextTransaction Transaction { get; private set; }

        public DatabaseContext([NotNullAttribute] DbContextOptions options) : base(options)
        { }

        public async Task BeginTransactionAsync()
        {
            if (this.Transaction != null)
                throw new InvalidOperationException("Already transaction initiated!");
            this.Transaction = await base.Database.BeginTransactionAsync();
        }
        public async Task CommitAsync()
        {
            if (this.Transaction == null)
                throw new InvalidOperationException("No transaction to commit");

            await this.SaveChangesAsync();
            await this.Transaction.CommitAsync();
        }
        public async Task RollbackAsync()
        {
            if (this.Transaction == null)
                throw new InvalidOperationException("No transaction to commit");

            await this.Transaction.RollbackAsync();
        }
    }
}
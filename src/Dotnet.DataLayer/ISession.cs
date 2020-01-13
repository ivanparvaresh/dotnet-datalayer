using System;
using System.Threading.Tasks;

namespace Dotnet.DataLayer
{
    public interface ISession
    {
        Task CommitAsync();
        Task RollbackAsync();
    }
    public interface ISession<TDatasource> : IDisposable, ISession
        where TDatasource : IDatasource
    {
        TDatasource Datasource { get; }
    }
}

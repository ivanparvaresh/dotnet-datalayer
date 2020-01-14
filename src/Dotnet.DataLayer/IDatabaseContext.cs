using System.Threading.Tasks;

namespace Dotnet.DataLayer
{
    public interface IDatabaseContext
    {
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
    public interface IDatabaseContext<TDatabaseContext> : IDatabaseContext
        where TDatabaseContext : IDatabaseContext
    {
        
    }
}

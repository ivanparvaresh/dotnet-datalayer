using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;


namespace Dotnet.DataLayer.EntityFramework
{
    public class EfCoreDatasource : DbContext, IDatasource
    {
        public EfCoreDatasource([NotNullAttribute] DbContextOptions options) : base(options)
        { }
    }
}
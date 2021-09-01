using Quartz.Impl.AdoJobStore;
using Quartz.Impl.AdoJobStore.Common;

namespace Blazor.Quartz.Core.Repositories
{
    public class RepositorieFactory
    {
        public static IRepositorie CreateRepositorie(string driverDelegateType, IDbProvider dbProvider)
        {

            if (driverDelegateType == typeof(SQLiteDelegate).AssemblyQualifiedName)
            {
                return new RepositorieSQLite(dbProvider);
            }
            else if (driverDelegateType == typeof(MySQLDelegate).AssemblyQualifiedName)
            {
                return new RepositorieMySql(dbProvider);
            }
            else if (driverDelegateType == typeof(SqlServerDelegate).AssemblyQualifiedName)
            {
                return new RepositorieSqlServer(dbProvider);
            }
            else
            {
                return null;
            }
        }
    }
}

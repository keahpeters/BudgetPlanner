using System.Data.Common;

namespace BudgetPlanner.Repositories
{
    public interface IDbConnectionFactory
    {
        DbConnection Create();
    }
}

using System.Data;

namespace BudgetPlanner.Repositories
{
    public interface IDbConnectionFactory
    {
        IDbConnection Create();
    }
}

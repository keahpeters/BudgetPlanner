using System.Data;
using System.Data.SqlClient;

namespace BudgetPlanner.Repositories
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IDbConnection Create()
        {
            return new SqlConnection(this.connectionString);
        }
    }
}

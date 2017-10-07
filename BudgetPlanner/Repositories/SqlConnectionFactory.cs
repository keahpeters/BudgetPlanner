using System.Data.Common;
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

        public DbConnection Create()
        {
            return new SqlConnection(this.connectionString);
        }
    }
}

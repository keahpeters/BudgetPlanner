using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BudgetPlanner.Infrastructure;
using BudgetPlanner.Models;
using Dapper;

namespace BudgetPlanner.Repositories
{
    public interface IUserRepository
    {
        Task CreateAsync(ApplicationUser user);

        Task<ApplicationUser> FindByName(string userName);
    }

    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory dbConnectionFactory;

        public UserRepository(IDbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory;
        }

        public async Task CreateAsync(ApplicationUser user)
        {
            const string sql = @"INSERT INTO dbo.ApplicationUser (Id, Email, EmailConfirmed, PasswordHash, UserName)
                           VALUES (@Id, @Email, @EmailConfirmed, @PasswordHash, @UserName)";

            using (DbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                try
                {
                    int rows = await dbConnection.ExecuteAsync(sql, new
                    {
                        user.Id,
                        user.Email,
                        user.EmailConfirmed,
                        user.PasswordHash,
                        user.UserName
                    });

                    if (rows == 0)
                        throw new RepositoryException("Failed to add user");
                }
                catch (SqlException ex)
                {
                    throw new RepositoryException("Failed to add user", ex);
                }
            }
        }

        public async Task<ApplicationUser> FindByName(string userName)
        {
            const string sql = @"SELECT Id, Email, PasswordHash, UserName
                            FROM ApplicationUser
                            WHERE UserName = @UserName";

            using (DbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                IEnumerable<ApplicationUser> user =
                    await dbConnection.QueryAsync<ApplicationUser>(sql, new {UserName = userName});

                return user.FirstOrDefault();
            }
        }
    }
}
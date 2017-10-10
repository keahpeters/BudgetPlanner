using System;
using System.Collections.Generic;
using System.Data;
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
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task CreateAsync(ApplicationUser user)
        {
            const string sql = @"INSERT INTO ApplicationUser (Id, Email, PasswordHash, UserName)
                           VALUES (@Id, @Email, @PasswordHash, @UserName)";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                try
                {
                    await dbConnection.ExecuteAsync(sql, new
                    {
                        user.Id,
                        user.Email,
                        user.PasswordHash,
                        user.UserName
                    }).ConfigureAwait(false);
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

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                IEnumerable<ApplicationUser> user = await dbConnection.QueryAsync<ApplicationUser>(sql, new { UserName = userName }).ConfigureAwait(false);

                return user.FirstOrDefault();
            }
        }
    }
}
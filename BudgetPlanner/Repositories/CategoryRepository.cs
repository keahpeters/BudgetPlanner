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
    public interface ICategoryRepository
    {
        Task Add(Category category);
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDbConnectionFactory dbConnectionFactory;

        public CategoryRepository(IDbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task Add(Category category)
        {
            const string existsSql = "SELECT 1 FROM Category WHERE Name = @Name AND CategoryGroupId = @CategoryGroupId";
            const string insertSql = @"INSERT INTO Category (Name, CategoryGroupId)
                                    VALUES (@Name, @CategoryGroupId)";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                IEnumerable<int> result = await dbConnection.QueryAsync<int>(existsSql, new { category.Name, category.CategoryGroupId }).ConfigureAwait(false);

                if (result.Any())
                    throw new EntityAlreadyExistsException("Category already exists in category group");
            }

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                try
                {
                    await dbConnection.ExecuteAsync(insertSql, new
                    {
                        category.Name,
                        category.CategoryGroupId
                    }).ConfigureAwait(false);
                }
                catch (SqlException ex)
                {
                    throw new RepositoryException("Failed to add category", ex);
                }
            }
        }
    }
}

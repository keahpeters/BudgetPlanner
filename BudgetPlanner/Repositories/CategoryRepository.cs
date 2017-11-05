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
        Task<Category> Get(int id);

        Task Add(Category category);

        Task Update(Category category);
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDbConnectionFactory dbConnectionFactory;

        public CategoryRepository(IDbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<Category> Get(int id)
        {
            const string sql = "SELECT Id, Name, CategoryGroupId, Budget FROM Category WHERE Id = @id";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                IEnumerable<Category> results = await dbConnection.QueryAsync<Category>(sql, new { id }).ConfigureAwait(false);

                return results.FirstOrDefault();
            }
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

        public async Task Update(Category category)
        {
            const string existsSql = "SELECT 1 FROM Category WHERE Name = @Name AND CategoryGroupId = @CategoryGroupId";
            const string updateSql = @"UPDATE Category
                                        SET Name = @Name
                                        WHERE Id = @Id";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                IEnumerable<int> result = await dbConnection.QueryAsync<int>(existsSql, new { category.Name, category.CategoryGroupId }).ConfigureAwait(false);

                if (result.Any())
                    throw new EntityAlreadyExistsException("Category group already exists");
            }

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                try
                {
                    await dbConnection.ExecuteAsync(updateSql, new
                    {
                        category.Id,
                        category.Name
                    }).ConfigureAwait(false);
                }
                catch (SqlException ex)
                {
                    throw new RepositoryException("Failed to update category", ex);
                }
            }
        }
    }
}

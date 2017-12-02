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
        Task<Category> GetAsync(int id);

        Task<IEnumerable<Category>> GetAsync(Guid userId);

        Task AddAsync(Category category);

        Task UpdateAsync(Category category);

        Task DeleteAsync(int id);

        Task AssignMoneyAsync(int id, decimal amount, Guid userId);

        Task ReassignMoneyAsync(int sourceCategoryid, int destinationCategoryId, decimal amount);

        Task UnassignMoneyAsync(int sourceCategoryid, decimal amount, Guid userId);
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDbConnectionFactory dbConnectionFactory;

        public CategoryRepository(IDbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<Category> GetAsync(int id)
        {
            const string sql = "SELECT Id, Name, CategoryGroupId, Budget FROM Category WHERE Id = @id";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                IEnumerable<Category> results = await dbConnection.QueryAsync<Category>(sql, new { id }).ConfigureAwait(false);

                return results.FirstOrDefault();
            }
        }

        public async Task<IEnumerable<Category>> GetAsync(Guid userId)
        {
            const string sql = @"SELECT c.Id, c.Name, CategoryGroupId, cg.Name AS CategoryGroup, Budget 
                                FROM Category c INNER JOIN CategoryGroup cg ON c.CategoryGroupId = cg.Id
                                WHERE UserId = @userId
                                ORDER BY c.Name";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                return await dbConnection.QueryAsync<Category>(sql, new { userId }).ConfigureAwait(false);
            }
        }

        public async Task AddAsync(Category category)
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

        public async Task UpdateAsync(Category category)
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

        public async Task DeleteAsync(int id)
        {
            Category category = await this.GetAsync(id);

            if (category.Budget != 0)
                throw new RepositoryException("Category can not be deleted when budget is not 0.00");

            const string sql = "DELETE FROM Category WHERE Id = @id";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                try
                {
                    await dbConnection.ExecuteAsync(sql, new { id }).ConfigureAwait(false);
                }
                catch (SqlException ex)
                {
                    throw new RepositoryException("Failed to delete category", ex);
                }
            }
        }

        public async Task AssignMoneyAsync(int id, decimal amount, Guid userId)
        {
            const string subtractFromBalanceSql = @"UPDATE ApplicationUser
                                                    SET Balance = Balance - @amount
                                                    WHERE Id = @userId";

            const string addToCategorySql = @"UPDATE Category
                                        SET Budget = Budget + @amount
                                        WHERE Id = @id";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                dbConnection.Open();

                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        await dbConnection.ExecuteAsync(subtractFromBalanceSql, new { amount, userId }, dbTransaction);
                        await dbConnection.ExecuteAsync(addToCategorySql, new { amount, id }, dbTransaction);

                        dbTransaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        throw new RepositoryException("Failed to assign money", ex);
                    }
                }
            }
        }

        public async Task ReassignMoneyAsync(int sourceCategoryid, int destinationCategoryId, decimal amount)
        {
            const string subtractFromCategorySql = @"UPDATE Category
                                        SET Budget = Budget - @amount
                                        WHERE Id = @sourceCategoryid";

            const string addToCategorySql = @"UPDATE Category
                                        SET Budget = Budget + @amount
                                        WHERE Id = @destinationCategoryId";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                dbConnection.Open();

                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        await dbConnection.ExecuteAsync(subtractFromCategorySql, new { amount, sourceCategoryid }, dbTransaction);
                        await dbConnection.ExecuteAsync(addToCategorySql, new { amount, destinationCategoryId }, dbTransaction);

                        dbTransaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        throw new RepositoryException("Failed to reassign money", ex);
                    }
                }
            }
        }

        public async Task UnassignMoneyAsync(int sourceCategoryid, decimal amount, Guid userId)
        {
            const string subtractFromCategorySql = @"UPDATE Category
                                        SET Budget = Budget - @amount
                                        WHERE Id = @sourceCategoryid";

            const string addToBalanceSql = @"UPDATE ApplicationUser
                                                    SET Balance = Balance + @amount
                                                    WHERE Id = @userId";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                dbConnection.Open();

                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        await dbConnection.ExecuteAsync(subtractFromCategorySql, new { amount, sourceCategoryid }, dbTransaction);
                        await dbConnection.ExecuteAsync(addToBalanceSql, new { amount, userId }, dbTransaction);

                        dbTransaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        throw new RepositoryException("Failed to unassign money", ex);
                    }
                }
            }
        }
    }
}

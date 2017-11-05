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
    public interface ICategoryGroupRepository
    {
        Task<IEnumerable<CategoryGroup>> GetByUserNameAsync(string userName);

        Task Add(CategoryGroup categoryGroup);
    }

    public class CategoryGroupRepository : ICategoryGroupRepository
    {
        private readonly IDbConnectionFactory dbConnectionFactory;

        public CategoryGroupRepository(IDbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<IEnumerable<CategoryGroup>> GetByUserNameAsync(string userName)
        {
            const string sql = @"SELECT
	                                cg.Id, cg.Name, c.Id, c.Name, c.Budget
                                FROM 
	                                CategoryGroup cg
	                                LEFT OUTER JOIN Category c ON cg.Id = c.CategoryGroupId
	                                INNER JOIN ApplicationUser au ON cg.UserId = au.Id
                                WHERE
	                                au.UserName = @userName";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                var lookup = new Dictionary<int, CategoryGroup>();

                await dbConnection.QueryAsync<CategoryGroup, Category, CategoryGroup>(
                    sql,
                    (cg, c) =>
                    {
                        if (!lookup.TryGetValue(cg.Id, out CategoryGroup categoryGroup))
                        {
                            lookup.Add(cg.Id, cg);
                            categoryGroup = cg;
                        }

                        if (categoryGroup.Categories == null)
                            categoryGroup.Categories = new List<Category>();

                        if (c != null)
                            categoryGroup.Categories.Add(c);

                        return categoryGroup;
                    },
                    new { userName }
                );

                return lookup.Values;
            }
        }

        public async Task Add(CategoryGroup categoryGroup)
        {
            const string existsSql = "SELECT 1 FROM CategoryGroup WHERE Name = @Name";
            const string insertSql = @"INSERT INTO CategoryGroup (UserId, Name)
                                    VALUES (@UserId, @Name)";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                IEnumerable<int> result = await dbConnection.QueryAsync<int>(existsSql, new { categoryGroup.Name }).ConfigureAwait(false);

                if (result.Any())
                {
                    throw new EntityAlreadyExistsException("Category group already exists");
                }
            }

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                try
                {
                    await dbConnection.ExecuteAsync(insertSql, new
                    {
                        categoryGroup.UserId,
                        categoryGroup.Name
                    }).ConfigureAwait(false);
                }
                catch (SqlException ex)
                {
                    throw new RepositoryException("Failed to add category group", ex);
                }
            }
        }
    }
}
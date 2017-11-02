using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using BudgetPlanner.Models;
using Dapper;

namespace BudgetPlanner.Repositories
{
    public interface ICategoryGroupRepository
    {
        Task<IEnumerable<CategoryGroup>> GetByUserNameAsync(string userName);
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
	                                INNER JOIN Category c ON cg.Id = c.CategoryGroupId
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
                        categoryGroup.Categories.Add(c);
                        return categoryGroup;
                    },
                    new { userName }
                );

                return lookup.Values;
            }
        }
    }
}
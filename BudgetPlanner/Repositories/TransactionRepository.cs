using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using BudgetPlanner.Models;
using Dapper;

namespace BudgetPlanner.Repositories
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> Get(Guid userId);
    }

    public class TransactionRepository : ITransactionRepository
    {
        private readonly IDbConnectionFactory dbConnectionFactory;

        public TransactionRepository(IDbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<IEnumerable<Transaction>> Get(Guid userId)
        {
            const string sql = @"SELECT t.Id, UserId, [Date], c.Name, Amount, Payee, Memo, IsInTransaction
                                FROM [Transaction] t 
                                LEFT OUTER JOIN Category c ON t.CategoryId = c.Id
                                WHERE UserId = @UserId
                                ORDER BY Date DESC";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                return await dbConnection.QueryAsync<Transaction>(sql, new { userId }).ConfigureAwait(false);
            }
        }
    }
}
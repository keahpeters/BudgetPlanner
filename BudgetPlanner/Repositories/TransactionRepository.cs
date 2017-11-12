using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BudgetPlanner.Infrastructure;
using BudgetPlanner.Models;
using Dapper;
using Microsoft.Rest;

namespace BudgetPlanner.Repositories
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> Get(Guid userId);

        Task Add(Transaction transaction);
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
            const string sql = @"SELECT t.Id, UserId, [Date], c.Name AS Category, Amount, Payee, Memo, IsInTransaction
                                FROM [Transaction] t 
                                LEFT OUTER JOIN Category c ON t.CategoryId = c.Id
                                WHERE UserId = @UserId
                                ORDER BY Date DESC";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                return await dbConnection.QueryAsync<Transaction>(sql, new { userId }).ConfigureAwait(false);
            }
        }

        public async Task Add(Transaction transaction)
        {
            const string insertTransactionSql = @"INSERT INTO [Transaction] (UserId, CategoryId, Date, Amount, Payee, Memo, IsInTransaction)
                                                    VALUES(@UserId, @CategoryId, @Date, @Amount, @Payee, @Memo, @IsInTransaction)";

            string updateBalanceSql = $@"UPDATE ApplicationUser
                                        SET Balance = Balance {(transaction.IsInTransaction ? "+" : "-")} @Amount
                                        WHERE Id = @UserId";

            string updateCategorySql = $@"Update Category
                                        SET Budget = Budget {(transaction.IsInTransaction ? "+" : "-")} @Amount
                                        WHERE Id = @CategoryId";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                dbConnection.Open();

                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        await dbConnection.ExecuteAsync(
                            insertTransactionSql,
                            new
                            {
                                transaction.UserId,
                                transaction.CategoryId,
                                transaction.Date,
                                transaction.Amount,
                                transaction.Payee,
                                transaction.Memo,
                                transaction.IsInTransaction
                            },
                            dbTransaction);

                        if (transaction.CategoryId == null)
                            await dbConnection.ExecuteAsync(updateBalanceSql, new { transaction.Amount, transaction.UserId }, dbTransaction);
                        else
                            await dbConnection.ExecuteAsync(updateCategorySql, new { transaction.Amount, transaction.CategoryId }, dbTransaction);

                        dbTransaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        throw new RepositoryException("Failed to add transaction", ex);
                    }
                }
            }
        }
    }
}
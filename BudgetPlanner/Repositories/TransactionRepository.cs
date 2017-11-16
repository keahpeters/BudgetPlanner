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
    public interface ITransactionRepository
    {
        Task<Transaction> Get(int id);

        Task<IEnumerable<Transaction>> Get(Guid userId);

        Task Add(Transaction transaction);

        Task Update(Transaction transaction);

        Task UpdateAndReapplyTransaction(Transaction newTransaction, Transaction oldTransaction);

        Task Delete(int id);
    }

    public class TransactionRepository : ITransactionRepository
    {
        private readonly IDbConnectionFactory dbConnectionFactory;

        public TransactionRepository(IDbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<Transaction> Get(int id)
        {
            const string sql = "select Id, UserId, Date, CategoryId, Amount, Payee, Memo, IsInTransaction from [Transaction] WHERE Id = @id";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                IEnumerable<Transaction> results = await dbConnection.QueryAsync<Transaction>(sql, new { id }).ConfigureAwait(false);

                return results.FirstOrDefault();
            }
        }

        public async Task<IEnumerable<Transaction>> Get(Guid userId)
        {
            const string sql = @"SELECT t.Id, UserId, [Date], c.Name AS Category, Amount, Payee, Memo, IsInTransaction
                                FROM [Transaction] t 
                                LEFT OUTER JOIN Category c ON t.CategoryId = c.Id
                                WHERE UserId = @UserId
                                ORDER BY Date DESC, Amount DESC";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                return await dbConnection.QueryAsync<Transaction>(sql, new { userId }).ConfigureAwait(false);
            }
        }

        public async Task Add(Transaction transaction)
        {
            const string insertTransactionSql = @"INSERT INTO [Transaction] (UserId, CategoryId, Date, Amount, Payee, Memo, IsInTransaction)
                                                    VALUES(@UserId, @CategoryId, @Date, @Amount, @Payee, @Memo, @IsInTransaction)";

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
                            dbTransaction).ConfigureAwait(false);

                        await this.UpdateBudget(transaction, dbConnection, dbTransaction).ConfigureAwait(false);

                        dbTransaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        throw new RepositoryException("Failed to add transaction", ex);
                    }
                }
            }
        }

        public async Task Update(Transaction transaction)
        {
            const string sql = @"Update [Transaction]
                                SET Date = @Date, Payee = @Payee, Memo = @Memo
                                WHERE Id = @Id";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                try
                {
                    await dbConnection.ExecuteAsync(sql, new
                    {
                        transaction.Id,
                        transaction.Date,
                        transaction.Payee,
                        transaction.Memo
                    }).ConfigureAwait(false);
                }
                catch (SqlException ex)
                {
                    throw new RepositoryException("Failed to update transaction", ex);
                }
            }
        }

        public async Task UpdateAndReapplyTransaction(Transaction newTransaction, Transaction oldTransaction)
        {
            const string updateTransactionSql = @"Update [Transaction]
                                SET Date = @Date, Payee = @Payee, Memo = @Memo, CategoryId = @CategoryId, Amount = @Amount, IsInTransaction = @IsInTransaction
                                WHERE Id = @Id";

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                dbConnection.Open();

                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        await dbConnection.ExecuteAsync(
                            updateTransactionSql,
                            new
                            {
                                newTransaction.CategoryId,
                                newTransaction.Date,
                                newTransaction.Amount,
                                newTransaction.Payee,
                                newTransaction.Memo,
                                newTransaction.IsInTransaction,
                                newTransaction.Id
                            },
                            dbTransaction).ConfigureAwait(false);

                        await this.UndoTransaction(oldTransaction, dbConnection, dbTransaction).ConfigureAwait(false);
                        await this.UpdateBudget(newTransaction, dbConnection, dbTransaction).ConfigureAwait(false);

                        dbTransaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        throw new RepositoryException("Failed to update transaction", ex);
                    }
                }
            }
        }

        public async Task Delete(int id)
        {
            const string sql = "DELETE FROM [Transaction] WHERE Id = @id";

            Transaction transaction = await this.Get(id).ConfigureAwait(false);

            using (IDbConnection dbConnection = this.dbConnectionFactory.Create())
            {
                dbConnection.Open();

                using (IDbTransaction dbTransaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        await this.UndoTransaction(transaction, dbConnection, dbTransaction).ConfigureAwait(false);

                        await dbConnection.ExecuteAsync(sql, new { id }, dbTransaction).ConfigureAwait(false);

                        dbTransaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        throw new RepositoryException("Failed to delete category", ex);
                    }
                }
            }
        }

        private async Task UpdateBudget(Transaction transaction, IDbConnection dbConnection, IDbTransaction dbTransaction)
        {
            if (transaction.CategoryId == null)
            {
                await this.UpdateBalance(transaction, dbConnection, dbTransaction).ConfigureAwait(false);
            }
            else
            {
                await this.UpdateCategory(transaction, dbConnection, dbTransaction).ConfigureAwait(false);
            }
        }

        private async Task UpdateCategory(Transaction transaction, IDbConnection dbConnection, IDbTransaction dbTransaction)
        {
            string updateCategorySql = $@"Update Category
                                        SET Budget = Budget {(transaction.IsInTransaction ? "+" : "-")} @Amount
                                        WHERE Id = @CategoryId";

            await dbConnection.ExecuteAsync(updateCategorySql, new { transaction.Amount, transaction.CategoryId }, dbTransaction);
        }

        private async Task UpdateBalance(Transaction transaction, IDbConnection dbConnection, IDbTransaction dbTransaction)
        {
            string updateBalanceSql = $@"UPDATE ApplicationUser
                                        SET Balance = Balance {(transaction.IsInTransaction ? "+" : "-")} @Amount
                                        WHERE Id = @UserId";

            await dbConnection.ExecuteAsync(updateBalanceSql, new { transaction.Amount, transaction.UserId }, dbTransaction);
        }

        private async Task UndoTransaction(Transaction transaction, IDbConnection dbConnection, IDbTransaction dbTransaction)
        {
            if (transaction.CategoryId == null)
            {
                await this.UndoTransactionBalance(transaction, dbConnection, dbTransaction).ConfigureAwait(false);
            }
            else
            {
                await this.UndoTransactionCategory(transaction, dbConnection, dbTransaction).ConfigureAwait(false);
            }
        }

        private async Task UndoTransactionCategory(Transaction transaction, IDbConnection dbConnection, IDbTransaction dbTransaction)
        {
            string updateCategorySql = $@"Update Category
                                        SET Budget = Budget {(transaction.IsInTransaction ? "-" : "+")} @Amount
                                        WHERE Id = @CategoryId";

            await dbConnection.ExecuteAsync(updateCategorySql, new { transaction.Amount, transaction.CategoryId }, dbTransaction);
        }

        private async Task UndoTransactionBalance(Transaction transaction, IDbConnection dbConnection, IDbTransaction dbTransaction)
        {
            string updateBalanceSql = $@"UPDATE ApplicationUser
                                        SET Balance = Balance {(transaction.IsInTransaction ? "-" : "+")} @Amount
                                        WHERE Id = @UserId";

            await dbConnection.ExecuteAsync(updateBalanceSql, new { transaction.Amount, transaction.UserId }, dbTransaction);
        }
    }
}
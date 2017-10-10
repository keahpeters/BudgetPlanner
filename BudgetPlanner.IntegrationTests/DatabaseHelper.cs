using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using ServiceStack.OrmLite;

namespace BudgetPlanner.IntegrationTests
{
    public class DatabaseHelper
    {
        private readonly OrmLiteConnectionFactory dbFactory = new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider);

        public IDbConnection OpenConnection()
        {
            return this.dbFactory.OpenDbConnection();
        }

        public void DropAndCreateTable<T>()
        {
            using (IDbConnection db = this.OpenConnection())
            {
                db.DropAndCreateTable<T>();
            }
        }

        public List<T> Select<T>(Expression<Func<T, bool>> predicate)
        {
            using (IDbConnection db = this.OpenConnection())
            {
                return db.Select(predicate);
            }
        }

        public void Insert<T>(IEnumerable<T> items)
        {
            using (IDbConnection db = this.OpenConnection())
            {
                foreach (T item in items)
                    db.Insert(item);
            }
        }
    }
}
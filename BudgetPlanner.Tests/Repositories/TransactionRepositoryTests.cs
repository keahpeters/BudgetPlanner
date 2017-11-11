using System;
using BudgetPlanner.Repositories;
using NUnit.Framework;

namespace BudgetPlanner.Tests.Repositories
{
    [TestFixture]
    public class TransactionRepositoryTests
    {
        private TransactionRepository transactionRepository;

        [Test]
        public void WhenDbConnectionFactoryIsNullThenThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => this.transactionRepository = new TransactionRepository(null));
        }
    }
}
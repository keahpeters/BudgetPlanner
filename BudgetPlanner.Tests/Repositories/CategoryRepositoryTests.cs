using System;
using BudgetPlanner.Repositories;
using NUnit.Framework;

namespace BudgetPlanner.Tests.Repositories
{
    [TestFixture]
    public class CategoryRepositoryTests
    {
        private CategoryRepository categoryRepository;

        [Test]
        public void WhenDbConnectionFactoryIsNullThenThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => this.categoryRepository = new CategoryRepository(null));
        }
    }
}
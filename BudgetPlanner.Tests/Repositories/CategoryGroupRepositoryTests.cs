using System;
using BudgetPlanner.Repositories;
using NUnit.Framework;

namespace BudgetPlanner.Tests.Repositories
{
    [TestFixture]
    public class CategoryGroupRepositoryTests
    {
        private CategoryGroupRepository categoryGroupRepository;

        [Test]
        public void WhenDbConnectionFactoryIsNullThenThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => this.categoryGroupRepository = new CategoryGroupRepository(null));
        }
    }
}
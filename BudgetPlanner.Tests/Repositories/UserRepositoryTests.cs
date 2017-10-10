using System;
using BudgetPlanner.Repositories;
using NUnit.Framework;

namespace BudgetPlanner.Tests.Repositories
{
    [TestFixture]
    public class UserRepositoryTests
    {
        private UserRepository userRepository;

        [Test]
        public void WhenDbConnectionFactoryIsNullThenThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => this.userRepository = new UserRepository(null));
        }
    }
}
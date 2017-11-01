using BudgetPlanner.Repositories;
using Moq;
using NUnit.Framework;

namespace BudgetPlanner.IntegrationTests
{
    [TestFixture]
    public class CategoryGroupRepositoryTests
    {
        [SetUp]
        public void SetUp()
        {
            this.dbConnectionFactory = new Mock<IDbConnectionFactory>();
            this.categoryGroupRepository = new CategoryGroupRepository(this.dbConnectionFactory.Object);

            this.dbConnectionFactory.Setup(d => d.Create()).Returns(this.databaseHelper.OpenConnection());
        }

        private Mock<IDbConnectionFactory> dbConnectionFactory;
        private CategoryGroupRepository categoryGroupRepository;
        private readonly DatabaseHelper databaseHelper = new DatabaseHelper();
    }
}
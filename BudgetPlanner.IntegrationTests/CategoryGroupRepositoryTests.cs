using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetPlanner.IntegrationTests.DatabaseModels;
using BudgetPlanner.Repositories;
using Moq;
using NUnit.Framework;

namespace BudgetPlanner.IntegrationTests
{
    [TestFixture]
    public class CategoryGroupRepositoryTests
    {
        private Mock<IDbConnectionFactory> dbConnectionFactory;
        private CategoryGroupRepository categoryGroupRepository;
        private readonly DatabaseHelper databaseHelper = new DatabaseHelper();

        [SetUp]
        public void SetUp()
        {
            this.dbConnectionFactory = new Mock<IDbConnectionFactory>();
            this.categoryGroupRepository = new CategoryGroupRepository(this.dbConnectionFactory.Object);

            this.dbConnectionFactory.Setup(d => d.Create()).Returns(this.databaseHelper.OpenConnection());

            this.databaseHelper.DropAndCreateTable<CategoryGroup>();
            this.databaseHelper.DropAndCreateTable<Category>();
        }

        [Test]
        public async Task WhenAddAsyncIsCalledThenCategoryGroupIsInserted()
        {
            var categoryGroup = new Models.CategoryGroup
            {
                Id = 1,
                Name = "Bills",
                UserId = new Guid("00000000-0000-0000-0000-000000000001")
            };

            await this.categoryGroupRepository.AddAsync(categoryGroup);

            List<CategoryGroup> insertedCategoryGroups = this.databaseHelper.Select<CategoryGroup>(c => c.Name == "Bills");

            Assert.That(insertedCategoryGroups.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenAddAsyncIsCalledThenAllFieldsArePopulated()
        {
            var categoryGroup = new Models.CategoryGroup
            {
                Id = 1,
                Name = "Bills",
                UserId = new Guid("00000000-0000-0000-0000-000000000001")
            };

            await this.categoryGroupRepository.AddAsync(categoryGroup);

            List<CategoryGroup> insertedCategoryGroups = this.databaseHelper.Select<CategoryGroup>(c => c.Name == "Bills");

            Assume.That(insertedCategoryGroups.Count, Is.EqualTo(1));

            CategoryGroup insertedCategoryGroup = insertedCategoryGroups.First();

            Assert.That(insertedCategoryGroup.Name, Is.EqualTo("Bills"));
        }

        [Test]
        public async Task WhenUpdateAsyncIsCalledThenCategoryGroupIsUpdated()
        {
            List<CategoryGroup> categoriesToInsert = new List<CategoryGroup>
            {
                new CategoryGroup
                {
                    Id = 1,
                    Name = "Bills",
                    UserId = "00000000-0000-0000-0000-000000000001"
                }
            };

            this.databaseHelper.Insert(categoriesToInsert);

            var categoryGroup = new Models.CategoryGroup
            {
                Id = 1,
                Name = "Just For Fun",
                UserId = new Guid("00000000-0000-0000-0000-000000000001")
            };

            await this.categoryGroupRepository.UpdateAsync(categoryGroup);

            List<CategoryGroup> updatedCategoryGroups = this.databaseHelper.Select<CategoryGroup>(c => c.Name == "Just For Fun");

            Assert.That(updatedCategoryGroups.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenDeleteAsyncIsCalledThenCategoryGroupIsDeleted()
        {
            List<CategoryGroup> categoriesToInsert = new List<CategoryGroup>
            {
                new CategoryGroup
                {
                    Id = 1,
                    Name = "Bills",
                    UserId = "00000000-0000-0000-0000-000000000001"
                }
            };

            this.databaseHelper.Insert(categoriesToInsert);

            await this.categoryGroupRepository.DeleteAsync(1);

            List<CategoryGroup> updatedCategoryGroups = this.databaseHelper.Select<CategoryGroup>(c => c.Name == "Bills");

            Assert.That(updatedCategoryGroups, Is.Empty);
        }
    }
}
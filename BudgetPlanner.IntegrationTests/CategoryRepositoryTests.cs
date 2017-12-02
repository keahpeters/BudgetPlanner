using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetPlanner.IntegrationTests.DatabaseModels;
using BudgetPlanner.Repositories;
using Moq;
using NUnit.Framework;

namespace BudgetPlanner.IntegrationTests
{
    [TestFixture]
    public class CategoryRepositoryTests
    {
        private Mock<IDbConnectionFactory> dbConnectionFactory;
        private CategoryRepository categoryRepository;
        private readonly DatabaseHelper databaseHelper = new DatabaseHelper();

        [SetUp]
        public void SetUp()
        {
            this.dbConnectionFactory = new Mock<IDbConnectionFactory>();
            this.categoryRepository = new CategoryRepository(this.dbConnectionFactory.Object);

            this.dbConnectionFactory.Setup(d => d.Create()).Returns(this.databaseHelper.OpenConnection());

            this.databaseHelper.DropAndCreateTable<CategoryGroup>();
            this.databaseHelper.DropAndCreateTable<Category>();
        }

        [Test]
        public async Task WhenGetIsCalledThenCorrectCategoryIsReturned()
        {
            var categoriesToInsert = new List<Category>
            {
                new Category
                {
                    Id = 1,
                    Name = "Bills",
                    CategoryGroupId = 1,
                    Budget = 50.00M
                },
                new Category
                {
                    Id = 2,
                    Name = "Rent",
                    CategoryGroupId = 1,
                    Budget = 650.00M
                },
                new Category
                {
                    Id = 3,
                    Name = "Groceries",
                    CategoryGroupId = 1,
                    Budget = 150.00M
                }
            };

            this.databaseHelper.Insert(categoriesToInsert);

            Models.Category result = await this.categoryRepository.GetAsync(2);

            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.EqualTo(2));
                Assert.That(result.Name, Is.EqualTo("Rent"));
                Assert.That(result.CategoryGroupId, Is.EqualTo(1));
                Assert.That(result.Budget, Is.EqualTo(650.00M));
            });
        }

        [Test]
        public async Task WhenAddAsyncIsCalledThenCategoryIsInserted()
        {
            var category = new Models.Category
            {
                Name = "Bills",
                CategoryGroupId = 1
            };

            await this.categoryRepository.AddAsync(category);

            List<Category> insertedCategories = this.databaseHelper.Select<Category>(c => c.Name == "Bills");

            Assert.That(insertedCategories.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenAddAsyncIsCalledThenAllFieldsArePopulated()
        {
            var category = new Models.Category
            {
                Name = "Bills",
                CategoryGroupId = 1
            };

            await this.categoryRepository.AddAsync(category);

            List<Category> insertedCategories = this.databaseHelper.Select<Category>(c => c.Name == "Bills");

            Assume.That(insertedCategories.Count, Is.EqualTo(1));

            Category insertedCategory = insertedCategories.First();

            Assert.That(insertedCategory.Name, Is.EqualTo("Bills"));
            Assert.That(insertedCategory.CategoryGroupId, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenUpdateAsyncIsCalledThenCategoryIsUpdated()
        {
            var categoriesToInsert = new List<Category>
            {
                new Category
                {
                    Id = 1,
                    Name = "Bills",
                    CategoryGroupId = 1
                }
            };

            this.databaseHelper.Insert(categoriesToInsert);

            var category = new Models.Category
            {
                Id = 1,
                Name = "Rent",
                CategoryGroupId = 1
            };

            await this.categoryRepository.UpdateAsync(category);

            List<Category> updatedCategories = this.databaseHelper.Select<Category>(c => c.Name == "Rent");

            Assert.That(updatedCategories.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenDeleteAsyncIsCalledThenCategoryIsDeleted()
        {
            var categoriesToInsert = new List<Category>
            {
                new Category
                {
                    Id = 1,
                    Name = "Bills",
                    CategoryGroupId = 1
                }
            };

            this.databaseHelper.Insert(categoriesToInsert);

            await this.categoryRepository.DeleteAsync(1);

            List<Category> deletedCategories = this.databaseHelper.Select<Category>(c => c.Name == "Bills");

            Assert.That(deletedCategories, Is.Empty);
        }
    }
}

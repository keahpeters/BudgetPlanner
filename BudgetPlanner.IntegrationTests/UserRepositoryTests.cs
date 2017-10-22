using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetPlanner.Repositories;
using Moq;
using NUnit.Framework;

namespace BudgetPlanner.IntegrationTests
{
    [TestFixture]
    public class UserRepositoryTests
    {
        [SetUp]
        public void SetUp()
        {
            this.dbConnectionFactory = new Mock<IDbConnectionFactory>();
            this.userRepository = new UserRepository(this.dbConnectionFactory.Object);

            this.dbConnectionFactory.Setup(d => d.Create()).Returns(this.databaseHelper.OpenConnection());

            this.databaseHelper.DropAndCreateTable<ApplicationUser>();
        }

        private Mock<IDbConnectionFactory> dbConnectionFactory;
        private UserRepository userRepository;
        private readonly DatabaseHelper databaseHelper = new DatabaseHelper();

        [Test]
        public async Task WhenCreateAsyncIsCalledThenAllFieldsArePopulated()
        {
            var user = new Models.ApplicationUser
            {
                UserName = "Keah",
                PasswordHash = "Test",
                Email = "test@test.com"
            };

            await this.userRepository.CreateAsync(user);

            List<ApplicationUser> insertedUsers = this.databaseHelper.Select<ApplicationUser>(u => u.UserName == "Keah");

            Assume.That(insertedUsers.Count, Is.EqualTo(1));

            ApplicationUser insertedUser = insertedUsers.First();

            Assert.Multiple(() =>
            {
                Assert.That(insertedUser.UserName, Is.EqualTo("Keah"));
                Assert.That(insertedUser.PasswordHash, Is.EqualTo("Test"));
                Assert.That(insertedUser.Email, Is.EqualTo("test@test.com"));
            });
        }

        [Test]
        public async Task WhenCreateAsyncIsCalledThenUserIsInserted()
        {
            var user = new Models.ApplicationUser
            {
                UserName = "Keah",
                PasswordHash = "Test",
                Email = "test@test.com"
            };

            await this.userRepository.CreateAsync(user);

            List<ApplicationUser> insertedUsers = this.databaseHelper.Select<ApplicationUser>(u => u.UserName == "Keah");

            Assert.That(insertedUsers.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenFindByNameIsCalledAndUserDoesNotExistsThenNullIsReturned()
        {
            Models.ApplicationUser user = await this.userRepository.FindByName("Keah");

            Assert.That(user, Is.Null);
        }

        [Test]
        public async Task WhenFindByNameIsCalledAndUserExistsThenUserIsReturned()
        {
            var usersToInsert = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    UserName = "Keah",
                    PasswordHash = "Test",
                    Email = "test@test.com"
                }
            };

            this.databaseHelper.Insert(usersToInsert);

            Models.ApplicationUser user = await this.userRepository.FindByName("Keah");

            Assert.Multiple(() =>
            {
                Assert.That(user.UserName, Is.EqualTo("Keah"));
                Assert.That(user.PasswordHash, Is.EqualTo("Test"));
                Assert.That(user.Email, Is.EqualTo("test@test.com"));
            });
        }

        [Test]
        public async Task WhenGetBalanceIsCalledThenBalanceIsReturned()
        {
            var usersToInsert = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    UserName = "Keah",
                    PasswordHash = "Test",
                    Email = "test@test.com",
                    Balance = 23.15M
                }
            };

            this.databaseHelper.Insert(usersToInsert);

            decimal balance = await this.userRepository.GetBalance("Keah");

            Assert.That(balance, Is.EqualTo(23.15M));
        }
    }
}
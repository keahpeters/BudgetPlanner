using System;
using System.Threading;
using System.Threading.Tasks;
using BudgetPlanner.Identity;
using BudgetPlanner.Infrastructure;
using BudgetPlanner.Models;
using BudgetPlanner.Repositories;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;

namespace BudgetPlanner.Tests.Identity
{
    [TestFixture]
    public class UserStoreTests
    {
        private Mock<IUserRepository> userRepository;
        private UserStore userStore;

        [SetUp]
        public void SetUp()
        {
            this.userRepository = new Mock<IUserRepository>();
            this.userStore = new UserStore(this.userRepository.Object);
        }

        [Test]
        public async Task WhenCreateAsyncIsCalledAndRepositoryThrowsExceptionThenReturnFailedIdentityResult()
        {
            this.userRepository.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>())).Throws(new RepositoryException("Test"));

            IdentityResult result = await this.userStore.CreateAsync(new ApplicationUser(), new CancellationToken());

            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public async Task WhenCreateAsyncIsCalledAndUserIsAddedThenReturnFailedIdentityResult()
        {
            IdentityResult result = await this.userStore.CreateAsync(new ApplicationUser(), new CancellationToken());

            Assert.That(result.Succeeded, Is.True);
        }

        [Test]
        public void WhenCreateAsyncIsCalledAndUserIsNullThenThrowArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await this.userStore.CreateAsync(null, new CancellationToken()));
        }

        [Test]
        public void WhenDeleteAsyncIsCalledThenThrowNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() => this.userStore.DeleteAsync(new ApplicationUser(), new CancellationToken()));
        }

        [Test]
        public void WhenFindByIdAsyncIsCalledThenThrowNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() => this.userStore.FindByIdAsync(Guid.NewGuid().ToString(), new CancellationToken()));
        }

        [Test]
        public async Task WhenFindByNameAsyncIsCalledThenUserIsReturned()
        {
            this.userRepository.Setup(u => u.FindByName(It.IsAny<string>())).ReturnsAsync(new ApplicationUser { UserName = "Test" });

            ApplicationUser result = await this.userStore.FindByNameAsync("Test", new CancellationToken());

            Assert.That(result.UserName, Is.EqualTo("Test"));
        }

        [Test]
        public void WhenGetNormalizedUserNameAsyncIsCalledThenThrowNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() => this.userStore.GetNormalizedUserNameAsync(new ApplicationUser(), new CancellationToken()));
        }

        [Test]
        public async Task WhenGetUserIdAsyncIsCalledThenReturnUserId()
        {
            Guid userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId };

            string result = await this.userStore.GetUserIdAsync(user, new CancellationToken());

            Assert.That(result, Is.EqualTo(userId.ToString()));
        }

        [Test]
        public async Task WhenGetUserNameAsyncIsCalledThenReturnUserName()
        {
            var user = new ApplicationUser { UserName = "Test" };

            string result = await this.userStore.GetUserNameAsync(user, new CancellationToken());

            Assert.That(result, Is.EqualTo("Test"));
        }

        [Test]
        public async Task WhenSetNormalizedUserNameAsyncIsCalledThenNothingHappens()
        {
            var user = new ApplicationUser { UserName = "Test" };

            await this.userStore.SetNormalizedUserNameAsync(user, "TEST", new CancellationToken());

            Assert.That(user.UserName, Is.EqualTo("Test"));
        }

        [Test]
        public void WhenSetUserNameAsyncIsCalledThenThrowNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() => this.userStore.SetUserNameAsync(new ApplicationUser(), "Test", new CancellationToken()));
        }

        [Test]
        public void WhenUpdateAsyncIsCalledThenThrowNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() => this.userStore.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task WhenSetPasswordHashAsyncIsCalledThenSetPasswordHashOnUser()
        {
            var user = new ApplicationUser();

            await this.userStore.SetPasswordHashAsync(user, "Test", new CancellationToken());

            Assert.That(user.PasswordHash, Is.EqualTo("Test"));
        }

        [Test]
        public async Task WhenGetPasswordHashAsyncIsCalledThenReturnPasswordHash()
        {
            var user = new ApplicationUser { PasswordHash = "Test" };

            string result = await this.userStore.GetPasswordHashAsync(user, new CancellationToken());

            Assert.That(result, Is.EqualTo("Test"));
        }

        [Test]
        public async Task WhenHasPasswordAsyncIsCalledAndPasswordHashIsSetThenReturnTrue()
        {
            var user = new ApplicationUser { PasswordHash = "Test" };

            bool result = await this.userStore.HasPasswordAsync(user, new CancellationToken());

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task WhenHasPasswordAsyncIsCalledAndPasswordHashIsNotSetThenReturnFalse()
        {
            var user = new ApplicationUser();

            bool result = await this.userStore.HasPasswordAsync(user, new CancellationToken());

            Assert.That(result, Is.False);
        }
    }
}
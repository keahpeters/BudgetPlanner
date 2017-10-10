using System;
using System.Threading;
using BudgetPlanner.Identity;
using BudgetPlanner.Models;
using Moq;
using NUnit.Framework;

namespace BudgetPlanner.Tests.Identity
{
    [TestFixture]
    public class RoleStoreTests
    {
        private RoleStore roleStore;

        [SetUp]
        public void SetUp()
        {
            this.roleStore = new RoleStore();
        }

        [Test]
        public void WhenCreateAsyncIsCalledThenThrowNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() => this.roleStore.CreateAsync(It.IsAny<ApplicationRole>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public void WhenUpdateAsyncIsCalledThenThrowNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() => this.roleStore.UpdateAsync(It.IsAny<ApplicationRole>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public void WhenDeleteAsyncIsCalledThenThrowNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() => this.roleStore.DeleteAsync(It.IsAny<ApplicationRole>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public void WhenGetRoleIdAsyncIsCalledThenThrowNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() => this.roleStore.GetRoleIdAsync(It.IsAny<ApplicationRole>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public void WhenGetRoleNameAsyncIsCalledThenThrowNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() => this.roleStore.GetRoleNameAsync(It.IsAny<ApplicationRole>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public void WhenSetRoleNameAsyncIsCalledThenThrowNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() => this.roleStore.SetRoleNameAsync(It.IsAny<ApplicationRole>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public void WhenGetNormalizedRoleNameAsyncIsCalledThenThrowNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() => this.roleStore.GetNormalizedRoleNameAsync(It.IsAny<ApplicationRole>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public void WhenSetNormalizedRoleNameAsyncIsCalledThenThrowNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() => this.roleStore.SetNormalizedRoleNameAsync(It.IsAny<ApplicationRole>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public void WhenFindByIdAsyncIsCalledThenThrowNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() => this.roleStore.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public void WhenFindByNameAsyncIsCalledThenThrowNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() => this.roleStore.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));
        }
    }
}
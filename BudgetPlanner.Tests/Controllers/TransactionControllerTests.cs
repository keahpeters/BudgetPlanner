using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using BudgetPlanner.Controllers;
using BudgetPlanner.Models;
using BudgetPlanner.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace BudgetPlanner.Tests.Controllers
{
    [TestFixture]
    public class TransactionControllerTests
    {
        [SetUp]
        public void SetUp()
        {
            this.userRepository = new Mock<IUserRepository>();
            this.transactionRepository = new Mock<ITransactionRepository>();
            this.transactionController = new TransactionController(this.userRepository.Object, this.transactionRepository.Object);
            this.SetUpContext(this.transactionController);

            this.userRepository.Setup(u => u.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser { UserName = "TestUser", Id = Guid.NewGuid() });
        }

        private Mock<IUserRepository> userRepository;
        private Mock<ITransactionRepository> transactionRepository;
        private TransactionController transactionController;

        private void SetUpContext(TransactionController controller)
        {
            var httpContext = new DefaultHttpContext();
            var identity = new GenericIdentity("User");
            var principal = new GenericPrincipal(identity, null);

            httpContext.User = principal;

            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        [Test]
        public async Task WhenIndexGetMethodIsCalledThenTransactionsAreSetOnModel()
        {
            this.transactionRepository.Setup(t => t.Get(It.IsAny<Guid>())).ReturnsAsync(new List<Transaction>
            {
                new Transaction(),
                new Transaction(),
                new Transaction()
            });

            IActionResult result = await this.transactionController.Index();

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            var model = (IEnumerable<Transaction>) ((ViewResult) result).Model;

            Assert.That(model.Count(), Is.EqualTo(3));
        }

        [Test]
        public async Task WhenIndexGetMethodIsCalledThenUseDefaultView()
        {
            IActionResult result = await this.transactionController.Index();

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult) result).ViewName, Is.Null);
        }

        [Test]
        public void WhenTransactionRepositoryIsNullThenThrowNullArguementException()
        {
            Assert.Throws<ArgumentNullException>(() => this.transactionController = new TransactionController(this.userRepository.Object, null));
        }

        [Test]
        public void WhenUserRepositoryIsNullThenThrowNullArguementException()
        {
            Assert.Throws<ArgumentNullException>(() => this.transactionController = new TransactionController(this.userRepository.Object, null));
        }
    }
}
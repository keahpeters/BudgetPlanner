using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using BudgetPlanner.Controllers;
using BudgetPlanner.Infrastructure;
using BudgetPlanner.Models;
using BudgetPlanner.Repositories;
using BudgetPlanner.ViewModels;
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
            this.categoryRepository = new Mock<ICategoryRepository>();
            this.transactionController = new TransactionController(this.userRepository.Object, this.transactionRepository.Object, this.categoryRepository.Object);
            this.SetUpContext(this.transactionController);

            this.userRepository.Setup(u => u.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser { UserName = "TestUser", Id = Guid.NewGuid() });
        }

        private Mock<IUserRepository> userRepository;
        private Mock<ITransactionRepository> transactionRepository;
        private Mock<ICategoryRepository> categoryRepository;
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
            Assert.Throws<ArgumentNullException>(() => this.transactionController = new TransactionController(this.userRepository.Object, null, this.categoryRepository.Object));
        }

        [Test]
        public void WhenUserRepositoryIsNullThenThrowNullArguementException()
        {
            Assert.Throws<ArgumentNullException>(() => this.transactionController = new TransactionController(null, this.transactionRepository.Object, this.categoryRepository.Object));
        }

        [Test]
        public void WhenCategoryRepositoryIsNullThenThrowNullArguementException()
        {
            Assert.Throws<ArgumentNullException>(() => this.transactionController = new TransactionController(this.userRepository.Object, this.transactionRepository.Object, null));
        }

        [Test]
        public async Task WhenAddTransactionGetMethodIsCalledThenViewResultIsReturned()
        {
            IActionResult result = await this.transactionController.AddTransaction();

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenAddTransactionGetMethodIsCalledThenUseDefaultView()
        {
            IActionResult result = await this.transactionController.AddTransaction();

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenAddTransactionPostMethodIsCalledAndModelStateIsInvalidThenUseDefaultView()
        {
            this.transactionController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.transactionController.AddTransaction(new TransactionViewModel());

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenAddTransactionPostMethodIsCalledAndModelStateIsInvalidThenViewResultIsReturned()
        {
            this.transactionController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.transactionController.AddTransaction(new TransactionViewModel());

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenAddTransactionPostMethodIsCalledAndAddFailsThenAddModelError()
        {
            this.transactionRepository.Setup(t => t.Add(It.IsAny<Transaction>())).ThrowsAsync(new RepositoryException("Test"));

            await this.transactionController.AddTransaction(new TransactionViewModel());

            Assert.That(this.transactionController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenAddTransactionPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToActionResultIsReturned()
        {
            IActionResult result = await this.transactionController.AddTransaction(new TransactionViewModel { Amount = 1.00M });

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenAddTransactionPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToCorrectAction()
        {
            IActionResult result = await this.transactionController.AddTransaction(new TransactionViewModel { Amount = 1.00M });

            Assume.That(result, Is.TypeOf(typeof(RedirectToActionResult)));

            Assert.That(((RedirectToActionResult)result).ControllerName, Is.Null);
            Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task WhenEditTransactionGetMethodIsCalledAndModelExistsThenViewResultIsReturned()
        {
            this.transactionRepository.Setup(c => c.Get(It.IsAny<int>())).ReturnsAsync(new Transaction());

            IActionResult result = await this.transactionController.EditTransaction(1);

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenEditTransactionGetMethodIsCalledAndModelExistsThenUseDefaultView()
        {
            this.transactionRepository.Setup(c => c.Get(It.IsAny<int>())).ReturnsAsync(new Transaction());

            IActionResult result = await this.transactionController.EditTransaction(1);

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenEditTransactionGetMethodIsCalledAndModelDoesNotExistsThenRedirectResultIsReturned()
        {
            IActionResult result = await this.transactionController.EditTransaction(1);

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenEditTransactionPostMethodIsCalledAndModelStateIsInvalidThenUseDefaultView()
        {
            this.transactionController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.transactionController.EditTransaction(new TransactionViewModel());

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenEditTransactionPostMethodIsCalledAndModelStateIsInvalidThenViewResultIsReturned()
        {
            this.transactionController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.transactionController.EditTransaction(new TransactionViewModel());

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenEditTransactionPostMethodIsCalledAndAddFailsThenAddModelError()
        {
            this.categoryRepository.Setup(c => c.Update(It.IsAny<Category>())).ThrowsAsync(new RepositoryException("Test"));

            await this.transactionController.EditTransaction(new TransactionViewModel());

            Assert.That(this.transactionController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenEditTransactionPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToActionResultIsReturned()
        {
            this.transactionRepository.Setup(t => t.Get(It.IsAny<int>())).ReturnsAsync(new Transaction { Amount = 1.00M });

            IActionResult result = await this.transactionController.EditTransaction(new TransactionViewModel { Amount = 1.00M });

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenEditTransactionPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToCorrectAction()
        {
            this.transactionRepository.Setup(t => t.Get(It.IsAny<int>())).ReturnsAsync(new Transaction { Amount = 1.00M });

            IActionResult result = await this.transactionController.EditTransaction(new TransactionViewModel { Amount = 1.00M });

            Assume.That(result, Is.TypeOf(typeof(RedirectToActionResult)));

            Assert.That(((RedirectToActionResult)result).ControllerName, Is.Null);
            Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task WhenEditTransactionPostMethodIsCalledAndAmountHasChangedThenUpdateAndReapplyTransaction()
        {
            this.transactionRepository.Setup(t => t.Get(It.IsAny<int>())).ReturnsAsync(new Transaction { Amount = 1.00M, CategoryId = 1, IsInTransaction = false });

            await this.transactionController.EditTransaction(new TransactionViewModel { Amount = 2.00M, CategoryId = 1, IsInTransaction = false });

            this.transactionRepository.Verify(t => t.UpdateAndReapplyTransaction(It.IsAny<Transaction>(), It.IsAny<Transaction>()), Times.Once);
        }

        [Test]
        public async Task WhenEditTransactionPostMethodIsCalledAndCategoryHasChangedThenUpdateAndReapplyTransaction()
        {
            this.transactionRepository.Setup(t => t.Get(It.IsAny<int>())).ReturnsAsync(new Transaction { Amount = 1.00M, CategoryId = 1, IsInTransaction = false });

            await this.transactionController.EditTransaction(new TransactionViewModel { Amount = 1.00M, CategoryId = 2, IsInTransaction = false });

            this.transactionRepository.Verify(t => t.UpdateAndReapplyTransaction(It.IsAny<Transaction>(), It.IsAny<Transaction>()), Times.Once);
        }

        [Test]
        public async Task WhenEditTransactionPostMethodIsCalledAndIsInTransactionHasChangedThenUpdateAndReapplyTransaction()
        {
            this.transactionRepository.Setup(t => t.Get(It.IsAny<int>())).ReturnsAsync(new Transaction { Amount = 1.00M, CategoryId = 1, IsInTransaction = false });

            await this.transactionController.EditTransaction(new TransactionViewModel { Amount = 1.00M, CategoryId = 1, IsInTransaction = true });

            this.transactionRepository.Verify(t => t.UpdateAndReapplyTransaction(It.IsAny<Transaction>(), It.IsAny<Transaction>()), Times.Once);
        }

        [Test]
        public async Task WhenEditTransactionPostMethodIsCalledAndAmountCategoryAndIsInTransactionHaveNotChangedThenUpdate()
        {
            this.transactionRepository.Setup(t => t.Get(It.IsAny<int>())).ReturnsAsync(new Transaction { Amount = 1.00M, CategoryId = 1, IsInTransaction = false });

            await this.transactionController.EditTransaction(new TransactionViewModel { Amount = 1.00M, CategoryId = 1, IsInTransaction = false });

            this.transactionRepository.Verify(t => t.Update(It.IsAny<Transaction>()), Times.Once);
        }
    }
}
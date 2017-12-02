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
    public class HomeControllerTests
    {
        [SetUp]
        public void SetUp()
        {
            this.userRepository = new Mock<IUserRepository>();
            this.categoryGroupRepository = new Mock<ICategoryGroupRepository>();
            this.categoryRepository = new Mock<ICategoryRepository>();
            this.homeController = new HomeController(this.userRepository.Object, this.categoryGroupRepository.Object, this.categoryRepository.Object);
            this.SetUpContext(this.homeController);

            this.userRepository.Setup(u => u.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser { UserName = "TestUser", Id = Guid.NewGuid() });
        }

        private Mock<IUserRepository> userRepository;
        private Mock<ICategoryGroupRepository> categoryGroupRepository;
        private Mock<ICategoryRepository> categoryRepository;
        private HomeController homeController;

        private void SetUpContext(HomeController controller)
        {
            var httpContext = new DefaultHttpContext();
            var identity = new GenericIdentity("User");
            var principal = new GenericPrincipal(identity, null);

            httpContext.User = principal;

            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        [Test]
        public void WhenCategoryGroupRepositoryIsNullThenThrowNullArguementException()
        {
            Assert.Throws<ArgumentNullException>(() => this.homeController = new HomeController(this.userRepository.Object, null, this.categoryRepository.Object));
        }

        [Test]
        public void WhenCategoryRepositoryIsNullThenThrowNullArguementException()
        {
            Assert.Throws<ArgumentNullException>(() => this.homeController = new HomeController(this.userRepository.Object, this.categoryGroupRepository.Object, null));
        }

        [Test]
        public async Task WhenIndexGetMethodIsCalledThenUseDefaultView()
        {
            IActionResult result = await this.homeController.Index();

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenIndexGetMethodIsCalledThenApplicationUserIsSetOnModel()
        {
            this.userRepository.Setup(u => u.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser
            {
                UserName = "TestUser"
            });

            IActionResult result = await this.homeController.Index();

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            var model = (BudgetViewModel)((ViewResult) result).Model;

            Assert.That(model.ApplicationUser.UserName, Is.EqualTo("TestUser"));
        }

        [Test]
        public async Task WhenIndexGetMethodIsCalledThenCategoryGroupsIsSetOnModel()
        {
            this.categoryGroupRepository.Setup(c => c.GetByUserNameAsync(It.IsAny<string>())).ReturnsAsync(new List<CategoryGroup>
            {
                new CategoryGroup(),
                new CategoryGroup(),
                new CategoryGroup()
            });

            IActionResult result = await this.homeController.Index();

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            var model = (BudgetViewModel)((ViewResult)result).Model;

            Assert.That(model.CategoryGroups.Count(), Is.EqualTo(3));
        }

        [Test]
        public void WhenUserRepositoryIsNullThenThrowNullArguementException()
        {
            Assert.Throws<ArgumentNullException>(() => this.homeController = new HomeController(null, this.categoryGroupRepository.Object, this.categoryRepository.Object));
        }

        [Test]
        public void WhenAddCategoryGroupGetMethodIsCalledThenViewResultIsReturned()
        {
            IActionResult result = this.homeController.AddCategoryGroup();

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public void WhenAddCategoryGroupGetMethodIsCalledThenUseDefaultView()
        {
            IActionResult result = this.homeController.AddCategoryGroup();

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenAddCategoryGroupPostMethodIsCalledAndModelStateIsInvalidThenUseDefaultView()
        {
            this.homeController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.homeController.AddCategoryGroup(new CategoryGroup());

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenAddCategoryGroupPostMethodIsCalledAndModelStateIsInvalidThenViewResultIsReturned()
        {
            this.homeController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.homeController.AddCategoryGroup(new CategoryGroup());

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenAddCategoryGroupPostMethodIsCalledAndCategoryGroupAlreadyExistsThenAddModelError()
        {
            this.categoryGroupRepository.Setup(c => c.AddAsync(It.IsAny<CategoryGroup>())).ThrowsAsync(new EntityAlreadyExistsException("Test"));

            await this.homeController.AddCategoryGroup(new CategoryGroup());

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenAddCategoryGroupPostMethodIsCalledAndAddFailsThenAddModelError()
        {
            this.categoryGroupRepository.Setup(c => c.AddAsync(It.IsAny<CategoryGroup>())).ThrowsAsync(new RepositoryException("Test"));

            await this.homeController.AddCategoryGroup(new CategoryGroup());

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenAddCategoryGroupPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToActionResultIsReturned()
        {
            IActionResult result = await this.homeController.AddCategoryGroup(new CategoryGroup());

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenAddCategoryGroupPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToCorrectAction()
        {
            IActionResult result = await this.homeController.AddCategoryGroup(new CategoryGroup());

            Assume.That(result, Is.TypeOf(typeof(RedirectToActionResult)));

            Assert.That(((RedirectToActionResult)result).ControllerName, Is.Null);
            Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task WhenEditCategoryGroupGetMethodIsCalledAndModelExistsThenViewResultIsReturned()
        {
            this.categoryGroupRepository.Setup(c => c.GetAsync(It.IsAny<int>())).ReturnsAsync(new CategoryGroup());

            IActionResult result = await this.homeController.EditCategoryGroup(1);

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenEditCategoryGroupGetMethodIsCalledAndModelExistsThenUseDefaultView()
        {
            this.categoryGroupRepository.Setup(c => c.GetAsync(It.IsAny<int>())).ReturnsAsync(new CategoryGroup());

            IActionResult result = await this.homeController.EditCategoryGroup(1);

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenEditCategoryGroupGetMethodIsCalledAndModelDoesNotExistsThenRedirectResultIsReturned()
        {
            IActionResult result = await this.homeController.EditCategoryGroup(1);

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenEditCategoryGroupPostMethodIsCalledAndModelStateIsInvalidThenUseDefaultView()
        {
            this.homeController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.homeController.EditCategoryGroup(new CategoryGroup());

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenEditCategoryGroupPostMethodIsCalledAndModelStateIsInvalidThenViewResultIsReturned()
        {
            this.homeController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.homeController.EditCategoryGroup(new CategoryGroup());

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenEditCategoryGroupPostMethodIsCalledAndCategoryGroupAlreadyExistsThenAddModelError()
        {
            this.categoryGroupRepository.Setup(c => c.UpdateAsync(It.IsAny<CategoryGroup>())).ThrowsAsync(new EntityAlreadyExistsException("Test"));

            await this.homeController.EditCategoryGroup(new CategoryGroup());

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenEditCategoryGroupPostMethodIsCalledAndAddFailsThenAddModelError()
        {
            this.categoryGroupRepository.Setup(c => c.UpdateAsync(It.IsAny<CategoryGroup>())).ThrowsAsync(new RepositoryException("Test"));

            await this.homeController.EditCategoryGroup(new CategoryGroup());

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenEditCategoryGroupPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToActionResultIsReturned()
        {
            IActionResult result = await this.homeController.AddCategoryGroup(new CategoryGroup());

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenEditCategoryGroupPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToCorrectAction()
        {
            IActionResult result = await this.homeController.AddCategoryGroup(new CategoryGroup());

            Assume.That(result, Is.TypeOf(typeof(RedirectToActionResult)));

            Assert.That(((RedirectToActionResult)result).ControllerName, Is.Null);
            Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task WhenDeleteCategoryGroupGetMethodIsCalledAndModelExistsThenViewResultIsReturned()
        {
            this.categoryGroupRepository.Setup(c => c.GetAsync(It.IsAny<int>())).ReturnsAsync(new CategoryGroup());

            IActionResult result = await this.homeController.DeleteCategoryGroup(1);

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenDeleteCategoryGroupGetMethodIsCalledAndModelExistsThenUseDefaultView()
        {
            this.categoryGroupRepository.Setup(c => c.GetAsync(It.IsAny<int>())).ReturnsAsync(new CategoryGroup());

            IActionResult result = await this.homeController.DeleteCategoryGroup(1);

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenDeleteCategoryGroupGetMethodIsCalledAndModelDoesNotExistsThenRedirectResultIsReturned()
        {
            IActionResult result = await this.homeController.DeleteCategoryGroup(1);

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenDeleteCategoryGroupPostMethodIsCalledAndModelStateIsInvalidThenUseDefaultView()
        {
            this.homeController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.homeController.DeleteCategoryGroup(new CategoryGroup());

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenDeleteCategoryGroupPostMethodIsCalledAndModelStateIsInvalidThenViewResultIsReturned()
        {
            this.homeController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.homeController.DeleteCategoryGroup(new CategoryGroup());

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenDeleteCategoryGroupPostMethodIsCalledAndCategoryGroupHasCategoriesAssignedThenAddModelError()
        {
            this.categoryGroupRepository.Setup(c => c.DeleteAsync(It.IsAny<int>())).ThrowsAsync(new ChildEntitiesExistException("Test"));

            await this.homeController.DeleteCategoryGroup(new CategoryGroup());

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenDeleteCategoryGroupPostMethodIsCalledAndAddFailsThenAddModelError()
        {
            this.categoryGroupRepository.Setup(c => c.DeleteAsync(It.IsAny<int>())).ThrowsAsync(new RepositoryException("Test"));

            await this.homeController.DeleteCategoryGroup(new CategoryGroup());

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenDeleteCategoryGroupPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToActionResultIsReturned()
        {
            IActionResult result = await this.homeController.DeleteCategoryGroup(new CategoryGroup());

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenDeleteCategoryGroupPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToCorrectAction()
        {
            IActionResult result = await this.homeController.DeleteCategoryGroup(new CategoryGroup());

            Assume.That(result, Is.TypeOf(typeof(RedirectToActionResult)));

            Assert.That(((RedirectToActionResult)result).ControllerName, Is.Null);
            Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public void WhenAddCategoryGetMethodIsCalledThenViewResultIsReturned()
        {
            IActionResult result = this.homeController.AddCategory(1);

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public void WhenAddCategoryGetMethodIsCalledThenUseDefaultView()
        {
            IActionResult result = this.homeController.AddCategory(1);

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenAddCategoryPostMethodIsCalledAndModelStateIsInvalidThenUseDefaultView()
        {
            this.homeController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.homeController.AddCategory(new Category());

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenAddCategoryPostMethodIsCalledAndModelStateIsInvalidThenViewResultIsReturned()
        {
            this.homeController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.homeController.AddCategory(new Category());

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenAddCategoryPostMethodIsCalledAndCategoryGroupAlreadyExistsThenAddModelError()
        {
            this.categoryRepository.Setup(c => c.AddAsync(It.IsAny<Category>())).ThrowsAsync(new EntityAlreadyExistsException("Test"));

            await this.homeController.AddCategory(new Category());

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenAddCategoryPostMethodIsCalledAndAddFailsThenAddModelError()
        {
            this.categoryRepository.Setup(c => c.AddAsync(It.IsAny<Category>())).ThrowsAsync(new RepositoryException("Test"));

            await this.homeController.AddCategory(new Category());

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenAddCategoryPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToActionResultIsReturned()
        {
            IActionResult result = await this.homeController.AddCategory(new Category());

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenAddCategoryPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToCorrectAction()
        {
            IActionResult result = await this.homeController.AddCategory(new Category());

            Assume.That(result, Is.TypeOf(typeof(RedirectToActionResult)));

            Assert.That(((RedirectToActionResult)result).ControllerName, Is.Null);
            Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task WhenEditCategoryGetMethodIsCalledAndModelExistsThenViewResultIsReturned()
        {
            this.categoryRepository.Setup(c => c.GetAsync(It.IsAny<int>())).ReturnsAsync(new Category());

            IActionResult result = await this.homeController.EditCategory(1);

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenEditCategoryGetMethodIsCalledAndModelExistsThenUseDefaultView()
        {
            this.categoryRepository.Setup(c => c.GetAsync(It.IsAny<int>())).ReturnsAsync(new Category());

            IActionResult result = await this.homeController.EditCategory(1);

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenEditCategoryGetMethodIsCalledAndModelDoesNotExistsThenRedirectResultIsReturned()
        {
            IActionResult result = await this.homeController.EditCategory(1);

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenEditCategoryPostMethodIsCalledAndModelStateIsInvalidThenUseDefaultView()
        {
            this.homeController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.homeController.EditCategory(new Category());

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenEditCategoryPostMethodIsCalledAndModelStateIsInvalidThenViewResultIsReturned()
        {
            this.homeController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.homeController.EditCategory(new Category());

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenEditCategoryPostMethodIsCalledAndCategoryGroupAlreadyExistsThenAddModelError()
        {
            this.categoryRepository.Setup(c => c.UpdateAsync(It.IsAny<Category>())).ThrowsAsync(new EntityAlreadyExistsException("Test"));

            await this.homeController.EditCategory(new Category());

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenEditCategoryPostMethodIsCalledAndAddFailsThenAddModelError()
        {
            this.categoryRepository.Setup(c => c.UpdateAsync(It.IsAny<Category>())).ThrowsAsync(new RepositoryException("Test"));

            await this.homeController.EditCategory(new Category());

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenEditCategoryPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToActionResultIsReturned()
        {
            IActionResult result = await this.homeController.EditCategory(new Category());

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenEditCategoryPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToCorrectAction()
        {
            IActionResult result = await this.homeController.EditCategory(new Category());

            Assume.That(result, Is.TypeOf(typeof(RedirectToActionResult)));

            Assert.That(((RedirectToActionResult)result).ControllerName, Is.Null);
            Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task WhenDeleteCategoryGetMethodIsCalledAndModelExistsThenViewResultIsReturned()
        {
            this.categoryRepository.Setup(c => c.GetAsync(It.IsAny<int>())).ReturnsAsync(new Category());

            IActionResult result = await this.homeController.DeleteCategory(1);

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenDeleteCategoryGetMethodIsCalledAndModelExistsThenUseDefaultView()
        {
            this.categoryRepository.Setup(c => c.GetAsync(It.IsAny<int>())).ReturnsAsync(new Category());

            IActionResult result = await this.homeController.DeleteCategory(1);

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenDeleteCategoryGetMethodIsCalledAndModelDoesNotExistsThenRedirectResultIsReturned()
        {
            IActionResult result = await this.homeController.DeleteCategory(1);

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenDeleteCategoryPostMethodIsCalledAndModelStateIsInvalidThenUseDefaultView()
        {
            this.homeController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.homeController.DeleteCategory(new Category());

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenDeleteCategoryPostMethodIsCalledAndModelStateIsInvalidThenViewResultIsReturned()
        {
            this.homeController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.homeController.DeleteCategory(new Category());

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenDeleteCategoryPostMethodIsCalledAndAddFailsThenAddModelError()
        {
            this.categoryRepository.Setup(c => c.DeleteAsync(It.IsAny<int>())).ThrowsAsync(new RepositoryException("Test"));

            await this.homeController.DeleteCategory(new Category());

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenDeleteCategoryPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToActionResultIsReturned()
        {
            IActionResult result = await this.homeController.DeleteCategory(new Category());

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenDeleteCategoryPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToCorrectAction()
        {
            IActionResult result = await this.homeController.DeleteCategory(new Category());

            Assume.That(result, Is.TypeOf(typeof(RedirectToActionResult)));

            Assert.That(((RedirectToActionResult)result).ControllerName, Is.Null);
            Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task WhenDeleteCategoryPostMethodIsCalledAndBudgetIsNotZeroThenAddModelError()
        {
            await this.homeController.DeleteCategory(new Category { Budget = 1.00M });

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenAssignMoneyGetMethodIsCalledAndModelExistsThenViewResultIsReturned()
        {
            IActionResult result = await this.homeController.AssignMoney();

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenAssignMoneyGetMethodIsCalledAndModelExistsThenUseDefaultView()
        {
            IActionResult result = await this.homeController.AssignMoney();

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenAssignMoneyGetMethodIsCalledThenCategoriesAreSetOnModel()
        {
            this.categoryRepository.Setup(c => c.GetAsync(It.IsAny<Guid>())).ReturnsAsync(
                new List<Category> { new Category(), new Category() });

            IActionResult result = await this.homeController.AssignMoney();

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            var model = (AssignMoneyViewModel)((ViewResult) result).Model;

            Assert.That(model.Categories.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task WhenAssignMoneyPostMethodIsCalledAndModelStateIsInvalidThenUseDefaultView()
        {
            this.homeController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.homeController.AssignMoney(new AssignMoneyViewModel());

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenAssignMoneyPostMethodIsCalledAndModelStateIsInvalidThenViewResultIsReturned()
        {
            this.homeController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.homeController.AssignMoney(new AssignMoneyViewModel());

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenAssignMoneyPostMethodIsCalledAndOperationFailsThenAddModelError()
        {
            this.categoryRepository.Setup(c => c.AssignMoneyAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<Guid>())).ThrowsAsync(new RepositoryException("Test"));

            await this.homeController.AssignMoney(new AssignMoneyViewModel { Amount = 1.00M});

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenAssignMoneyPostMethodIsCalledAndMoneyIsAssignedThenRedirectToActionResultIsReturned()
        {
            IActionResult result = await this.homeController.AssignMoney(new AssignMoneyViewModel());

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenAssignMoneyPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToCorrectAction()
        {
            IActionResult result = await this.homeController.AssignMoney(new AssignMoneyViewModel());

            Assume.That(result, Is.TypeOf(typeof(RedirectToActionResult)));

            Assert.That(((RedirectToActionResult)result).ControllerName, Is.Null);
            Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task WhenAssignMoneyPostMethodIsCalledAndAmountIsLessThanZeroThenAddModelError()
        {
            await this.homeController.AssignMoney(new AssignMoneyViewModel { Amount = -1.00M });

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenReassignMoneyGetMethodIsCalledAndModelExistsThenViewResultIsReturned()
        {
            IActionResult result = await this.homeController.ReassignMoney(1);

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenReassignMoneyGetMethodIsCalledAndModelExistsThenUseDefaultView()
        {
            IActionResult result = await this.homeController.ReassignMoney(1);

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenReassignMoneyGetMethodIsCalledThenCategoriesAreSetOnModel()
        {
            this.categoryRepository.Setup(c => c.GetAsync(It.IsAny<Guid>())).ReturnsAsync(
                new List<Category> { new Category(), new Category() });

            IActionResult result = await this.homeController.ReassignMoney(1);

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            var model = (ReassignMoneyViewModel)((ViewResult) result).Model;

            Assert.That(model.Categories.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task WhenReassignMoneyPostMethodIsCalledAndModelStateIsInvalidThenUseDefaultView()
        {
            this.homeController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.homeController.ReassignMoney(new ReassignMoneyViewModel());

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenReassignMoneyPostMethodIsCalledAndModelStateIsInvalidThenViewResultIsReturned()
        {
            this.homeController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.homeController.ReassignMoney(new ReassignMoneyViewModel());

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenReassignMoneyPostMethodIsCalledAndOperationFailsThenAddModelError()
        {
            this.categoryRepository.Setup(c => c.ReassignMoneyAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>())).ThrowsAsync(new RepositoryException("Test"));

            await this.homeController.ReassignMoney(new ReassignMoneyViewModel { Amount = 1.00M, Unassign = false});

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenReassignMoneyPostMethodIsCalledAndMoneyIsAssignedThenRedirectToActionResultIsReturned()
        {
            IActionResult result = await this.homeController.ReassignMoney(new ReassignMoneyViewModel());

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenReassignMoneyPostMethodIsCalledAndOperationIsSuccessfulThenRedirectToCorrectAction()
        {
            IActionResult result = await this.homeController.ReassignMoney(new ReassignMoneyViewModel());

            Assume.That(result, Is.TypeOf(typeof(RedirectToActionResult)));

            Assert.That(((RedirectToActionResult)result).ControllerName, Is.Null);
            Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task WhenReassignMoneyPostMethodIsCalledAndAmountIsLessThanZeroThenAddModelError()
        {
            await this.homeController.ReassignMoney(new ReassignMoneyViewModel() { Amount = -1.00M });

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenReassignMoneyPostMethodIsCalledAndUnassignIsTrueOnModelThenCallUnassignOnRepository()
        {
            await this.homeController.ReassignMoney(new ReassignMoneyViewModel() { Amount = 1.00M, Unassign = true });

            this.categoryRepository.Verify(c => c.UnassignMoneyAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<Guid>()));
        }

        [Test]
        public async Task WhenReassignMoneyPostMethodIsCalledAndUnassignIsFalseOnModelThenCallReassignOnRepository()
        {
            await this.homeController.ReassignMoney(new ReassignMoneyViewModel() { Amount = 1.00M, Unassign = false });

            this.categoryRepository.Verify(c => c.ReassignMoneyAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>()));
        }

        [Test]
        public void WhenErrorGetMethodIsCalledThenViewResultIsReturned()
        {
            IActionResult result = this.homeController.Error();

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public void WhenErrorGetMethodIsCalledThenUseDefaultView()
        {
            IActionResult result = this.homeController.Error();

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public void WhenNotFoundErrorGetMethodIsCalledThenViewResultIsReturned()
        {
            IActionResult result = this.homeController.NotFoundError();

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public void WhenNotFoundErrorGetMethodIsCalledThenUseDefaultView()
        {
            IActionResult result = this.homeController.NotFoundError();

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }
    }
}
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
            this.homeController = new HomeController(this.userRepository.Object, this.categoryGroupRepository.Object);
            this.SetUpContext(this.homeController);

            this.userRepository.Setup(u => u.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser { UserName = "TestUser", Id = Guid.NewGuid() });
        }

        private Mock<IUserRepository> userRepository;
        private Mock<ICategoryGroupRepository> categoryGroupRepository;
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
            Assert.Throws<ArgumentNullException>(() => this.homeController = new HomeController(this.userRepository.Object, null));
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

            var model = (IndexViewModel)((ViewResult) result).Model;

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

            var model = (IndexViewModel)((ViewResult)result).Model;

            Assert.That(model.CategoryGroups.Count(), Is.EqualTo(3));
        }

        [Test]
        public void WhenUserRepositoryIsNullThenThrowNullArguementException()
        {
            Assert.Throws<ArgumentNullException>(() => this.homeController = new HomeController(null, this.categoryGroupRepository.Object));
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
            this.categoryGroupRepository.Setup(c => c.Add(It.IsAny<CategoryGroup>())).ThrowsAsync(new EntityAlreadyExistsException("Test"));

            await this.homeController.AddCategoryGroup(new CategoryGroup());

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenAddCategoryGroupPostMethodIsCalledAndAddFailsThenAddModelError()
        {
            this.categoryGroupRepository.Setup(c => c.Add(It.IsAny<CategoryGroup>())).ThrowsAsync(new RepositoryException("Test"));

            await this.homeController.AddCategoryGroup(new CategoryGroup());

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenAddCategoryGroupPostMethodIsCalledAndAccountIsCreatedThenRedirectToActionResultIsReturned()
        {
            IActionResult result = await this.homeController.AddCategoryGroup(new CategoryGroup());

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenAddCategoryGroupPostMethodIsCalledAndAccountIsCreatedThenRedirectToCorrectAction()
        {
            IActionResult result = await this.homeController.AddCategoryGroup(new CategoryGroup());

            Assume.That(result, Is.TypeOf(typeof(RedirectToActionResult)));

            Assert.That(((RedirectToActionResult)result).ControllerName, Is.Null);
            Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task WhenEditCategoryGroupGetMethodIsCalledAndModelExistsThenViewResultIsReturned()
        {
            this.categoryGroupRepository.Setup(c => c.Get(It.IsAny<int>())).ReturnsAsync(new CategoryGroup());

            IActionResult result = await this.homeController.EditCategoryGroup(1);

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenEditCategoryGroupGetMethodIsCalledAndModelExistsThenUseDefaultView()
        {
            this.categoryGroupRepository.Setup(c => c.Get(It.IsAny<int>())).ReturnsAsync(new CategoryGroup());

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
            this.categoryGroupRepository.Setup(c => c.Update(It.IsAny<CategoryGroup>())).ThrowsAsync(new EntityAlreadyExistsException("Test"));

            await this.homeController.EditCategoryGroup(new CategoryGroup());

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenEditCategoryGroupPostMethodIsCalledAndAddFailsThenAddModelError()
        {
            this.categoryGroupRepository.Setup(c => c.Update(It.IsAny<CategoryGroup>())).ThrowsAsync(new RepositoryException("Test"));

            await this.homeController.EditCategoryGroup(new CategoryGroup());

            Assert.That(this.homeController.ViewData.ModelState.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public async Task WhenEditCategoryGroupPostMethodIsCalledAndAccountIsCreatedThenRedirectToActionResultIsReturned()
        {
            IActionResult result = await this.homeController.AddCategoryGroup(new CategoryGroup());

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenEditCategoryGroupPostMethodIsCalledAndAccountIsCreatedThenRedirectToCorrectAction()
        {
            IActionResult result = await this.homeController.AddCategoryGroup(new CategoryGroup());

            Assume.That(result, Is.TypeOf(typeof(RedirectToActionResult)));

            Assert.That(((RedirectToActionResult)result).ControllerName, Is.Null);
            Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("Index"));
        }
    }
}
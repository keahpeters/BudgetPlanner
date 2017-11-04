using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using BudgetPlanner.Controllers;
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
        public async Task WhenIndexGetMethodIsCalledThenBalanceIsSetOnModel()
        {
            this.userRepository.Setup(u => u.GetBalanceAsync(It.IsAny<string>())).ReturnsAsync(2000.01M);

            IActionResult result = await this.homeController.Index();

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            var model = (IndexViewModel)((ViewResult) result).Model;

            Assert.That(model.Balance, Is.EqualTo(2000.01M));
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
    }
}
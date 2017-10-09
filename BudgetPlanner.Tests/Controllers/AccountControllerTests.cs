using System;
using System.Threading.Tasks;
using BudgetPlanner.Controllers;
using BudgetPlanner.Identity;
using BudgetPlanner.Models;
using BudgetPlanner.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace BudgetPlanner.Tests.Controllers
{
    [TestFixture]
    public class AccountControllerTests
    {
        private Mock<IUserManagerWrapper<ApplicationUser>> userManager;
        private Mock<ISignInManagerWrapper<ApplicationUser>> signInManager;
        private AccountController accountController;

        [SetUp]
        public void SetUp()
        {
            this.userManager = new Mock<IUserManagerWrapper<ApplicationUser>>();
            this.signInManager = new Mock<ISignInManagerWrapper<ApplicationUser>>();
            this.accountController = new AccountController(this.userManager.Object, this.signInManager.Object);
        }

        [Test]
        public void WhenUserManagerIsNullThenThrowNullArguementException()
        {
            Assert.Throws<ArgumentNullException>(() => this.accountController = new AccountController(null, this.signInManager.Object));
        }

        [Test]
        public void WhenSignInManagerIsNullThenThrowNullArguementException()
        {
            Assert.Throws<ArgumentNullException>(() => this.accountController = new AccountController(this.userManager.Object, null));
        }

        [Test]
        public void WhenRegisterGetMethodIsCalledThenViewResultIsReturned()
        {
            IActionResult result = this.accountController.Register();

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public void WhenRegisterGetMethodIsCalledThenUseDefaultView()
        {
            IActionResult result = this.accountController.Register();

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenRegisterPostMethodIsCalledAndModelStateIsInvalidThenViewResultIsReturned()
        {
            this.accountController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.accountController.Register(new RegisterUserViewModel());

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenRegisterPostMethodIsCalledAndModelStateIsInvalidThenUseDefaultView()
        {
            this.accountController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.accountController.Register(new RegisterUserViewModel());

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenRegisterPostMethodIsCalledAndUserCannotBeCreatedThenViewResultIsReturned()
        {
            var model = new RegisterUserViewModel
            {
                Username = "Keah",
                Password = "Password1!",
                ConfirmPassword = "Password1!",
                Email = "test@test.com"
            };

            this.userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError {Description = "Test error"}));

            IActionResult result = await this.accountController.Register(model);

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenRegisterPostMethodIsCalledAndUserCannotBeCreatedThenUseDefaultView()
        {
            var model = new RegisterUserViewModel
            {
                Username = "Keah",
                Password = "Password1!",
                ConfirmPassword = "Password1!",
                Email = "test@test.com"
            };

            this.userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError {Description = "Test error"}));

            IActionResult result = await this.accountController.Register(model);

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenRegisterPostMethodIsCalledAndAccountIsCreatedThenRedirectToActionResultIsReturned()
        {
            var model = new RegisterUserViewModel
            {
                Username = "Keah",
                Password = "Password1!",
                ConfirmPassword = "Password1!",
                Email = "test@test.com"
            };

            this.userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            IActionResult result = await this.accountController.Register(model);

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenRegisterPostMethodIsCalledAndAccountIsCreatedThenRedirectToCorrectAction()
        {
            var model = new RegisterUserViewModel
            {
                Username = "Keah",
                Password = "Password1!",
                ConfirmPassword = "Password1!",
                Email = "test@test.com"
            };

            this.userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            IActionResult result = await this.accountController.Register(model);

            Assume.That(result, Is.TypeOf(typeof(RedirectToActionResult)));

            Assert.That(((RedirectToActionResult)result).ControllerName, Is.EqualTo("Home"));
            Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task WhenRegisterPostMethodIsCalledAndAccountIsCreatedThenSignIn()
        {
            var model = new RegisterUserViewModel
            {
                Username = "Keah",
                Password = "Password1!",
                ConfirmPassword = "Password1!",
                Email = "test@test.com"
            };

            this.userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            IActionResult result = await this.accountController.Register(model);

            Assume.That(result, Is.TypeOf(typeof(RedirectToActionResult)));

            this.signInManager.Verify(s => s.SignInAsync(It.IsAny<ApplicationUser>(), false, It.IsAny<string>()), Times.Once);
        }
    }
}
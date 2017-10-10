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
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

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
        public void WhenLoginGetMethodIsCalledThenUseDefaultView()
        {
            IActionResult result = this.accountController.Login();

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult) result).ViewName, Is.Null);
        }

        [Test]
        public void WhenLoginGetMethodIsCalledThenViewResultIsReturned()
        {
            IActionResult result = this.accountController.Login();

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenLoginPostMethodIsCalledAndAccountIsCreatedThenRedirectToActionResultIsReturned()
        {
            var model = new LoginViewModel
            {
                Username = "Keah",
                Password = "Password1!"
            };

            this.signInManager.Setup(u => u.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(SignInResult.Success);

            IActionResult result = await this.accountController.Login(model);

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenLoginPostMethodIsCalledAndAccountIsCreatedThenRedirectToCorrectAction()
        {
            var model = new LoginViewModel
            {
                Username = "Keah",
                Password = "Password1!"
            };

            this.signInManager.Setup(u => u.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(SignInResult.Success);

            IActionResult result = await this.accountController.Login(model);

            Assume.That(result, Is.TypeOf(typeof(RedirectToActionResult)));

            Assert.That(((RedirectToActionResult) result).ControllerName, Is.EqualTo("Home"));
            Assert.That(((RedirectToActionResult) result).ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task WhenLoginPostMethodIsCalledAndCannotSignInThenUseDefaultView()
        {
            var model = new LoginViewModel
            {
                Username = "Keah",
                Password = "Password1!"
            };

            this.signInManager.Setup(u => u.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(SignInResult.Failed);

            IActionResult result = await this.accountController.Login(model);

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult) result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenLoginPostMethodIsCalledAndCannotSignInThenViewResultIsReturned()
        {
            var model = new LoginViewModel
            {
                Username = "Keah",
                Password = "Password1!"
            };

            this.signInManager.Setup(u => u.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(SignInResult.Failed);

            IActionResult result = await this.accountController.Login(model);

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenLoginPostMethodIsCalledAndModelStateIsInvalidThenUseDefaultView()
        {
            this.accountController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.accountController.Login(new LoginViewModel());

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult) result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenLoginPostMethodIsCalledAndModelStateIsInvalidThenViewResultIsReturned()
        {
            this.accountController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.accountController.Login(new LoginViewModel());

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
        }

        [Test]
        public async Task WhenLogoutIsCalledThenRedirectToActionResultIsReturned()
        {
            IActionResult result = await this.accountController.Logout();

            Assert.That(result, Is.TypeOf(typeof(RedirectToActionResult)));
        }

        [Test]
        public async Task WhenLogoutIsCalledThenRedirectToCorrectAction()
        {
            IActionResult result = await this.accountController.Logout();

            Assume.That(result, Is.TypeOf(typeof(RedirectToActionResult)));

            Assert.That(((RedirectToActionResult) result).ControllerName, Is.EqualTo("Home"));
            Assert.That(((RedirectToActionResult) result).ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task WhenLogoutIsCalledThenSignOut()
        {
            await this.accountController.Logout();

            this.signInManager.Verify(a => a.SignOutAsync(), Times.Once);
        }

        [Test]
        public void WhenRegisterGetMethodIsCalledThenUseDefaultView()
        {
            IActionResult result = this.accountController.Register();

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult) result).ViewName, Is.Null);
        }

        [Test]
        public void WhenRegisterGetMethodIsCalledThenViewResultIsReturned()
        {
            IActionResult result = this.accountController.Register();

            Assert.That(result, Is.TypeOf(typeof(ViewResult)));
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

            Assert.That(((RedirectToActionResult) result).ControllerName, Is.EqualTo("Home"));
            Assert.That(((RedirectToActionResult) result).ActionName, Is.EqualTo("Index"));
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

        [Test]
        public async Task WhenRegisterPostMethodIsCalledAndModelStateIsInvalidThenUseDefaultView()
        {
            this.accountController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.accountController.Register(new RegisterUserViewModel());

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult) result).ViewName, Is.Null);
        }

        [Test]
        public async Task WhenRegisterPostMethodIsCalledAndModelStateIsInvalidThenViewResultIsReturned()
        {
            this.accountController.ViewData.ModelState.AddModelError(string.Empty, "Model error");

            IActionResult result = await this.accountController.Register(new RegisterUserViewModel());

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

            Assert.That(((ViewResult) result).ViewName, Is.Null);
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
        public void WhenSignInManagerIsNullThenThrowNullArguementException()
        {
            Assert.Throws<ArgumentNullException>(() => this.accountController = new AccountController(this.userManager.Object, null));
        }

        [Test]
        public void WhenUserManagerIsNullThenThrowNullArguementException()
        {
            Assert.Throws<ArgumentNullException>(() => this.accountController = new AccountController(null, this.signInManager.Object));
        }
    }
}
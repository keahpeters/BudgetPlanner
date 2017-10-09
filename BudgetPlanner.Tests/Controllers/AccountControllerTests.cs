using System;
using System.Threading;
using System.Threading.Tasks;
using BudgetPlanner.Controllers;
using BudgetPlanner.Models;
using BudgetPlanner.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace BudgetPlanner.Tests.Controllers
{
    [TestFixture]
    public class AccountControllerTests
    {
        private Mock<IUserPasswordStore<ApplicationUser>> userStore;
        private Mock<IHttpContextAccessor> contextAccessor;
        private Mock<IUserClaimsPrincipalFactory<ApplicationUser>> claimsFactory;
        private Mock<IPasswordHasher<ApplicationUser>> passwordHasher;
        private UserManager<ApplicationUser> userManager;
        private SignInManager<ApplicationUser> signInManager;
        private AccountController accountController;

        [SetUp]
        public void SetUp()
        {
            this.userStore = new Mock<IUserPasswordStore<ApplicationUser>>();
            this.contextAccessor = new Mock<IHttpContextAccessor>();
            this.claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            this.passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
            this.userManager = new UserManager<ApplicationUser>(this.userStore.Object, null, this.passwordHasher.Object, null, null, null, null, null, null);
            this.signInManager = new SignInManager<ApplicationUser>(this.userManager, this.contextAccessor.Object, this.claimsFactory.Object, null, null, null);
            this.accountController = new AccountController(this.userManager, this.signInManager);
        }

        [Test]
        public void WhenUserManagerIsNullThenThrowNullArguementException()
        {
            Assert.Throws<ArgumentNullException>(() => this.accountController = new AccountController(null, this.signInManager));
        }

        [Test]
        public void WhenSignInManagerIsNullThenThrowNullArguementException()
        {
            Assert.Throws<ArgumentNullException>(() => this.accountController = new AccountController(this.userManager, null));
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

            this.userStore.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Failed());

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

            this.userStore.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Failed());

            IActionResult result = await this.accountController.Register(model);

            Assume.That(result, Is.TypeOf(typeof(ViewResult)));

            Assert.That(((ViewResult)result).ViewName, Is.Null);
        }
    }
}
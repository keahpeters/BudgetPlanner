using System;
using BudgetPlanner.Controllers;
using BudgetPlanner.Models;
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
        private Mock<IUserStore<ApplicationUser>> userStore;
        private Mock<IHttpContextAccessor> contextAccessor;
        private Mock<IUserClaimsPrincipalFactory<ApplicationUser>> claimsFactory;
        private UserManager<ApplicationUser> userManager;
        private SignInManager<ApplicationUser> signInManager;
        private AccountController accountController;

        [SetUp]
        public void SetUp()
        {
            this.userStore = new Mock<IUserStore<ApplicationUser>>();
            this.contextAccessor = new Mock<IHttpContextAccessor>();
            this.claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            this.userManager = new UserManager<ApplicationUser>(this.userStore.Object, null, null, null, null, null, null, null, null);
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
    }
}
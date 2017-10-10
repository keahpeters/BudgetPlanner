using BudgetPlanner.ViewComponents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using NUnit.Framework;

namespace BudgetPlanner.Tests.ViewComponentTests
{
    [TestFixture]
    public class LoginLogoutViewComponentTests
    {
        private LoginLogoutViewComponent viewComponent;

        [SetUp]
        public void SetUp()
        {
            this.viewComponent = new LoginLogoutViewComponent();
        }

        [Test]
        public void WhenInvokeIsCalledThenUseDefaultView()
        {
            IViewComponentResult result = this.viewComponent.Invoke();

            Assume.That(result, Is.TypeOf(typeof(ViewViewComponentResult)));

            Assert.That(((ViewViewComponentResult) result).ViewName, Is.Null);
        }

        [Test]
        public void WhenInvokeIsCalledThenViewComponentResultIsReturned()
        {
            IViewComponentResult result = this.viewComponent.Invoke();

            Assert.That(result, Is.TypeOf(typeof(ViewViewComponentResult)));
        }
    }
}
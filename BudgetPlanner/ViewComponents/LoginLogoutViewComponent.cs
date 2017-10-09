using Microsoft.AspNetCore.Mvc;

namespace BudgetPlanner.ViewComponents
{
    public class LoginLogoutViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return this.View();
        }
    }
}
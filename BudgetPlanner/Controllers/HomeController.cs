using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetPlanner.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public string Index()
        {
            return "Welcome";
        }
    }
}

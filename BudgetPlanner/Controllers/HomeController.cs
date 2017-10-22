using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetPlanner.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return this.View();
        }
    }
}

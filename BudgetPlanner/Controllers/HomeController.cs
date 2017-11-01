using System.Threading.Tasks;
using BudgetPlanner.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetPlanner.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IUserRepository userRepository;

        public HomeController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ViewResult> Index()
        {
            decimal balance = await this.userRepository.GetBalanceAsync(this.User.Identity.Name);

            return this.View(balance);
        }
    }
}

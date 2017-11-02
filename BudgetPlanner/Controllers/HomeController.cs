using System.Threading.Tasks;
using BudgetPlanner.Repositories;
using BudgetPlanner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetPlanner.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IUserRepository userRepository;
        private readonly ICategoryGroupRepository categoryGroupRepository;

        public HomeController(IUserRepository userRepository, ICategoryGroupRepository categoryGroupRepository)
        {
            this.userRepository = userRepository;
            this.categoryGroupRepository = categoryGroupRepository;
        }

        [HttpGet]
        public async Task<ViewResult> Index()
        {
            var model = new IndexViewModel
            {
                Balance = await this.userRepository.GetBalanceAsync(this.User.Identity.Name),
                CategoryGroups = await this.categoryGroupRepository.GetByUserNameAsync(this.User.Identity.Name)
            };

            return this.View(model);
        }
    }
}

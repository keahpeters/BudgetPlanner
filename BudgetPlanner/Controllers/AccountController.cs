using System.Threading.Tasks;
using BudgetPlanner.Models;
using BudgetPlanner.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BudgetPlanner.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterUserViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email
                };

                IdentityResult createResult = await this.userManager.CreateAsync(user, model.Password);
                if (createResult.Succeeded)
                {
                    await this.signInManager.SignInAsync(user, false);
                    return this.RedirectToAction("Index", "Home");
                }
                foreach (IdentityError error in createResult.Errors)
                    this.ModelState.AddModelError(string.Empty, error.Description);
            }

            return this.View();
        }
    }
}
using System;
using System.Threading.Tasks;
using BudgetPlanner.Infrastructure;
using BudgetPlanner.Models;
using BudgetPlanner.Repositories;
using BudgetPlanner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetPlanner.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ICategoryGroupRepository categoryGroupRepository;
        private readonly IUserRepository userRepository;

        public HomeController(IUserRepository userRepository, ICategoryGroupRepository categoryGroupRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.categoryGroupRepository = categoryGroupRepository ?? throw new ArgumentNullException(nameof(categoryGroupRepository));
        }

        [HttpGet]
        public async Task<ViewResult> Index()
        {
            var model = new IndexViewModel
            {
                ApplicationUser = await this.userRepository.FindByNameAsync(this.User.Identity.Name),
                CategoryGroups = await this.categoryGroupRepository.GetByUserNameAsync(this.User.Identity.Name)
            };

            return this.View(model);
        }

        [HttpGet]
        public IActionResult AddCategoryGroup()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCategoryGroup(CategoryGroup categoryGroup)
        {
            if (this.ModelState.IsValid)
            {
                ApplicationUser user = await this.userRepository.FindByNameAsync(this.User.Identity.Name);
                categoryGroup.UserId = user.Id;

                try
                {
                    await this.categoryGroupRepository.Add(categoryGroup);
                    return this.RedirectToAction("Index");
                }
                catch (EntityAlreadyExistsException)
                {
                    this.ModelState.AddModelError(string.Empty, "A category group with this name already exists.");
                }
                catch (RepositoryException)
                {
                    this.ModelState.AddModelError(string.Empty, "Failed to add category group, an unexpected error occured.");
                }
            }

            return this.View(categoryGroup);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategoryGroup(int id)
        {
            CategoryGroup model = await this.categoryGroupRepository.Get(id);

            if (model == null)
                return this.RedirectToAction("Index");

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategoryGroup(CategoryGroup categoryGroup)
        {
            if (this.ModelState.IsValid)
            {
                try
                {
                    await this.categoryGroupRepository.Update(categoryGroup);
                    return this.RedirectToAction("Index");
                }
                catch (EntityAlreadyExistsException)
                {
                    this.ModelState.AddModelError(string.Empty, "A category group with this name already exists.");
                }
                catch (RepositoryException)
                {
                    this.ModelState.AddModelError(string.Empty, "Failed to edit category group, an unexpected error occured.");
                }
            }

            return this.View(categoryGroup);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCategoryGroup(int id)
        {
            CategoryGroup model = await this.categoryGroupRepository.Get(id);

            if (model == null)
                return this.RedirectToAction("Index");

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategoryGroup(CategoryGroup categoryGroup)
        {
            if (this.ModelState.IsValid)
            {
                try
                {
                    await this.categoryGroupRepository.Delete(categoryGroup.Id);
                    return this.RedirectToAction("Index");
                }
                catch (ChildEntitiesExistException)
                {
                    this.ModelState.AddModelError(string.Empty, "This category group has categories assigned. Please delete child categories before deleting category group.");
                }
                catch (RepositoryException)
                {
                    this.ModelState.AddModelError(string.Empty, "Failed to delete category group, an unexpected error occured.");
                }
            }

            return this.View(categoryGroup);
        }
    }
}
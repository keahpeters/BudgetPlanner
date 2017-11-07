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
        private readonly ICategoryRepository categoryRepository;
        private readonly ICategoryGroupRepository categoryGroupRepository;
        private readonly IUserRepository userRepository;

        public HomeController(IUserRepository userRepository, ICategoryGroupRepository categoryGroupRepository, ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
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

        [HttpGet]
        public IActionResult AddCategory(int categoryGroupId)
        {
            Category model = new Category { CategoryGroupId = categoryGroupId };

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(Category category)
        {
            if (this.ModelState.IsValid)
            {
                try
                {
                    await this.categoryRepository.Add(category);
                    return this.RedirectToAction("Index");
                }
                catch (EntityAlreadyExistsException)
                {
                    this.ModelState.AddModelError(string.Empty, "A category with this name already exists for this category group.");
                }
                catch (RepositoryException)
                {
                    this.ModelState.AddModelError(string.Empty, "Failed to add category, an unexpected error occured.");
                }
            }

            return this.View(category);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            Category model = await this.categoryRepository.Get(id);

            if (model == null)
                return this.RedirectToAction("Index");

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(Category category)
        {
            if (this.ModelState.IsValid)
            {
                try
                {
                    await this.categoryRepository.Update(category);
                    return this.RedirectToAction("Index");
                }
                catch (EntityAlreadyExistsException)
                {
                    this.ModelState.AddModelError(string.Empty, "A category with this name already exists for this category group.");
                }
                catch (RepositoryException)
                {
                    this.ModelState.AddModelError(string.Empty, "Failed to edit category, an unexpected error occured.");
                }
            }

            return this.View(category);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            Category model = await this.categoryRepository.Get(id);

            if (model == null)
                return this.RedirectToAction("Index");

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(Category category)
        {
            if (category.Budget != 0)
                this.ModelState.AddModelError(string.Empty, "The budget for this category must be 0.00 to delete it.");

            if (this.ModelState.IsValid)
            {
                try
                {
                    await this.categoryRepository.Delete(category.Id);
                    return this.RedirectToAction("Index");
                }
                catch (RepositoryException)
                {
                    this.ModelState.AddModelError(string.Empty, "Failed to delete category, an unexpected error occured.");
                }
            }

            return this.View(category);
        }

        [HttpGet]
        public async Task<IActionResult> AssignMoney()
        {
            ApplicationUser user = await this.userRepository.FindByNameAsync(this.User.Identity.Name);
            var model = new AssignMoneyViewModel
            {
                Categories = await this.categoryRepository.Get(user.Id)
            };

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignMoney(AssignMoneyViewModel model)
        {
            ApplicationUser user = await this.userRepository.FindByNameAsync(this.User.Identity.Name);

            if (model.Amount < 0)
                this.ModelState.AddModelError(string.Empty, "The amount can not be negative");

            if (this.ModelState.IsValid)
            {
                try
                {
                    if (model.Amount != 0)
                        await this.categoryRepository.AssignMoney(model.CategoryId, model.Amount, user.Id);

                    return this.RedirectToAction("Index");
                }
                catch (RepositoryException)
                {
                    this.ModelState.AddModelError(string.Empty, "Failed to assign money, an unexpected error occured.");
                }
            }

            model.Categories = await this.categoryRepository.Get(user.Id);
            return this.View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ReassignMoney(int sourceCategoryId)
        {
            ApplicationUser user = await this.userRepository.FindByNameAsync(this.User.Identity.Name);
            var model = new ReassignMoneyViewModel
            {
                SourceCategoryId = sourceCategoryId,
                Categories = await this.categoryRepository.Get(user.Id)
            };

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ReassignMoney(ReassignMoneyViewModel model)
        {
            ApplicationUser user = await this.userRepository.FindByNameAsync(this.User.Identity.Name);

            if (model.Amount < 0)
                this.ModelState.AddModelError(string.Empty, "The amount can not be negative");

            if (this.ModelState.IsValid)
            {
                try
                {
                    if (model.Amount != 0)
                    {
                        if (model.Unassign)
                            await this.categoryRepository.UnassignMoney(model.SourceCategoryId, model.Amount, user.Id);
                        else
                            await this.categoryRepository.ReassignMoney(model.SourceCategoryId, model.DestinationCategoryId, model.Amount);
                    }

                    return this.RedirectToAction("Index");
                }
                catch (RepositoryException)
                {
                    this.ModelState.AddModelError(string.Empty, "Failed to assign money, an unexpected error occured.");
                }
            }

            model.Categories = await this.categoryRepository.Get(user.Id);
            return this.View(model);
        }
    }
}
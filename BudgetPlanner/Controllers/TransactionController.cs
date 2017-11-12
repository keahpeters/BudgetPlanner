using System;
using System.Collections.Generic;
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
    public class TransactionController : Controller
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly ITransactionRepository transactionRepository;
        private readonly IUserRepository userRepository;

        public TransactionController(IUserRepository userRepository, ITransactionRepository transactionRepository, ICategoryRepository categoryRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            this.categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        }

        [HttpGet]
        public async Task<ViewResult> Index()
        {
            ApplicationUser user = await this.userRepository.FindByNameAsync(this.User.Identity.Name);
            IEnumerable<Transaction> transactions = await this.transactionRepository.Get(user.Id);

            return this.View(transactions);
        }

        [HttpGet]
        public async Task<IActionResult> AddTransaction()
        {
            ApplicationUser user = await this.userRepository.FindByNameAsync(this.User.Identity.Name);
            var model = new TransactionViewModel
            {
                Categories = await this.categoryRepository.Get(user.Id)
            };

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddTransaction(TransactionViewModel model)
        {
            if (model.Amount <= 0)
                this.ModelState.AddModelError(string.Empty, "The amount must be greater than 0.00");

            ApplicationUser user = await this.userRepository.FindByNameAsync(this.User.Identity.Name);

            if (this.ModelState.IsValid)
            {
                var transaction = new Transaction
                {
                    UserId = user.Id,
                    CategoryId = model.CategoryId,
                    Date = model.Date,
                    Amount = model.Amount,
                    Payee = model.Payee,
                    Memo = model.Memo,
                    IsInTransaction = model.IsInTransaction
                };

                try
                {
                    await this.transactionRepository.Add(transaction);
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
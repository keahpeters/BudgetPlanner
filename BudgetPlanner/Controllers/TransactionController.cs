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
                Categories = await this.categoryRepository.GetAsync(user.Id),
                Date = DateTime.Today
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
                    this.ModelState.AddModelError(string.Empty, "Failed to add transaction, an unexpected error occured.");
                }
            }

            model.Categories = await this.categoryRepository.GetAsync(user.Id);
            return this.View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditTransaction(int id)
        {
            ApplicationUser user = await this.userRepository.FindByNameAsync(this.User.Identity.Name);
            Transaction transaction = await this.transactionRepository.Get(id);

            if (transaction == null)
                return this.RedirectToAction("Index");

            var model = new TransactionViewModel
            {
                Categories = await this.categoryRepository.GetAsync(user.Id),
                Id = transaction.Id,
                UserId = transaction.UserId,
                CategoryId = transaction.CategoryId,
                Date = transaction.Date,
                Amount = transaction.Amount,
                Payee = transaction.Payee,
                Memo = transaction.Memo,
                IsInTransaction = transaction.IsInTransaction
            };

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditTransaction(TransactionViewModel model)
        {
            if (model.Amount <= 0)
                this.ModelState.AddModelError(string.Empty, "The amount must be greater than 0.00");

            ApplicationUser user = await this.userRepository.FindByNameAsync(this.User.Identity.Name);

            if (this.ModelState.IsValid)
            {
                var newTransaction = new Transaction
                {
                    Id = model.Id,
                    UserId = user.Id,
                    CategoryId = model.CategoryId,
                    Date = model.Date,
                    Amount = model.Amount,
                    Payee = model.Payee,
                    Memo = model.Memo,
                    IsInTransaction = model.IsInTransaction
                };

                Transaction oldTransaction = await this.transactionRepository.Get(model.Id);

                try
                {
                    if (oldTransaction.CategoryId != newTransaction.CategoryId
                        || oldTransaction.Amount != newTransaction.Amount
                        || oldTransaction.IsInTransaction != newTransaction.IsInTransaction)
                        await this.transactionRepository.UpdateAndReapplyTransaction(newTransaction, oldTransaction);
                    else
                        await this.transactionRepository.Update(newTransaction);

                    return this.RedirectToAction("Index");
                }
                catch (RepositoryException)
                {
                    this.ModelState.AddModelError(string.Empty, "Failed to edit transaction, an unexpected error occured.");
                }
            }

            model.Categories = await this.categoryRepository.GetAsync(user.Id);
            return this.View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            Transaction model = await this.transactionRepository.Get(id);

            if (model == null)
                return this.RedirectToAction("Index");

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTransaction(Transaction model)
        {
            if (this.ModelState.IsValid)
            {
                try
                {
                    await this.transactionRepository.Delete(model.Id);
                    return this.RedirectToAction("Index");
                }
                catch (RepositoryException)
                {
                    this.ModelState.AddModelError(string.Empty, "Failed to delete transaction, an unexpected error occured.");
                }
            }

            return this.View(model);
        }
    }
}
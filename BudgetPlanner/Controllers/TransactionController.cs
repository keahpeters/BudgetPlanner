using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetPlanner.Models;
using BudgetPlanner.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetPlanner.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ITransactionRepository transactionRepository;
        private readonly IUserRepository userRepository;

        public TransactionController(IUserRepository userRepository, ITransactionRepository transactionRepository)
        {
            this.userRepository = userRepository;
            this.transactionRepository = transactionRepository;
        }

        [HttpGet]
        public async Task<ViewResult> Index()
        {
            ApplicationUser user = await this.userRepository.FindByNameAsync(this.User.Identity.Name);
            IEnumerable<Transaction> transactions = await this.transactionRepository.Get(user.Id);

            return this.View(transactions);
        }
    }
}
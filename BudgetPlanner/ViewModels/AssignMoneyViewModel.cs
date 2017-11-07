using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BudgetPlanner.Models;

namespace BudgetPlanner.ViewModels
{
    public class AssignMoneyViewModel
    {
        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        public int CategoryId { get; set; }

        public IEnumerable<Category> Categories { get; set; }
    }
}
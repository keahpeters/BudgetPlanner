using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BudgetPlanner.Models;

namespace BudgetPlanner.ViewModels
{
    public class ReassignMoneyViewModel
    {
        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        public int SourceCategoryId { get; set; }

        public int DestinationCategoryId { get; set; }

        [Display(Name = "Move to un-budgeted balance")]
        public bool Unassign { get; set; }

        public IEnumerable<Category> Categories { get; set; }
    }
}
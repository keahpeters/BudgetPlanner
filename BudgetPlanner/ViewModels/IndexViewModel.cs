using System.Collections.Generic;
using BudgetPlanner.Models;

namespace BudgetPlanner.ViewModels
{
    public class IndexViewModel
    {
        public decimal Balance { get; set; }

        public IEnumerable<CategoryGroup> CategoryGroups { get; set; }
    }
}
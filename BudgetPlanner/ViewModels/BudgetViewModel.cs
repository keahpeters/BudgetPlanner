using System.Collections.Generic;
using BudgetPlanner.Models;

namespace BudgetPlanner.ViewModels
{
    public class BudgetViewModel
    {
        public ApplicationUser ApplicationUser { get; set; }

        public IEnumerable<CategoryGroup> CategoryGroups { get; set; }
    }
}
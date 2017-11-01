using System.Collections.Generic;

namespace BudgetPlanner.Models
{
    public class CategoryGroup
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<Category> Categories { get; set; }
    }
}
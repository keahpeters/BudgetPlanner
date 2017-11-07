using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public decimal Budget { get; set; }

        public int CategoryGroupId { get; set; }

        public string CategoryGroup { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BudgetPlanner.Models
{
    public class CategoryGroup
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public List<Category> Categories { get; set; }

        public Guid UserId { get; set; }

        public decimal Budget => this.Categories?.Sum(c => c.Budget) ?? 0.00M;
    }
}
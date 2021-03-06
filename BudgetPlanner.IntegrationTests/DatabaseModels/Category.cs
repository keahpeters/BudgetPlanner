﻿using ServiceStack.DataAnnotations;

namespace BudgetPlanner.IntegrationTests.DatabaseModels
{
    public class Category
    {
        [PrimaryKey]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int CategoryGroupId { get; set; }

        [Required]
        [Default(typeof(decimal), "0.00")]
        public decimal Budget { get; set; }
    }
}

using ServiceStack.DataAnnotations;

namespace BudgetPlanner.IntegrationTests.DatabaseModels
{
    public class CategoryGroup
    {
        [PrimaryKey]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string UserId { get; set; }
    }
}

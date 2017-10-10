using System;
using ServiceStack.DataAnnotations;

namespace BudgetPlanner.IntegrationTests
{
    /// <summary>
    ///     This class is used to create the ApplicationUser table in the in memory sqlite database
    /// </summary>
    public class ApplicationUser
    {
        [PrimaryKey]
        public string Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Index(Unique = true)]
        [Required]
        public string UserName { get; set; }
    }
}
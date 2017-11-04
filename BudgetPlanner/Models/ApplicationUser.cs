using Microsoft.AspNetCore.Identity;
using System;

namespace BudgetPlanner.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public override Guid Id { get; set; } = Guid.NewGuid();

        public decimal Balance { get; set; }
    }
}

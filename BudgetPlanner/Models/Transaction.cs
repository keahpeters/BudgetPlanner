using System;

namespace BudgetPlanner.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }

        public DateTime Date { get; set; }

        public int? CategoryId { get; set; }

        public string Category { get; set; }

        public decimal Amount { get; set; }

        public string Payee { get; set; }

        public string Memo { get; set; }

        public bool IsInTransaction { get; set; }
    }
}
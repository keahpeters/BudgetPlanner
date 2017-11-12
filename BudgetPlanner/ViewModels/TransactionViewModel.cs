using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BudgetPlanner.Models;

namespace BudgetPlanner.ViewModels
{
    public class TransactionViewModel
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }

        public int? CategoryId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        public string Payee { get; set; }

        public string Memo { get; set; }

        [Display(Name = "Incoming Transaction")]
        public bool IsInTransaction { get; set; }

        public IEnumerable<Category> Categories { get; set; }
    }
}
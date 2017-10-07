using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.ViewModels
{
    public class RegisterUserViewModel
    {
        [Required, MaxLength(256)]
        public string Username { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

        [Required, MaxLength(256), DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}

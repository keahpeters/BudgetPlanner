using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace BudgetPlanner.Identity
{
    public interface ISignInManagerWrapper<in TUser>
    {
        Task SignInAsync(TUser user, bool isPersistent, string authenticationMethod = null);

        Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure);

        Task SignOutAsync();
    }

    public class SignInManagerWrapper<TUser> : ISignInManagerWrapper<TUser> where TUser : class
    {
        private readonly SignInManager<TUser> signInManager;

        public SignInManagerWrapper(SignInManager<TUser> signInManager)
        {
            this.signInManager = signInManager;
        }

        public Task SignInAsync(TUser user, bool isPersistent, string authenticationMethod = null)
        {
            return this.signInManager.SignInAsync(user, isPersistent, authenticationMethod);
        }

        public Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            return this.signInManager.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
        }

        public Task SignOutAsync()
        {
            return this.signInManager.SignOutAsync();
        }
    }
}
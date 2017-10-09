using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace BudgetPlanner.Identity
{
    public interface IUserManagerWrapper<in TUser> : IDisposable where TUser : class
    {
        Task<IdentityResult> CreateAsync(TUser user, string password);
    }

    public class UserManagerWrapper<TUser> : IUserManagerWrapper<TUser> where TUser : class
    {
        private readonly UserManager<TUser> userManager;

        public UserManagerWrapper(UserManager<TUser> userManager)
        {
            this.userManager = userManager;
        }

        public void Dispose()
        {
            this.userManager.Dispose();
        }

        public Task<IdentityResult> CreateAsync(TUser user, string password)
        {
            return this.userManager.CreateAsync(user, password);
        }
    }
}
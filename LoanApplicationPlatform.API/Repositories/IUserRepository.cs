using LoanApplicationPlatform.API.Entities;
using LoanApplicationPlatform.API.Helpers;

namespace LoanApplicationPlatform.API.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username, bool ignoreQueryFilters = false);
        Task<User?> GetByIdAsync(int id);
        Task<PagedList<User>> GetTeamMembersAsync(UserResourceParameters parameters);
        void AddUser(User user);
        Task<bool> SaveChangesAsync();
    }
}

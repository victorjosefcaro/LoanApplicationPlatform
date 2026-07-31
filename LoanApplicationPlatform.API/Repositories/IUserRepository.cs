using LoanApplicationPlatform.API.Entities;

namespace LoanApplicationPlatform.API.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username, bool ignoreQueryFilters = false);
        Task<User?> GetByIdAsync(int id);
        void AddUser(User user);
        Task<bool> SaveChangesAsync();
    }
}

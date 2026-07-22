using LoanApplicationPlatform.API.DbContexts;
using LoanApplicationPlatform.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanApplicationPlatform.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly LoanApplicationPlatformContext _context;

        public UserRepository(LoanApplicationPlatformContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User?> GetByUsernameAsync(string username, bool ignoreQueryFilters = false)
        {
            if (ignoreQueryFilters)
            {
                return await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Username == username);
            }
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public void AddUser(User user)
        {
            _context.Users.Add(user);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() >= 0);
        }
    }
}

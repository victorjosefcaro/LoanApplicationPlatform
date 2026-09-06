using LoanApplicationPlatform.API.DbContexts;
using LoanApplicationPlatform.API.Entities;
using LoanApplicationPlatform.API.Helpers;
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

        public async Task<PagedList<User>> GetTeamMembersAsync(UserResourceParameters parameters)
        {
            // Built on the DbSet without IgnoreQueryFilters, so the global tenant
            // filter automatically scopes results to the caller's tenant. "Team members"
            // are all staff (non-Applicant) accounts.
            var collection = _context.Users.Where(u => u.Role != "Applicant");

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var search = parameters.Search.Trim();
                collection = collection.Where(u => u.Username.Contains(search));
            }

            var orderedCollection = collection.OrderBy(u => u.Username);
            return await PagedList<User>.CreateAsync(orderedCollection, parameters.PageNumber, parameters.PageSize);
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

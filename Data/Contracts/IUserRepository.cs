using System.Threading;
using System.Threading.Tasks;
using Entities.Entities;

namespace Data.Repositories
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        Task<ApplicationUser> GetByUserAndPass(string username, string password, CancellationToken cancellationToken);

        Task AddAsync(ApplicationUser user, string password, CancellationToken cancellationToken);
        Task UpdateSecurityStampAsync(ApplicationUser user, CancellationToken cancellationToken);
        Task UpdateLastLoginDateAsync(ApplicationUser user, CancellationToken cancellationToken);
    }
}
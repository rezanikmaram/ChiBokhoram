using Data.DbContext;
using Entities.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebFrameWork.Configuration
{
    public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser,
       ApplicationRole>
    {
        public CustomClaimsPrincipalFactory(UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            AppDbContext context)
            : base(userManager, roleManager, optionsAccessor)
        {
            Context = context;
        }

        public AppDbContext Context { get; }

        public async override Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);

            ((ClaimsIdentity)principal.Identity).AddClaims(
                new[] {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("FirstName",user.FirstName),
                    new Claim("LastName",user.LastName),
                    new Claim("UserId",user.Id.ToString()),
                 });

            return principal;
        }
    }
}

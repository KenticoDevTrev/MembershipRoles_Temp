using Kentico.Membership;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using XperienceCommunity.MemberRoles.Models;

namespace XperienceCommunity.MemberRoles.Repositories.Implementation
{
    internal class MemberAuthenticationContext(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager) : IMemberAuthenticationContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task<AuthenticationContext> GetAuthenticationContext()
        {
            var username = "public";
            var authenticated = false;
            var roles = new List<string>();

            var context = _httpContextAccessor.HttpContext;
            if (context is not null) {
                var identity = context.User.Identities.FirstOrDefault();
                if (identity is not null && identity.Name is not null) {
                    username = identity.Name;
                    authenticated = identity.IsAuthenticated;
                }
                var user = (await _userManager.GetUserAsync(context.User));
                if (user != null) {
                    roles.AddRange((await _userManager.GetRolesAsync(user)).Select(x => x.ToLowerInvariant()));
                }
            }

            // Just double checking for public to set not authenticated, roles may still apply if there is customization to set roles on the public user i suppose
            if (username.Equals("public", StringComparison.OrdinalIgnoreCase)) {
                authenticated = false;
            }

            return new AuthenticationContext(authenticated, username, [.. roles]);
        }
    }
}

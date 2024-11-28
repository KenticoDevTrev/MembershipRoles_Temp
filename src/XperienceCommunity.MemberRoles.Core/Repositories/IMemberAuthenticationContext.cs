using XperienceCommunity.MemberRoles.Models;

namespace XperienceCommunity.MemberRoles.Repositories
{
    public interface IMemberAuthenticationContext
    {
        /// <summary>
        /// Gets the current Authenticated User and roles.
        /// </summary>
        /// <returns>The basic Authenticated information for the current user.</returns>
        public Task<AuthenticationContext> GetAuthenticationContext();
    }
}

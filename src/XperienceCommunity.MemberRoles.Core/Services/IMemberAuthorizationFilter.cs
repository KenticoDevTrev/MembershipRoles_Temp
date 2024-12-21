namespace XperienceCommunity.MemberRoles.Services
{
    public interface IMemberAuthorizationFilter
    {
        /// <summary>
        /// Only types that inherit IPermissionConfigurationBase (or the derived interfaces IMemberPermissionConfiguration / the Reusable Schema IXperienceCommunityMemberPermissionConfiguration) will be checked, the rest will be returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> RemoveUnauthorizedItems<T>(IEnumerable<T> items);

        /// <summary>
        /// Only types that inherit IPermissionConfigurationBase (or the derived interfaces IMemberPermissionConfiguration / the Reusable Schema IXperienceCommunityMemberPermissionConfiguration) will be checked, the rest will be returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> RemoveUnauthorizedItems<T>(IEnumerable<T> items, bool userIsAuthenticated, string[] userRoles);
    }
}

using Kentico.Membership;
using Microsoft.AspNetCore.Identity;
using XperienceCommunity.MemberRoles;
using XperienceCommunity.MemberRoles.Models;
using XperienceCommunity.MemberRoles.Repositories;
using XperienceCommunity.MemberRoles.Repositories.Implementation;
using XperienceCommunity.MemberRoles.Services.Implementations;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MemberRolesServiceExtensions
    {
        /// <summary>
        /// Hooks up the XperienceCommunity.MemberRoles stores, interfaces, and installer.
        /// </summary>
        /// <typeparam name="TUser">Must be or inherit the ApplicationUser class</typeparam>
        /// <typeparam name="TRole">Must be or inherit the TagApplicationuserRole class</typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IdentityBuilder AddMemberRolesStores<TUser, TRole>(this IdentityBuilder builder) where TUser : ApplicationUser, new() where TRole : TagApplicationUserRole, new()
        {
            builder.AddRoleStore<MemberRoleRoleStore<TRole>>();

            builder.Services.AddScoped<IMemberAuthenticationContext, MemberAuthenticationContext<TUser>>()
                .AddScoped<IUserLoginStore<TUser>, ApplicationUserStore<TUser>>()
                .AddScoped<IUserRoleStore<TUser>, MemberRoleUserRoleStore<TUser>>()
                .AddScoped<IMemberAuthorizationFilter, MemberAuthorizationFilterAndPermissionSummaryRepository>()
                .AddScoped<IMemberPermissionSummaryRepository, MemberAuthorizationFilterAndPermissionSummaryRepository>()
                .AddSingleton<MemberRolesInstaller>();

            return builder;

        }
    }
}

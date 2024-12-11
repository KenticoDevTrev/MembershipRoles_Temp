using XperienceCommunity.MemberRoles.Admin.ActionComponents;
using XperienceCommunity.MemberRoles.Admin.UIPages;
using XperienceCommunity.MemberRoles.Admin.UIPages.ContentFolderMemberPermission;
using XperienceCommunity.MemberRoles.Models;

namespace XperienceCommunity.MemberRoles.Admin.Extensions
{
    public static class MemberRolesPermissionSummaryClientPropertiesExtensions
    {
        public static MemberRolesPermissionSummaryClientProperties ToClientProperties(this MemberRolePermissionSummary summary, string adminPathPrefixFromIAdminPathRetriever)
        {
            string inheritFrom = summary.InheritanceType switch
            {
                PermissionInheritanceType.None => "No permissions",
                PermissionInheritanceType.ContentItem => $"Content Item (Self)",
                PermissionInheritanceType.WebPageItem => $"Web Page Item{(summary.PermissionIsSelf ? " (Self)" : $" [{summary.InheritedFromPath}]")}",
                PermissionInheritanceType.ContentFolder => $"Content Folder{(summary.PermissionIsSelf ? " (Self)" : $" [{summary.InheritedFromPath}]")}",
                _ => "No Inheritance"
            };
            inheritFrom += summary.OverridesExistOnContentItem ? " + Overrides on Content Item" : "";

            var link = "";
            if (summary.InheritanceType == PermissionInheritanceType.WebPageItem && !summary.PermissionIsSelf && summary.InheritedFromId > 0 && summary.ChannelId > 0)
            {
                link = $"/{adminPathPrefixFromIAdminPathRetriever.Trim('/')}/webpages-{summary.ChannelId}/en_{summary.InheritedFromId}/properties/member-permissions";
            }
            if (summary.InheritanceType == PermissionInheritanceType.ContentFolder && !summary.PermissionIsSelf && summary.InheritedFromId > 0)
            {
                link = $"/{adminPathPrefixFromIAdminPathRetriever.Trim('/')}/{FolderPermissionsApplication.SLUG}/{ContentFolderMemberPermissionListing.SLUG}/{summary.InheritedFromId}/{ContentFolderMemberSecurityPageTemplate.SLUG}";
            }

            return new MemberRolesPermissionSummaryClientProperties(inheritFrom, summary.RequiresAuthentication, summary.MemberRoles, link);
        }
    }
}

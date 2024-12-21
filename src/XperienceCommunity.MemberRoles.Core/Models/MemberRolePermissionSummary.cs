namespace XperienceCommunity.MemberRoles.Models
{
    public record MemberRolePermissionSummary(bool RequiresAuthentication, string[] MemberRoles, PermissionInheritanceType InheritanceType, bool OverridesExistOnContentItem, bool PermissionIsSelf, int InheritedFromId = 0, int ChannelId = 0, string InheritedFromPath = "");

    public enum PermissionInheritanceType
    {
        None,
        ContentItem,
        WebPageItem,
        ContentFolder,
    }

}

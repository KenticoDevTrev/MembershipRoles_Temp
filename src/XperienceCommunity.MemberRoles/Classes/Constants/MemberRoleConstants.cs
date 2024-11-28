using System.Collections.ObjectModel;

namespace XperienceCommunity.MemberRoles
{
    public static class MemberRoleConstants
    {
        public readonly static string _MemberRoleTaxonomy = "MemberRoles";
        public readonly static string _MemberRoleTaxonomyGuid = "3f1d99de-b420-489e-966f-9f4f21d7a088";
        public static readonly ReadOnlyCollection<string> _InheritanceCacheDependencies = new (
          [
            $"{WebPageItemMemberPermissionSettingInfo.OBJECT_TYPE}|all",
            $"{WebPageItemRoleTagInfo.OBJECT_TYPE}|all",
            $"{ContentFolderMemberPermissionSettingInfo.OBJECT_TYPE}|all",
            $"{ContentFolderRoleTagInfo.OBJECT_TYPE}|all"
          ]);
    }
}

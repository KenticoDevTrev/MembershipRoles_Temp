using XperienceCommunity.MemberRoles.Interfaces;

namespace XperienceCommunity.MemberRoles.Models
{
    /// <summary>
    /// Wrapper for a DTO that should have Member Permission Configuration, used in conjuction with the IMemberPermissionConfigurationService that helps quickly convert Content Types that inherit the IXperienceCommunityMemberPermissionConfiguration reusable Schema
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Model"></param>
    /// <param name="MemberPermissionOverride"></param>
    /// <param name="ContentID"></param>
    /// <param name="MemberPermissionIsSecure"></param>
    /// <param name="MemberPermissionRoleTags"></param>
    public record DTOWithMemberPermissionConfiguration<T>(T Model, bool MemberPermissionOverride, int ContentID, bool MemberPermissionIsSecure, string[] MemberPermissionRoleTags) : IMemberPermissionConfiguration
    {
        public bool GetCheckPermissions() => true;

        public int GetContentID() => ContentID;

        public bool GetMemberPermissionIsSecure() => MemberPermissionIsSecure;

        public bool GetMemberPermissionOverride() => MemberPermissionOverride;

        public IEnumerable<string> GetMemberPermissionRoleTags() => MemberPermissionRoleTags;
    }
}

using CMS.DataEngine;

namespace XperienceCommunity.MemberRoles
{
    /// <summary>
    /// Class providing <see cref="ContentFolderMemberPermissionSettingInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IContentFolderMemberPermissionSettingInfoProvider))]
    public partial class ContentFolderMemberPermissionSettingInfoProvider : AbstractInfoProvider<ContentFolderMemberPermissionSettingInfo, ContentFolderMemberPermissionSettingInfoProvider>, IContentFolderMemberPermissionSettingInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentFolderMemberPermissionSettingInfoProvider"/> class.
        /// </summary>
        public ContentFolderMemberPermissionSettingInfoProvider()
            : base(ContentFolderMemberPermissionSettingInfo.TYPEINFO)
        {
        }
    }
}
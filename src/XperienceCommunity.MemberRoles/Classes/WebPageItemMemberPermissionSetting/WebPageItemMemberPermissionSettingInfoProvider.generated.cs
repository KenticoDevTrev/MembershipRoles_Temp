using CMS.DataEngine;

namespace XperienceCommunity.MemberRoles
{
    /// <summary>
    /// Class providing <see cref="WebPageItemMemberPermissionSettingInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IWebPageItemMemberPermissionSettingInfoProvider))]
    public partial class WebPageItemMemberPermissionSettingInfoProvider : AbstractInfoProvider<WebPageItemMemberPermissionSettingInfo, WebPageItemMemberPermissionSettingInfoProvider>, IWebPageItemMemberPermissionSettingInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebPageItemMemberPermissionSettingInfoProvider"/> class.
        /// </summary>
        public WebPageItemMemberPermissionSettingInfoProvider()
            : base(WebPageItemMemberPermissionSettingInfo.TYPEINFO)
        {
        }
    }
}
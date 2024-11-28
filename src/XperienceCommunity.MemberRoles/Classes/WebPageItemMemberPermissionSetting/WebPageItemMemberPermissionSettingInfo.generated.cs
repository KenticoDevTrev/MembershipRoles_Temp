using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using XperienceCommunity.MemberRoles;

[assembly: RegisterObjectType(typeof(WebPageItemMemberPermissionSettingInfo), WebPageItemMemberPermissionSettingInfo.OBJECT_TYPE)]

namespace XperienceCommunity.MemberRoles
{
    /// <summary>
    /// Data container class for <see cref="WebPageItemMemberPermissionSettingInfo"/>.
    /// </summary>
    public partial class WebPageItemMemberPermissionSettingInfo : AbstractInfo<WebPageItemMemberPermissionSettingInfo, IWebPageItemMemberPermissionSettingInfoProvider>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "xperiencecommunity.webpageitemmemberpermissionsetting";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WebPageItemMemberPermissionSettingInfoProvider), OBJECT_TYPE, "XperienceCommunity.WebPageItemMemberPermissionSetting", "WebPageItemMemberPermissionSettingID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("WebPageItemMemberPermissionSettingWebPageItemID", "cms.webpageitem", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Web page item member permission setting ID.
        /// </summary>
        [DatabaseField]
        public virtual int WebPageItemMemberPermissionSettingID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(WebPageItemMemberPermissionSettingID)), 0);
            set => SetValue(nameof(WebPageItemMemberPermissionSettingID), value);
        }


        /// <summary>
        /// Web page item member permission setting Web Page Item ID.
        /// </summary>
        [DatabaseField]
        public virtual int WebPageItemMemberPermissionSettingWebPageItemID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(WebPageItemMemberPermissionSettingWebPageItemID)), 0);
            set => SetValue(nameof(WebPageItemMemberPermissionSettingWebPageItemID), value);
        }


        /// <summary>
        /// Web page item member permission setting break inheritance.
        /// </summary>
        [DatabaseField]
        public virtual bool WebPageItemMemberPermissionSettingBreakInheritance
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(WebPageItemMemberPermissionSettingBreakInheritance)), false);
            set => SetValue(nameof(WebPageItemMemberPermissionSettingBreakInheritance), value);
        }


        /// <summary>
        /// Web page item member permission setting is secured.
        /// </summary>
        [DatabaseField]
        public virtual bool WebPageItemMemberPermissionSettingIsSecured
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(WebPageItemMemberPermissionSettingIsSecured)), false);
            set => SetValue(nameof(WebPageItemMemberPermissionSettingIsSecured), value);
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            Provider.Delete(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            Provider.Set(this);
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="WebPageItemMemberPermissionSettingInfo"/> class.
        /// </summary>
        public WebPageItemMemberPermissionSettingInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="WebPageItemMemberPermissionSettingInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public WebPageItemMemberPermissionSettingInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}
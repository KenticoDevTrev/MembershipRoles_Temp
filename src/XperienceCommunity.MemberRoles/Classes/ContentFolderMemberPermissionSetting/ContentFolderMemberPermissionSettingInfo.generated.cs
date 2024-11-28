using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using XperienceCommunity.MemberRoles;

[assembly: RegisterObjectType(typeof(ContentFolderMemberPermissionSettingInfo), ContentFolderMemberPermissionSettingInfo.OBJECT_TYPE)]

namespace XperienceCommunity.MemberRoles
{
    /// <summary>
    /// Data container class for <see cref="ContentFolderMemberPermissionSettingInfo"/>.
    /// </summary>
    public partial class ContentFolderMemberPermissionSettingInfo : AbstractInfo<ContentFolderMemberPermissionSettingInfo, IContentFolderMemberPermissionSettingInfoProvider>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "xperiencecommunity.contentfoldermemberpermissionsetting";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ContentFolderMemberPermissionSettingInfoProvider), OBJECT_TYPE, "XperienceCommunity.ContentFolderMemberPermissionSetting", "ContentFolderMemberPermissionSettingID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("ContentFolderMemberPermissionContentFolderID", "cms.contentfolder", ObjectDependencyEnum.Required),
            },
            LogEvents = true,
            MacroSettings = {
                ContainsMacros = false
            }
        };


        /// <summary>
        /// Content folder member permission setting ID.
        /// </summary>
        [DatabaseField]
        public virtual int ContentFolderMemberPermissionSettingID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ContentFolderMemberPermissionSettingID)), 0);
            set => SetValue(nameof(ContentFolderMemberPermissionSettingID), value);
        }


        /// <summary>
        /// Content folder member permission content folder ID.
        /// </summary>
        [DatabaseField]
        public virtual int ContentFolderMemberPermissionContentFolderID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ContentFolderMemberPermissionContentFolderID)), 0);
            set => SetValue(nameof(ContentFolderMemberPermissionContentFolderID), value);
        }


        /// <summary>
        /// Content folder member permission setting break inheritance.
        /// </summary>
        [DatabaseField]
        public virtual bool ContentFolderMemberPermissionSettingBreakInheritance
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(ContentFolderMemberPermissionSettingBreakInheritance)), false);
            set => SetValue(nameof(ContentFolderMemberPermissionSettingBreakInheritance), value);
        }


        /// <summary>
        /// Content folder member permission setting is secured.
        /// </summary>
        [DatabaseField]
        public virtual bool ContentFolderMemberPermissionSettingIsSecured
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(ContentFolderMemberPermissionSettingIsSecured)), false);
            set => SetValue(nameof(ContentFolderMemberPermissionSettingIsSecured), value);
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
        /// Creates an empty instance of the <see cref="ContentFolderMemberPermissionSettingInfo"/> class.
        /// </summary>
        public ContentFolderMemberPermissionSettingInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ContentFolderMemberPermissionSettingInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ContentFolderMemberPermissionSettingInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}
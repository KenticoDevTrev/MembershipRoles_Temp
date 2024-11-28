using System;
using System.Data;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using XperienceCommunity.MemberRoles;

[assembly: RegisterObjectType(typeof(ContentFolderRoleTagInfo), ContentFolderRoleTagInfo.OBJECT_TYPE)]

namespace XperienceCommunity.MemberRoles
{
    /// <summary>
    /// Data container class for <see cref="ContentFolderRoleTagInfo"/>.
    /// </summary>
    public partial class ContentFolderRoleTagInfo : AbstractInfo<ContentFolderRoleTagInfo, IContentFolderRoleTagInfoProvider>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "xperiencecommunity.contentfolderroletag";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ContentFolderRoleTagInfoProvider), OBJECT_TYPE, "XperienceCommunity.ContentFolderRoleTag", "ContentFolderRoleTagID", null, null, null, null, null, "ContentFolderRoleTagContentFolderID", "cms.contentfolder")
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("ContentFolderRoleTagTagID", "cms.tag", ObjectDependencyEnum.Binding),
            },
            LogEvents = true,
            IsBinding = true,
            MacroSettings = {
                ContainsMacros = false
            }
        };


        /// <summary>
        /// Content folder role tag ID
        /// </summary>
        [DatabaseField]
        public virtual int ContentFolderRoleTagID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ContentFolderRoleTagID"), 0);
            }
            set
            {
                SetValue("ContentFolderRoleTagID", value);
            }
        }


        /// <summary>
        /// Content folder role tag content folder ID
        /// </summary>
        [DatabaseField]
        public virtual int ContentFolderRoleTagContentFolderID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ContentFolderRoleTagContentFolderID"), 0);
            }
            set
            {
                SetValue("ContentFolderRoleTagContentFolderID", value);
            }
        }


        /// <summary>
        /// Content folder role tag tag ID
        /// </summary>
        [DatabaseField]
        public virtual int ContentFolderRoleTagTagID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ContentFolderRoleTagTagID"), 0);
            }
            set
            {
                SetValue("ContentFolderRoleTagTagID", value);
            }
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
        /// Creates an empty instance of the <see cref="ContentFolderRoleTagInfo"/> class.
        /// </summary>
        public ContentFolderRoleTagInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instance of the <see cref="ContentFolderRoleTagInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ContentFolderRoleTagInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}
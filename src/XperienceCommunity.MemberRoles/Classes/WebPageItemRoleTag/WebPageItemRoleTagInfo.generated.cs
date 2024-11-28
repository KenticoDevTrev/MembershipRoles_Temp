using System;
using System.Data;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using XperienceCommunity.MemberRoles;

[assembly: RegisterObjectType(typeof(WebPageItemRoleTagInfo), WebPageItemRoleTagInfo.OBJECT_TYPE)]

namespace XperienceCommunity.MemberRoles
{
    /// <summary>
    /// Data container class for <see cref="WebPageItemRoleTagInfo"/>.
    /// </summary>
    public partial class WebPageItemRoleTagInfo : AbstractInfo<WebPageItemRoleTagInfo, IWebPageItemRoleTagInfoProvider>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "xperiencecommunity.webpageitemroletag";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WebPageItemRoleTagInfoProvider), OBJECT_TYPE, "XperienceCommunity.WebPageItemRoleTag", "WebPageItemRoleTagID", null, null, null, null, null, "WebPageItemRoleTagWebPageItemID", "cms.webpageitem")
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("WebPageItemRoleTagTagID", "cms.tag", ObjectDependencyEnum.Binding),
            },
            LogEvents = true,
            IsBinding = true,
            MacroSettings = {
                ContainsMacros = false
            }
        };


        /// <summary>
        /// Web page item role tag ID
        /// </summary>
        [DatabaseField]
        public virtual int WebPageItemRoleTagID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("WebPageItemRoleTagID"), 0);
            }
            set
            {
                SetValue("WebPageItemRoleTagID", value);
            }
        }


        /// <summary>
        /// Web page item role tag web page item ID
        /// </summary>
        [DatabaseField]
        public virtual int WebPageItemRoleTagWebPageItemID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("WebPageItemRoleTagWebPageItemID"), 0);
            }
            set
            {
                SetValue("WebPageItemRoleTagWebPageItemID", value);
            }
        }


        /// <summary>
        /// Web page item role tag tag ID
        /// </summary>
        [DatabaseField]
        public virtual int WebPageItemRoleTagTagID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("WebPageItemRoleTagTagID"), 0);
            }
            set
            {
                SetValue("WebPageItemRoleTagTagID", value);
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
        /// Creates an empty instance of the <see cref="WebPageItemRoleTagInfo"/> class.
        /// </summary>
        public WebPageItemRoleTagInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instance of the <see cref="WebPageItemRoleTagInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public WebPageItemRoleTagInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}
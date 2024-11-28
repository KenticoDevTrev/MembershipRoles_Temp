using System;
using System.Data;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using XperienceCommunity.MemberRoles;

[assembly: RegisterObjectType(typeof(MemberRoleTagInfo), MemberRoleTagInfo.OBJECT_TYPE)]

namespace XperienceCommunity.MemberRoles
{
    /// <summary>
    /// Data container class for <see cref="MemberRoleTagInfo"/>.
    /// </summary>
    public partial class MemberRoleTagInfo : AbstractInfo<MemberRoleTagInfo, IMemberRoleTagInfoProvider>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "xperiencecommunity.memberroletag";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MemberRoleTagInfoProvider), OBJECT_TYPE, "XperienceCommunity.MemberRoleTag", "MemberRoleTagID", null, null, null, null, null, "MemberRoleTagMemberID", "cms.member")
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("MemberRoleTagTagID", "cms.tag", ObjectDependencyEnum.Binding),
            },
            LogEvents = false,
            IsBinding = true,
            MacroSettings = {
                ContainsMacros = false
            }
        };


        /// <summary>
        /// Member role tag ID
        /// </summary>
        [DatabaseField]
        public virtual int MemberRoleTagID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MemberRoleTagID"), 0);
            }
            set
            {
                SetValue("MemberRoleTagID", value);
            }
        }


        /// <summary>
        /// Member role tag member ID
        /// </summary>
        [DatabaseField]
        public virtual int MemberRoleTagMemberID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MemberRoleTagMemberID"), 0);
            }
            set
            {
                SetValue("MemberRoleTagMemberID", value);
            }
        }


        /// <summary>
        /// Member role tag tag ID
        /// </summary>
        [DatabaseField]
        public virtual int MemberRoleTagTagID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MemberRoleTagTagID"), 0);
            }
            set
            {
                SetValue("MemberRoleTagTagID", value);
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
        /// Creates an empty instance of the <see cref="MemberRoleTagInfo"/> class.
        /// </summary>
        public MemberRoleTagInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instance of the <see cref="MemberRoleTagInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public MemberRoleTagInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}
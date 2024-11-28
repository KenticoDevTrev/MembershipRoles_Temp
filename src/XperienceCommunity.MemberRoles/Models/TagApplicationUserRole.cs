using CMS.ContentEngine;
using Microsoft.AspNetCore.Identity;

namespace XperienceCommunity.MemberRoles.Models
{
    public class TagApplicationUserRole : IdentityRole<int>
    {
        public virtual void SetRoleFromTag<TRole>(TagInfo tag) where TRole : TagApplicationUserRole, new()
        {
            Id = tag.TagID;
            Name = tag.TagName;
            NormalizedName = tag.TagName.Normalize();
            ConcurrencyStamp = tag.TagLastModified.ToString();
        }
    }
}

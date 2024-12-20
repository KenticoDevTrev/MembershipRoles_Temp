﻿using CMS.ContentEngine;

namespace CMS.ContentEngine
{
    public static class MemberRolesContentItemQueryExtensions
    {

        /// <summary>
        /// ONLY CALL if you are already filtering columns!!!
        /// 
        /// Ensures the fields needed to do the Membership Authorization logic are returned.  Call IMembershipAuthorizationFilter.RemoveUnauthorizedItems(IEnumerable<IContentQueryDataContainer> retrievedItems) to then remove items the current user is not authorized for.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ContentTypeQueryParameters IncludeMemberAuthorization(this ContentTypeQueryParameters builder)
        {
            return builder.Columns(
                nameof(ContentItemFields.ContentItemID),
                nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionOverride), nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionIsSecure), nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionRoleTags));
        }
    }
}

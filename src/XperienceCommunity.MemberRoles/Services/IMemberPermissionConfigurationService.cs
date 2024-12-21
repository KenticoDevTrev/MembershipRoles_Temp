using CMS.ContentEngine;
using XperienceCommunity.MemberRoles.Models;

namespace XperienceCommunity.MemberRoles.Services
{
    public interface IMemberPermissionConfigurationService
    {
        /// <summary>
        /// Wraps the Content Type model with the given DTO in a model that can be used with the IMemberAuthorizationFilter to filter items out.
        /// 
        /// Use this to wrap the DTO given your ContentItem that inherits IXperienceCommunityMemberPermissionConfiguration, then pass these to the IMemberAuthorizationFilter.RemoveUnauthorizedItems(items), lastly then select the filtered item's Model fields to retrieve your items.
        /// </summary>
        /// <typeparam name="TDtoType">The Generic DTO Type</typeparam>
        /// <typeparam name="TContentType">The Content Item Type that inherits the IContentItemFieldSource and IXperienceCommunityMemberPermissionConfiguration</typeparam>
        /// <param name="DtoModel">The Generic DTO Object</param>
        /// <param name="contentItem">The Content Item</param>
        /// <returns>The wrapped and filterable item</returns>
        DTOWithMemberPermissionConfiguration<TDtoType> WrapGenericDTOWithMemberPermissions<TDtoType, TContentType>(TDtoType DtoModel, TContentType contentItem) where TContentType : IContentItemFieldsSource, IXperienceCommunityMemberPermissionConfiguration;
    }
}

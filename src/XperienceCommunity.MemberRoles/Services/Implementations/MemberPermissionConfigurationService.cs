using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using XperienceCommunity.MemberRoles.Models;

namespace XperienceCommunity.MemberRoles.Services.Implementations
{
    public class MemberPermissionConfigurationService(
        IProgressiveCache progressiveCache,
        IInfoProvider<TagInfo> tagInfoProvider
        ) : IMemberPermissionConfigurationService
    {
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IInfoProvider<TagInfo> _tagInfoProvider = tagInfoProvider;

        public DTOWithMemberPermissionConfiguration<TDtoType> WrapGenericDTOWithMemberPermissions<TDtoType, TContentType>(TDtoType DtoModel, TContentType contentItem) where TContentType : IContentItemFieldsSource, IXperienceCommunityMemberPermissionConfiguration
        {
            var tagGuidToName = GetTagGuidToTagName();
            return new DTOWithMemberPermissionConfiguration<TDtoType>(DtoModel, contentItem.MemberPermissionOverride, contentItem.SystemFields.ContentItemID, contentItem.MemberPermissionIsSecure, contentItem.MemberPermissionRoleTags.Select(x => x.Identifier).Intersect(tagGuidToName.Keys).Select(x => tagGuidToName[x]).ToArray());
        }

        private Dictionary<Guid, string> GetTagGuidToTagName()
        {
            // Shouldn't be concerned if this is called within another cached method, because the objects you are retrieving would need to 'change' to add a new tag's identity anyway.
            return _progressiveCache.Load(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{TagInfo.OBJECT_TYPE}|all");
                }

                return _tagInfoProvider.Get()
                .Columns(nameof(TagInfo.TagGUID), nameof(TagInfo.TagName))
                .GetEnumerableTypedResult()
                .ToDictionary(key => key.TagGUID, value => value.TagName);
            }, new CacheSettings(1440, "XperienceCommunity.MembershipRoles_GetTagGuidToTagName"));
        }
    }
}

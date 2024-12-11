using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using System.Data;
using System.Data.Common;
using System.Text.Json;
using XperienceCommunity.MemberRoles.Models;

namespace XperienceCommunity.MemberRoles.Repositories.Implementation
{
    public class MemberAuthorizationFilterAndPermissionSummaryRepository(IMemberAuthenticationContext memberAuthenticationContext,
        IProgressiveCache progressiveCache,
        IInfoProvider<TagInfo> tagInfoProvider,
        IContentQueryExecutor contentQueryExecutor) : IMemberAuthorizationFilter, IMemberPermissionSummaryRepository
    {
        private readonly IMemberAuthenticationContext _memberAuthenticationContext = memberAuthenticationContext;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IInfoProvider<TagInfo> _tagInfoProvider = tagInfoProvider;
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;

        public async Task<IEnumerable<T>> RemoveUnauthorizedItems<T>(IEnumerable<T> items)
        {
            var currentMember = await _memberAuthenticationContext.GetAuthenticationContext();
            return await RemoveUnauthorizedItemsInternal(items, currentMember.IsAuthenticated, currentMember.Roles);
        }

        public Task<IEnumerable<T>> RemoveUnauthorizedItems<T>(IEnumerable<T> items, bool userIsAuthenticated, string[] userRoles) => RemoveUnauthorizedItemsInternal(items, userIsAuthenticated, userRoles);

        public async Task<IEnumerable<T>> RemoveUnauthorizedItemsInternal<T>(IEnumerable<T> items, bool userIsAuthenticated, string[] userRoles)
        {
            List<int> contentIdsToCheck = [];

            // First pass, gather contentItemIds of things we need to run a check on.
            foreach (var item in items) {
                if (item is IXperienceCommunityMemberPermissionConfiguration reusableShemaConfiguration 
                    && !reusableShemaConfiguration.MemberPermissionOverride
                    && item is IContentItemFieldsSource fieldSource
                    && fieldSource.SystemFields.ContentItemID > 0) {
                    contentIdsToCheck.Add(fieldSource.SystemFields.ContentItemID);
                } else if(item is IMemberPermissionConfiguration customConfiguration
                    && customConfiguration.GetCheckPermissions()
                    && !customConfiguration.GetMemberPermissionOverride()
                    && customConfiguration.GetContentID() > 0) {
                    contentIdsToCheck.Add(customConfiguration.GetContentID());
                }
            }

            // Convert the ContentIDs to an Authorization Summary
            var contentIdToAuthorizationSummary = await ContentIDToAuthorizationSummary(contentIdsToCheck);

            var roleGuidToName = await GetTagGuidToRoleName();

            // Gather the proper Requires Authentication and Tags for these content items into a dictionary
            var test =  items.Where(item => {
                if (item is IPermissionConfigurationBase baseConfig) {
                    var authSummary = new MemberAuthorizationSummary(false, []);
                    var contentId = 0;

                    if(item is IXperienceCommunityMemberPermissionConfiguration reusableSchemaConfig 
                        && item is IContentItemFieldsSource fieldSource) {
                        contentId = fieldSource.SystemFields.ContentItemID;
                        if (reusableSchemaConfig.MemberPermissionOverride) {
                            authSummary = new MemberAuthorizationSummary(reusableSchemaConfig.MemberPermissionIsSecure, reusableSchemaConfig.MemberPermissionRoleTags.Select(x => roleGuidToName.GetValueOrDefault(x.Identifier, string.Empty)).Where(x => !string.IsNullOrWhiteSpace(x)));
                        } else if(contentIdToAuthorizationSummary.TryGetValue(contentId, out var summary)) {
                            authSummary = summary;
                        }
                    } else if(item is IMemberPermissionConfiguration customConfig
                        && customConfig.GetCheckPermissions()) {
                        contentId = customConfig.GetContentID();
                        if (!customConfig.GetMemberPermissionOverride()) { 
                            authSummary = new MemberAuthorizationSummary(customConfig.GetMemberPermissionIsSecure(), customConfig.GetMemberPermissionRoleTags().Select(x => x.ToLowerInvariant()));
                        } else if(contentIdToAuthorizationSummary.TryGetValue(contentId, out var summary)) {
                            authSummary = summary;
                        }
                    }

                    // If doesn't require authentication
                    if (!authSummary.RequiresAuthentication) {
                        return true;
                    }

                    // Authentication required at this point
                    if (!userIsAuthenticated) {
                        return false;
                    }

                    // No roles or User in at least one role
                    if (!authSummary.Roles.Any()
                        || authSummary.Roles.Intersect(userRoles).Any()
                       ) {
                        return true;
                    }

                    return false;
                }

                // Not an item to check for authorization
                return true;
            });
            return test;
        }

        private async Task<Dictionary<Guid, string>> GetTagGuidToRoleName()
        {
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{TagInfo.OBJECT_TYPE}|all");
                }
                return (await _tagInfoProvider.Get()
                .Source(x => x.InnerJoin<TaxonomyInfo>(nameof(TagInfo.TagTaxonomyID), nameof(TaxonomyInfo.TaxonomyID)))
                .WhereEquals(nameof(TaxonomyInfo.TaxonomyName), MemberRoleConstants._MemberRoleTaxonomy)
                .Columns(nameof(TagInfo.TagName), nameof(TagInfo.TagGUID))
                .GetEnumerableTypedResultAsync())
                .ToDictionary(key => key.TagGUID, value => value.TagName.ToLowerInvariant());
            }, new CacheSettings(1440, "GetTagGuidToRoleName"));
        }

        private async Task<Dictionary<int, string>> GetTagIdToRoleName()
        {
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{TagInfo.OBJECT_TYPE}|all");
                }
                return (await _tagInfoProvider.Get()
                .Source(x => x.InnerJoin<TaxonomyInfo>(nameof(TagInfo.TagTaxonomyID), nameof(TaxonomyInfo.TaxonomyID)))
                .WhereEquals(nameof(TaxonomyInfo.TaxonomyName), MemberRoleConstants._MemberRoleTaxonomy)
                .Columns(nameof(TagInfo.TagID), nameof(TagInfo.TagName))
                .GetEnumerableTypedResultAsync())
                .ToDictionary(key => key.TagID, value => value.TagName.ToLowerInvariant());
            }, new CacheSettings(1440, "GetTagIdToRoleName"));
        }

        private async Task<Dictionary<int, MemberAuthorizationSummary>> ContentIDToAuthorizationSummary(IEnumerable<int> contentIds)
        {
            if (!contentIds.Any()) {
                return [];
            }

            var webPageMaxDepth = await GetWebPageItemMaxDepth();
            var folderMaxDepth = await GetContentFolderMaxDepth();
            var tagIdToRoleName = await GetTagIdToRoleName();

            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    // If a lot of content items, just do "all" to avoid too many cache dependencies on this check
                    var pageDependencies = contentIds.Count() > 25 ? ["contentitem|all"] : contentIds.Select(x => $"contentitem|byid|{x}");

                    cs.CacheDependency = CacheHelper.GetCacheDependency(pageDependencies.Union([
                        $"{WebPageItemMemberPermissionSettingInfo.OBJECT_TYPE}|all",
                        $"{WebPageItemRoleTagInfo.OBJECT_TYPE}|all",
                        $"{ContentFolderMemberPermissionSettingInfo.OBJECT_TYPE}|all",
                        $"{ContentFolderRoleTagInfo.OBJECT_TYPE}|all",
                        $"{TagInfo.OBJECT_TYPE}|all" // So it clears if tags are added or removed as well
                    ]).ToArray());
                }

                var webPagePermissionSettingCoalesceItems = new List<string>();
                var webPageTableJoinItems = new List<string>();
                for (var i = 1; i <= webPageMaxDepth; i++) {
                    webPagePermissionSettingCoalesceItems.Add($"MPS{i}.WebPageItemMemberPermissionSettingID");
                    webPageTableJoinItems.Add($"left join CMS_WebPageItem as WP{i} on WP{i - 1}.WebPageItemParentID = WP{i}.WebPageItemID left join XperienceCommunity_WebPageItemMemberPermissionSetting as MPS{i} on WP{i}.WebPageItemID = MPS{i}.WebPageItemMemberPermissionSettingWebPageItemID and MPS{1}.WebPageItemMemberPermissionSettingBreakInheritance = 1");
                }
                // First join is different format
                webPageTableJoinItems.RemoveAt(0);

                var folderPermissionSettingCoalesceItems = new List<string>();
                var folderTableJoinItems = new List<string>();
                for (var i = 1; i <= folderMaxDepth; i++) {
                    folderPermissionSettingCoalesceItems.Add($"CFS{i}.ContentFolderMemberPermissionSettingID");
                    folderTableJoinItems.Add($"left join CMS_ContentFolder as CF{i} on CF{i - 1}.ContentFolderParentFolderID = CF{i}.ContentFolderID left join XperienceCommunity_ContentFolderMemberPermissionSetting as CFS{i} on CF{i}.ContentFolderID = CFS{i}.ContentFolderMemberPermissionContentFolderID and CFS{1}.ContentFolderMemberPermissionSettingBreakInheritance = 1");
                }
                // First join is different format
                folderTableJoinItems.RemoveAt(0);


                // Generates the SQL to find the proper inherited permission for the given content items
                // Does joins on parents for WebPageItems and ContentFolders, with logic to handle if Breaking Inheritance or not, and the roles for the items.
                // Each row is only an int, bit, and int, so should be fast even for large quantities of data
                var permissionQuery =
    $@"
Select 
InheritedMemberPermissionContentItemID,
InheritedMemberPermissionRequiresAuthentication,
InheritedMemberPermissionRoleTagID

from 
(
	select 
	ContentItemToSecuritySettings.ContentItemID as InheritedMemberPermissionContentItemID,
	ContentItemToSecuritySettings.RequiresAuthentication as InheritedMemberPermissionRequiresAuthentication,
	XperienceCommunity_WebPageItemRoleTag.WebPageItemRoleTagTagID as InheritedMemberPermissionRoleTagID
	 from (

		select WebPageItemContentItemID as ContentItemID, 
		case when WebPageItemMemberPermissionSettingBreakInheritance is null then null else WebPageItemMemberPermissionSettingWebPageItemID end as WebPageItemIDTaxonomyCheck,
		COALESCE(WebPageItemMemberPermissionSettingIsSecured, 0) as RequiresAuthentication

		from 
		(
			select WP1.WebPageItemContentItemID,
			COALESCE(null, {string.Join(",", webPagePermissionSettingCoalesceItems)}) as MemberPermissionID
			from CMS_WebPageItem as WP1 left join XperienceCommunity_WebPageItemMemberPermissionSetting as MPS1 on WP1.WebPageItemID = MPS1.WebPageItemMemberPermissionSettingWebPageItemID and MPS1.WebPageItemMemberPermissionSettingBreakInheritance = 1
			{string.Join(" ", webPageTableJoinItems)}
			where WP1.WebPageItemContentItemID in ({string.Join(',', contentIds)})
		) WebPageToPermission
		left join XperienceCommunity_WebPageItemMemberPermissionSetting on WebPageToPermission.MemberPermissionID = WebPageItemMemberPermissionSettingID
	) ContentItemToSecuritySettings
	left join XperienceCommunity_WebPageItemRoleTag on WebPageItemIDTaxonomyCheck = WebPageItemRoleTagWebPageItemID

	UNION ALL

	select 
	ContentItemID as InheritedMemberPermissionContentItemID,
	ContentItemToSecuritySettings.RequiresAuthentication as InheritedMemberPermissionRequiresAuthentication,
	XperienceCommunity_ContentFolderRoleTag.ContentFolderRoleTagTagID as InheritedMemberPermissionRoleTagID
	 from (

		select ContentFolderID as ContentFolderID, 
		case when ContentFolderMemberPermissionSettingBreakInheritance is null then null else ContentFolderMemberPermissionContentFolderID end as ContentFolderIDTaxonomyCheck,
		COALESCE(ContentFolderMemberPermissionSettingBreakInheritance, 1) as BreakInheritance,
		COALESCE(ContentFolderMemberPermissionSettingIsSecured, 0) as RequiresAuthentication

		from 
		(
			Select CF1.ContentFolderID,
			COALESCE(null, {string.Join(",", folderPermissionSettingCoalesceItems)}
			) as MemberPermissionID
			from CMS_ContentItem
			inner join CMS_ContentFolder as CF1 on CF1.ContentFolderID = ContentItemContentFolderID	left join XperienceCommunity_ContentFolderMemberPermissionSetting as CFS1 on CF1.ContentFolderID = CFS1.ContentFolderMemberPermissionContentFolderID and CFS1.ContentFolderMemberPermissionSettingBreakInheritance = 1
	        {string.Join(" ", folderTableJoinItems)}
			where ContentItemID in ({string.Join(',', contentIds)})
		) ContentFolderToPermission
		left join XperienceCommunity_ContentFolderMemberPermissionSetting on ContentFolderToPermission.MemberPermissionID = ContentFolderMemberPermissionSettingID
	) ContentItemToSecuritySettings
	left join XperienceCommunity_ContentFolderRoleTag on ContentFolderIDTaxonomyCheck = ContentFolderRoleTagContentFolderID
	inner join CMS_ContentItem on ContentItemContentFolderID = ContentItemToSecuritySettings.ContentFolderID
) Combined
";
                return (await ExecuteQueryAsync(permissionQuery, [], QueryTypeEnum.SQLQuery))
                    .Tables[0].Rows.Cast<DataRow>()
                    .GroupBy(x => (int)x["InheritedMemberPermissionContentItemID"])
                    .ToDictionary(key => key.Key, value => new MemberAuthorizationSummary(
                        RequiresAuthentication: ValidationHelper.GetBoolean(value.First()["InheritedMemberPermissionRequiresAuthentication"], false),
                        Roles: value.Where(x => tagIdToRoleName.ContainsKey(ValidationHelper.GetInteger(x["InheritedMemberPermissionRoleTagID"], 0))).Select(x => tagIdToRoleName[ValidationHelper.GetInteger(x["InheritedMemberPermissionRoleTagID"], 0)].ToLowerInvariant())
                        )
                    );
                // Doing a hash code just in case large amounts of IDs are passed in, so the Cache Name won't be gigantic.
            }, new CacheSettings(15, "ContentIDToAuthorizationSummary", webPageMaxDepth, folderMaxDepth, contentIds.Count(), string.Join("-", contentIds).GetHashCode()));
        }


        private async Task<int> GetContentFolderMaxDepth()
        {
            var depth = await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{ContentFolderInfo.OBJECT_TYPE}|all");
                }

                var query = @"select max(Depth) as MaxDepth from (select LEN(ContentFolderTreePath) - LEN(REPLACE(ContentFolderTreePath, '/', '')) as Depth  from CMS_ContentFolder) DepthQuery";
                return (int)(await ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery)).Tables[0].Rows[0]["MaxDepth"];
            }, new CacheSettings(1440, "GetContentFolderMaxDepth"));
            return depth < 1 ? 1 : depth;
        }

        private async Task<int> GetWebPageItemMaxDepth()
        {
            var depth = await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{ContentFolderInfo.OBJECT_TYPE}|all");
                }

                var query = @"select max(Depth) as MaxDepth from (select LEN(WebPageItemTreePath) - LEN(REPLACE(WebPageItemTreePath, '/', '')) as Depth  from CMS_WebPageItem) DepthQuery";
                return (int)(await ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery)).Tables[0].Rows[0]["MaxDepth"];
            }, new CacheSettings(1440, "GetWebPageItemMaxDepth"));
            return depth < 1 ? 1 : depth;
        }

        // Didn't want to have a dependency on XperienceCommunity.DevTools.QueryExtensions, i really wish Kentico added this...
        public static async Task<DataSet> ExecuteQueryAsync(string queryText, QueryDataParameters parameters, QueryTypeEnum queryType, CancellationToken token = default)
        {
            var reader = await ConnectionHelper.ExecuteReaderAsync(queryText, parameters, queryType, CommandBehavior.Default, token);
            return DbDataReaderToDataSet(reader);
        }

        private static DataSet DbDataReaderToDataSet(DbDataReader reader)
        {
            var ds = new DataSet();
            if (reader is null) {
                ds.Tables.Add(new DataTable());
                return ds;
            }

            // read each data result into a datatable
            do {
                var table = new DataTable();
                table.Load(reader);
                ds.Tables.Add(table);
            } while (!reader.IsClosed);

            return ds;
        }

        public async Task<MemberRolePermissionSummary> GetMemberRolePermissionSummaryByContentItem(int contentItemId, string language)
        {

            // do non-cached lookup since caching was messing with preview
            var summaryLookup = await RetrieveContentItemSummaryLookupForContentItemCheck(contentItemId, language);

            if (summaryLookup.Summary != null) {
                return summaryLookup.Summary;
            } else if(summaryLookup.WebPageItemID.HasValue) {
                var permission = await GetMemberRolePermissionSummaryByWebPageItem(summaryLookup.WebPageItemID.Value);
                return permission with { PermissionIsSelf = false, OverridesExistOnContentItem = false };
            } else if(summaryLookup.ContentItemFolderID.HasValue) {
                var permission = await GetMemberRolePermissionSummaryByContentFolder(summaryLookup.ContentItemFolderID.Value);
                return permission with { PermissionIsSelf = false, OverridesExistOnContentItem = false };
            }
            return new MemberRolePermissionSummary(false, [], PermissionInheritanceType.None, false, false);
        }

        private async Task<ContentItemSummaryLookup> RetrieveContentItemSummaryLookupForContentItemCheck(int contentItemId, string language)
        {
            var tagGuidToRoleName = await GetTagGuidToRoleName();

            // First get the individual item and see if it's overwritten.
            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentTypes(query => query.OfReusableSchema(IXperienceCommunityMemberPermissionConfiguration.REUSABLE_FIELD_SCHEMA_NAME))
                .Parameters(query => query.Where(where => where.WhereEquals(nameof(ContentItemFields.ContentItemID), contentItemId)))
                .InLanguage(language);

            var item = (await _contentQueryExecutor.GetResult(queryBuilder, options: new ContentQueryExecutionOptions() {
                ForPreview = true,
                IncludeSecuredItems = true
            }, resultSelector: (dataContainer) => {
                try {
                    var baseConfiguration = new BaseXperienceCommunityMemberPermissionConfiguration() {
                        MemberPermissionOverride = dataContainer.TryGetValue<bool>(nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionOverride), out var permissionOverride) && permissionOverride,
                        MemberPermissionIsSecure = dataContainer.TryGetValue<bool>(nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionIsSecure), out var isSecure) && isSecure,
                    };

                    var tagsString = dataContainer.TryGetValue<string>(nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionRoleTags), out var roleTagsVal) ? roleTagsVal : string.Empty;
                    if (!string.IsNullOrWhiteSpace(tagsString)) {
                        var tags = JsonSerializer.Deserialize<IEnumerable<TagReference>>(tagsString);
                        if (tags != null && tags.Any()) {
                            baseConfiguration.MemberPermissionRoleTags = tags;
                        }
                    }
                    return baseConfiguration;
                } catch (NullReferenceException) {
                    // BUG: the TryGetValue is throwing an exception when the content item doesn't have the reusable schema values yet.
                    return new BaseXperienceCommunityMemberPermissionConfiguration();
                }
            })).FirstOrDefault();

            if (item == null) {
                // shouldn't happen but handling
                return new ContentItemSummaryLookup(Summary: new MemberRolePermissionSummary(false, [], PermissionInheritanceType.None, false, false));
            }

            // From Self
            if (item.MemberPermissionOverride) {
                return new ContentItemSummaryLookup(Summary: new MemberRolePermissionSummary(item.MemberPermissionIsSecure, item.MemberPermissionRoleTags.Where(x => tagGuidToRoleName.ContainsKey(x.Identifier)).Select(x => tagGuidToRoleName[x.Identifier]).ToArray(), PermissionInheritanceType.ContentItem, false, true));
            }

            // Now should look for inheritance
            var query = $@"select WebPageItemID, ContentItemContentFolderID from CMS_ContentItem 
left join CMS_WebPageItem on WebPageItemContentItemID = ContentItemID
where ContentItemID = {contentItemId}
";
            var result = (await ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery)).Tables[0].Rows[0];
            var webPageItemID = ValidationHelper.GetInteger(result["WebPageItemID"], 0);
            var contentItemFolderId = ValidationHelper.GetInteger(result["ContentItemContentFolderID"], 0);
            return new ContentItemSummaryLookup(WebPageItemID: webPageItemID > 0 ? webPageItemID : (int?)null, ContentItemFolderID: contentItemFolderId > 0 ? contentItemFolderId : (int?)null);
        }

        public async Task<MemberRolePermissionSummary> GetMemberRolePermissionSummaryByContentFolder(int contentFolderId)
        {
            var folderMaxDepth = await GetContentFolderMaxDepth();
            var tagIdToRoleName = await GetTagIdToRoleName();

            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency([
                        $"{ContentFolderMemberPermissionSettingInfo.OBJECT_TYPE}|all",
                        $"{ContentFolderRoleTagInfo.OBJECT_TYPE}|all",
                        $"{TagInfo.OBJECT_TYPE}|all" // So it clears if tags are added or removed as well
                    ]);
                }

                var folderPermissionSettingCoalesceItems = new List<string>();
                var folderTableJoinItems = new List<string>();
                for (var i = 1; i <= folderMaxDepth; i++) {
                    folderPermissionSettingCoalesceItems.Add($"CFS{i}.ContentFolderMemberPermissionSettingID");
                    folderTableJoinItems.Add($"left join CMS_ContentFolder as CF{i} on CF{i - 1}.ContentFolderParentFolderID = CF{i}.ContentFolderID left join XperienceCommunity_ContentFolderMemberPermissionSetting as CFS{i} on CF{i}.ContentFolderID = CFS{i}.ContentFolderMemberPermissionContentFolderID and CFS{i}.ContentFolderMemberPermissionSettingBreakInheritance = 1");
                }
                // First join is different format
                folderTableJoinItems.RemoveAt(0);

                // Generates the SQL to find the proper inherited permission for the given Content Folder Item
                var permissionQuery =
                $@"
Select 
Combined.ContentFolderID as ContentFolderID,
CF.ContentFolderID as ContentFolderOfPermissions,
CF.ContentFolderTreePath as ContentFolderPathOfPermissions,
InheritedMemberPermissionRequiresAuthentication,
InheritedMemberPermissionRoleTagID

from 
(
	Select 
	ContentFolderID,
	ContentFolderIDTaxonomyCheck,
	ContentItemToSecuritySettings.RequiresAuthentication as InheritedMemberPermissionRequiresAuthentication,
	XperienceCommunity_ContentFolderRoleTag.ContentFolderRoleTagTagID as InheritedMemberPermissionRoleTagID
	 from (
		select ContentFolderID as ContentFolderID, 
		case when ContentFolderMemberPermissionSettingBreakInheritance is null then null else ContentFolderMemberPermissionContentFolderID end as ContentFolderIDTaxonomyCheck,
		COALESCE(ContentFolderMemberPermissionSettingBreakInheritance, 1) as BreakInheritance,
		COALESCE(ContentFolderMemberPermissionSettingIsSecured, 0) as RequiresAuthentication
		from 
		(
			Select CF1.ContentFolderID,
			COALESCE(null, {string.Join(",", folderPermissionSettingCoalesceItems)}) as MemberPermissionID
			from CMS_ContentFolder as CF1 left join XperienceCommunity_ContentFolderMemberPermissionSetting as CFS1 on CF1.ContentFolderID = CFS1.ContentFolderMemberPermissionContentFolderID and CFS1.ContentFolderMemberPermissionSettingBreakInheritance = 1
	        {string.Join(" ", folderTableJoinItems)}
			where CF1.ContentFolderID = {contentFolderId}
		) ContentFolderToPermission
		left join XperienceCommunity_ContentFolderMemberPermissionSetting on ContentFolderToPermission.MemberPermissionID = ContentFolderMemberPermissionSettingID
	) ContentItemToSecuritySettings
	left join XperienceCommunity_ContentFolderRoleTag on ContentFolderIDTaxonomyCheck = ContentFolderRoleTagContentFolderID
) Combined
left join CMS_ContentFolder CF on CF.ContentFolderID = ContentFolderIDTaxonomyCheck";

                var results = (await ExecuteQueryAsync(permissionQuery, [], QueryTypeEnum.SQLQuery))
                    .Tables[0].Rows.Cast<DataRow>()
                    .GroupBy(x => (int)x["ContentFolderID"])
                    .ToDictionary(key => key.Key, value => {
                        var firstRow = value.First();
                        var inheritedContentFolderID = ValidationHelper.GetInteger(firstRow["ContentFolderOfPermissions"], 0);
                        return new MemberRolePermissionSummary(
                            RequiresAuthentication: ValidationHelper.GetBoolean(firstRow["InheritedMemberPermissionRequiresAuthentication"], false),
                            MemberRoles: value.Where(x => tagIdToRoleName.ContainsKey(ValidationHelper.GetInteger(x["InheritedMemberPermissionRoleTagID"], 0))).Select(x => tagIdToRoleName[ValidationHelper.GetInteger(x["InheritedMemberPermissionRoleTagID"], 0)].ToLowerInvariant()).ToArray(),
                            InheritanceType: inheritedContentFolderID == 0 ? PermissionInheritanceType.None : PermissionInheritanceType.ContentFolder,
                            OverridesExistOnContentItem: false,
                            PermissionIsSelf: inheritedContentFolderID == contentFolderId,
                            InheritedFromId: inheritedContentFolderID,
                            InheritedFromPath: ValidationHelper.GetString(firstRow["ContentFolderPathOfPermissions"], string.Empty)
                            );
                    });

                return results.TryGetValue(contentFolderId, out var summary) ? summary : new MemberRolePermissionSummary(false, [], PermissionInheritanceType.None, false, false);
                // Doing a hash code just in case large amounts of IDs are passed in, so the Cache Name won't be gigantic.
            }, new CacheSettings(15, "GetMemberRolePermissionSummaryByContentFolder", folderMaxDepth, contentFolderId));
        }

        public async Task<MemberRolePermissionSummary> GetMemberRolePermissionSummaryByWebPageItem(int webPageItemId)
        {
            var webPageMaxDepth = await GetWebPageItemMaxDepth();
            var tagIdToRoleName = await GetTagIdToRoleName();

            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency([
                        $"{WebPageItemMemberPermissionSettingInfo.OBJECT_TYPE}|all",
                        $"{WebPageItemRoleTagInfo.OBJECT_TYPE}|all",
                        $"{TagInfo.OBJECT_TYPE}|all", // So it clears if tags are added or removed as well
                        $"webpageitem|byid|{webPageItemId}"
                    ]);
                }

                var webPagePermissionSettingCoalesceItems = new List<string>();
                var webPageTableJoinItems = new List<string>();
                for (var i = 1; i <= webPageMaxDepth; i++) {
                    webPagePermissionSettingCoalesceItems.Add($"MPS{i}.WebPageItemMemberPermissionSettingID");
                    webPageTableJoinItems.Add($"left join CMS_WebPageItem as WP{i} on WP{i - 1}.WebPageItemParentID = WP{i}.WebPageItemID left join XperienceCommunity_WebPageItemMemberPermissionSetting as MPS{i} on WP{i}.WebPageItemID = MPS{i}.WebPageItemMemberPermissionSettingWebPageItemID and MPS{i}.WebPageItemMemberPermissionSettingBreakInheritance = 1");
                }
                // First join is different format
                webPageTableJoinItems.RemoveAt(0);

                // Generates the SQL to find the proper inherited permission for the given content items
                // Does joins on parents for WebPageItems and ContentFolders, with logic to handle if Breaking Inheritance or not, and the roles for the items.
                // Each row is only an int, bit, and int, so should be fast even for large quantities of data
                var permissionQuery =
    $@"
Select 
InheritedMemberPermissionWebPageItemID,
WPI.WebPageItemID as WebPageItemOfPermissions,
WPI.WebPageItemTreePath as WebPageItemPathOfPermissions,
WPI.WebPageItemWebsiteChannelID,
InheritedMemberPermissionRequiresAuthentication,
InheritedMemberPermissionRoleTagID

from 
(
	select 
	WebPageItemToSecuritySettings.WebPageItemID as InheritedMemberPermissionWebPageItemID,
	WebPageItemIDTaxonomyCheck,
	WebPageItemToSecuritySettings.RequiresAuthentication as InheritedMemberPermissionRequiresAuthentication,
	XperienceCommunity_WebPageItemRoleTag.WebPageItemRoleTagTagID as InheritedMemberPermissionRoleTagID
	 from (

		select WebPageItemID, 
		case when WebPageItemMemberPermissionSettingBreakInheritance is null then null else WebPageItemMemberPermissionSettingWebPageItemID end as WebPageItemIDTaxonomyCheck,
		COALESCE(WebPageItemMemberPermissionSettingIsSecured, 0) as RequiresAuthentication

		from 
		(
			select WP1.WebPageItemID,
			COALESCE(null, {string.Join(",", webPagePermissionSettingCoalesceItems)}) as MemberPermissionID
			from CMS_WebPageItem as WP1 left join XperienceCommunity_WebPageItemMemberPermissionSetting as MPS1 on WP1.WebPageItemID = MPS1.WebPageItemMemberPermissionSettingWebPageItemID and MPS1.WebPageItemMemberPermissionSettingBreakInheritance = 1
			{string.Join(" ", webPageTableJoinItems)}
			where WP1.WebPageItemID = {webPageItemId}
		) WebPageToPermission
		left join XperienceCommunity_WebPageItemMemberPermissionSetting on WebPageToPermission.MemberPermissionID = WebPageItemMemberPermissionSettingID
	) WebPageItemToSecuritySettings
	left join XperienceCommunity_WebPageItemRoleTag on WebPageItemIDTaxonomyCheck = WebPageItemRoleTagWebPageItemID
) Combined
left join CMS_WebPageItem WPI on WPI.WebPageItemID = WebPageItemIDTaxonomyCheck
";
                // also do quick check for any overrides for the ContentItems
                var overrideExistanceQuery = $@"Select WebPageItemID from CMS_WebPageItem 
inner join CMS_ContentItem on ContentItemID = WebPageItemContentItemID
inner join CMS_ContentItemCommonData on ContentItemCommonDataContentItemID = ContentItemID
where WebPageItemID = {webPageItemId} and MemberPermissionOverride = 1 and ContentItemCommonDataIsLatest = 1";

                var overridesExist = (await ExecuteQueryAsync(overrideExistanceQuery, [], QueryTypeEnum.SQLQuery)).Tables[0].Rows.Count > 0;

                var items = (await ExecuteQueryAsync(permissionQuery, [], QueryTypeEnum.SQLQuery))
                    .Tables[0].Rows.Cast<DataRow>()
                    .GroupBy(x => (int)x["InheritedMemberPermissionWebPageItemID"])
                    .ToDictionary(key => key.Key, value => {
                        var firstRow = value.First();
                        var inheritedWebPageItemID = ValidationHelper.GetInteger(firstRow["WebPageItemOfPermissions"], 0);

                        return new MemberRolePermissionSummary(
                        RequiresAuthentication: ValidationHelper.GetBoolean(firstRow["InheritedMemberPermissionRequiresAuthentication"], false),
                        MemberRoles: value.Where(x => tagIdToRoleName.ContainsKey(ValidationHelper.GetInteger(x["InheritedMemberPermissionRoleTagID"], 0))).Select(x => tagIdToRoleName[ValidationHelper.GetInteger(x["InheritedMemberPermissionRoleTagID"], 0)].ToLowerInvariant()).ToArray(),
                        InheritanceType: inheritedWebPageItemID == 0 ? PermissionInheritanceType.None : PermissionInheritanceType.WebPageItem,
                        OverridesExistOnContentItem: overridesExist,
                        PermissionIsSelf: inheritedWebPageItemID == webPageItemId,
                        ChannelId: ValidationHelper.GetInteger(firstRow["WebPageItemWebsiteChannelID"], 1),
                        InheritedFromId: inheritedWebPageItemID,
                        InheritedFromPath: ValidationHelper.GetString(firstRow["WebPageItemPathOfPermissions"], string.Empty)
                        );
                        }
                    );

                return items.TryGetValue(webPageItemId, out var summary) ? summary : new MemberRolePermissionSummary(false, [], PermissionInheritanceType.None, false, false);
            }, new CacheSettings(15, "GetMemberRolePermissionSummaryByWebPageItem", webPageMaxDepth, webPageItemId));
            
        }

        internal record ContentItemSummaryLookup(MemberRolePermissionSummary? Summary = null, int? WebPageItemID = null, int? ContentItemFolderID = null);
    }
    /// <summary>
    /// Used primarily for casting objects into a unified implementation of the IXperienceCommunityMemberPermissionConfiguration for internal usage
    /// </summary>
    public class BaseXperienceCommunityMemberPermissionConfiguration : IXperienceCommunityMemberPermissionConfiguration
    {
        public bool MemberPermissionOverride { get; set; } = false;
        public bool MemberPermissionIsSecure { get; set; } = false;
        public IEnumerable<TagReference> MemberPermissionRoleTags { get; set; } = [];
    }

    
}

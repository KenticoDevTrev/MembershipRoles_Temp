
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using XperienceCommunity.MemberRoles.Admin.UIPages;
using XperienceCommunity.MemberRoles.Admin.UIPages.ContentFolderMemberPermission;
using XperienceCommunity.MemberRoles.Models;

[assembly: UIPage(
    parentType: typeof(FolderPermissionsApplication),
    slug: ContentFolderMemberPermissionListing.SLUG,
    uiPageType: typeof(ContentFolderMemberPermissionListing),
    name: "Folder Permissions",
    templateName: TemplateNames.LISTING,
    order: 200
    )]

namespace XperienceCommunity.MemberRoles.Admin.UIPages.ContentFolderMemberPermission
{
    [UIPermission(SystemPermissions.VIEW)]
    [UIPermission(SystemPermissions.CREATE)]
    [UIPermission(SystemPermissions.DELETE)]
    [UIPermission(SystemPermissions.UPDATE)]
    public class ContentFolderMemberPermissionListing(IInfoProvider<ContentFolderMemberPermissionSettingInfo> contentFolderMemberPermissionSettingInfoProvider,
        IInfoProvider<ContentFolderRoleTagInfo> contentFolderRoleTagInfoProvider,
        IInfoProvider<TagInfo> tagInfoProvider
        ) : ListingPage
    {
        public const string SLUG = "content-folder-member-permissions";
        public const string IDENTIFIER = "XperienceCommunity.MemberRoles.ContentFolderMemberPermissionApplication";

        private readonly IInfoProvider<ContentFolderMemberPermissionSettingInfo> _contentFolderMemberPermissionSettingInfoProvider = contentFolderMemberPermissionSettingInfoProvider;
        private readonly IInfoProvider<ContentFolderRoleTagInfo> _contentFolderRoleTagInfoProvider = contentFolderRoleTagInfoProvider;
        private readonly IInfoProvider<TagInfo> _tagInfoProvider = tagInfoProvider;

        protected override string ObjectType => ContentFolderInfo.OBJECT_TYPE;

        public async override Task ConfigurePage()
        {

            var foldersWithPermissions = await _contentFolderMemberPermissionSettingInfoProvider.Get()
                .Source(x => x.InnerJoin<ContentFolderInfo>(nameof(ContentFolderMemberPermissionSettingInfo.ContentFolderMemberPermissionContentFolderID), nameof(ContentFolderInfo.ContentFolderID)))
                .WhereEquals(nameof(ContentFolderMemberPermissionSettingInfo.ContentFolderMemberPermissionSettingBreakInheritance), true)
                .GetDataContainerResultAsync();

            var roleTagByID = (await _tagInfoProvider.Get()
                .Source(x => x.InnerJoin<TaxonomyInfo>(nameof(TagInfo.TagTaxonomyID), nameof(TaxonomyInfo.TaxonomyID)))
                .WhereEquals(nameof(TaxonomyInfo.TaxonomyName), MemberRoleConstants._MemberRoleTaxonomy)
                .Columns(nameof(TagInfo.TagID), nameof(TagInfo.TagName))
                .GetEnumerableTypedResultAsync())
                .ToDictionary(key => key.TagID, value => value.TagName.ToLowerInvariant());

            // get all the roles
            var contentFolderIdToRoleTags = (await _contentFolderRoleTagInfoProvider.Get()
                .WhereIn(nameof(ContentFolderRoleTagInfo.ContentFolderRoleTagTagID), roleTagByID.Keys)
                .GetEnumerableTypedResultAsync())
                .GroupBy(key => key.ContentFolderRoleTagContentFolderID)
                .ToDictionary(key => key.Key, value => value.Select(x => roleTagByID[x.ContentFolderRoleTagTagID]));
            

            var folderTreePathWithPermissionSummary = foldersWithPermissions.ToDictionary(key => ((string)key.GetValue(nameof(ContentFolderInfo.ContentFolderTreePath))).ToLowerInvariant(),
                value => 
                    new MemberAuthorizationSummary(
                        (bool)value.GetValue(nameof(ContentFolderMemberPermissionSettingInfo.ContentFolderMemberPermissionSettingIsSecured)),
                        contentFolderIdToRoleTags.TryGetValue((int)value.GetValue(nameof(ContentFolderMemberPermissionSettingInfo.ContentFolderMemberPermissionContentFolderID)), out var roles) ? roles : []
                    )
                );


            PageConfiguration.ColumnConfigurations
                .AddColumn(nameof(ContentFolderInfo.ContentFolderDisplayName), "Display Name", searchable: true, sortable: false)
                .AddColumn(nameof(ContentFolderInfo.ContentFolderTreePath), "Tree Path", searchable: true, sortable: false)
                .AddColumn(nameof(ContentFolderInfo.ContentFolderTreePath), "Permissions", formatter: (obj, data) =>
                {
                    var folderTreePath = ((string)obj).ToLowerInvariant();
                    var splitItems = folderTreePath.Split("/", StringSplitOptions.RemoveEmptyEntries);
                    var result = FindSummary(splitItems, folderTreePathWithPermissionSummary);
                    
                    if(result != null) {
                        return $"{(folderTreePathWithPermissionSummary.ContainsKey(folderTreePath) ? "Own Permissions" : "Inherited Permissions")} [{(result.RequiresAuthentication ? $"Auth Required - {result.Roles.Count()} roles" : "No Auth Required")}]";
                    }
                    return "None [No Auth Required]";
                });
            PageConfiguration.QueryModifiers.AddModifier(query =>
                query.WhereNotEquals(nameof(ContentFolderInfo.ContentFolderTreePath), "/")
                .OrderBy(nameof(ContentFolderInfo.ContentFolderTreePath))
            );

            PageConfiguration.AddEditRowAction<ContentFolderMemberSecurityPageTemplate>();

            await base.ConfigurePage();

        }


        private MemberAuthorizationSummary? FindSummary(IEnumerable<string> contentFolderPathSplit, Dictionary<string, MemberAuthorizationSummary> dictionary)
        {
            if(contentFolderPathSplit.Count() == 0) {
                return null;
            }

            return dictionary.TryGetValue($"/{string.Join("/", contentFolderPathSplit)}", out var summary) ? summary : FindSummary(contentFolderPathSplit.Take(contentFolderPathSplit.Count() - 1), dictionary);
        }
        
    }

    
}

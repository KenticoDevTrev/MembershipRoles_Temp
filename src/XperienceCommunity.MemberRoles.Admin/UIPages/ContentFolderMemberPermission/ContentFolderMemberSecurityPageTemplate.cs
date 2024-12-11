using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Web.Mvc.Internal;
using Kentico.Xperience.Admin.Base;
using XperienceCommunity.MemberRoles.Admin.ActionComponents;
using XperienceCommunity.MemberRoles.Admin.Extensions;
using XperienceCommunity.MemberRoles.Admin.UIPages.ContentFolderMemberPermission;
using XperienceCommunity.MemberRoles.Repositories;

[assembly: UIPage(

    parentType: typeof(ContentFolderMemberPermissionEditSection),
    slug: ContentFolderMemberSecurityPageTemplate.SLUG,
    uiPageType: typeof(ContentFolderMemberSecurityPageTemplate),
    name: "Edit Permissions",
    templateName: "@memberroles/web-admin/ContentFolderMemberSecurityPage",
    order: 200
    )]

namespace XperienceCommunity.MemberRoles.Admin.UIPages.ContentFolderMemberPermission
{
    public class ContentFolderMemberSecurityPageTemplate(IInfoProvider<ContentFolderRoleTagInfo> contentFolderRoleTagInfoProvider,
        IInfoProvider<ContentFolderMemberPermissionSettingInfo> contentFolderMemberPermissionSettingInfoProvider,
        IInfoProvider<TagInfo> tagInfoProvider,
        IMemberPermissionSummaryRepository memberPermissionSummaryRepository,
        IAdminPathRetriever adminPathRetriever) : Page<ContentFolderMemberRoleProperties>
    {
        private readonly IInfoProvider<ContentFolderRoleTagInfo> _contentFolderRoleTagInfoProvider = contentFolderRoleTagInfoProvider;
        private readonly IInfoProvider<ContentFolderMemberPermissionSettingInfo> _contentFolderMemberPermissionSettingInfoProvider = contentFolderMemberPermissionSettingInfoProvider;
        private readonly IInfoProvider<TagInfo> _tagInfoProvider = tagInfoProvider;
        private readonly IMemberPermissionSummaryRepository _memberPermissionSummaryRepository = memberPermissionSummaryRepository;
        private readonly IAdminPathRetriever _adminPathRetriever = adminPathRetriever;
        public const string SLUG = "permissions";

        [PageParameter(typeof(IntPageModelBinder))]
        public int ContentFolderID { get; set; }

        public async override Task<ContentFolderMemberRoleProperties> ConfigureTemplateProperties(ContentFolderMemberRoleProperties properties)
        {
            var allRoleTags = await _tagInfoProvider.Get()
                .Source(x => x.Join<TaxonomyInfo>(nameof(TagInfo.TagTaxonomyID), nameof(TaxonomyInfo.TaxonomyID)))
                .WhereEquals(nameof(TaxonomyInfo.TaxonomyName), MemberRoleConstants._MemberRoleTaxonomy)
                .GetEnumerableTypedResultAsync();

            var selectedTags = await _tagInfoProvider.Get()
                .Source(x => x.InnerJoin<ContentFolderRoleTagInfo>(nameof(TagInfo.TagID), nameof(ContentFolderRoleTagInfo.ContentFolderRoleTagTagID)))
                .WhereEquals(nameof(ContentFolderRoleTagInfo.ContentFolderRoleTagContentFolderID), ContentFolderID)
                .GetEnumerableTypedResultAsync();

            var currentConfiguration = (await _contentFolderMemberPermissionSettingInfoProvider.Get()
                .WhereEquals(nameof(ContentFolderMemberPermissionSettingInfo.ContentFolderMemberPermissionContentFolderID), ContentFolderID)
                .GetEnumerableTypedResultAsync())
                .FirstOrDefault();

            properties.MemberRoleTagIds = selectedTags.Select(x => x.TagID).ToList();
            properties.AvailableRoles = allRoleTags.Select(x => new ContentFolderMemberPermissionTagItem(x.TagTitle, x.TagName, x.TagID));

            if (currentConfiguration != null)
            {
                properties.BreakInheritance = currentConfiguration.ContentFolderMemberPermissionSettingBreakInheritance;
                properties.RequireAuthentication = currentConfiguration.ContentFolderMemberPermissionSettingIsSecured;
            }

            properties.MemberRolePermissionSummary = (await _memberPermissionSummaryRepository.GetMemberRolePermissionSummaryByContentFolder(ContentFolderID)).ToClientProperties(_adminPathRetriever.GetAdminPrefix());

            return properties;
        }

        [PageCommand]
        public async Task<ICommandResponse<ContentFolderMemberRoleData>> SetProperties(ContentFolderMemberRoleData properties)
        {
            var currentConfiguration = (await _contentFolderMemberPermissionSettingInfoProvider.Get()
                .WhereEquals(nameof(ContentFolderMemberPermissionSettingInfo.ContentFolderMemberPermissionContentFolderID), ContentFolderID)
                .GetEnumerableTypedResultAsync())
                .FirstOrDefault() ?? new ContentFolderMemberPermissionSettingInfo();

            currentConfiguration.ContentFolderMemberPermissionSettingBreakInheritance = properties.BreakInheritance;
            currentConfiguration.ContentFolderMemberPermissionSettingIsSecured = properties.RequireAuthentication;
            currentConfiguration.ContentFolderMemberPermissionContentFolderID = ContentFolderID;

            if (currentConfiguration.HasChanged)
            {
                _contentFolderMemberPermissionSettingInfoProvider.Set(currentConfiguration);
            }

            // handle taxonomies
            var allRoleTags = (await _tagInfoProvider.Get()
                            .Source(x => x.Join<TaxonomyInfo>(nameof(TagInfo.TagTaxonomyID), nameof(TaxonomyInfo.TaxonomyID)))
                            .WhereEquals(nameof(TaxonomyInfo.TaxonomyName), MemberRoleConstants._MemberRoleTaxonomy)
                            .GetEnumerableTypedResultAsync()).Select(x => x.TagID);

            var selectedTags = (await _tagInfoProvider.Get()
                .Source(x => x.InnerJoin<ContentFolderRoleTagInfo>(nameof(TagInfo.TagID), nameof(ContentFolderRoleTagInfo.ContentFolderRoleTagTagID)))
                .WhereEquals(nameof(ContentFolderRoleTagInfo.ContentFolderRoleTagContentFolderID), ContentFolderID)
                .GetEnumerableTypedResultAsync()).Select(x => x.TagID);

            // Add new items
            var tagsToAdd = properties.MemberRoleTagIds.Except(selectedTags);
            foreach (var tagToAdd in tagsToAdd)
            {
                _contentFolderRoleTagInfoProvider.Set(new ContentFolderRoleTagInfo()
                {
                    ContentFolderRoleTagContentFolderID = ContentFolderID,
                    ContentFolderRoleTagTagID = tagToAdd
                });
            }

            // Remove old items
            var tagsToRemove = await _contentFolderRoleTagInfoProvider.Get()
               .WhereEquals(nameof(ContentFolderRoleTagInfo.ContentFolderRoleTagContentFolderID), ContentFolderID)
               .WhereIn(nameof(ContentFolderRoleTagInfo.ContentFolderRoleTagTagID), selectedTags.Except(properties.MemberRoleTagIds))
               .GetEnumerableTypedResultAsync();
            foreach (var tagToRemove in tagsToRemove)
            {
                _contentFolderRoleTagInfoProvider.Delete(tagToRemove);
            }

            // regenerate summary
            properties.MemberRolePermissionSummary = (await _memberPermissionSummaryRepository.GetMemberRolePermissionSummaryByContentFolder(ContentFolderID)).ToClientProperties(_adminPathRetriever.GetAdminPrefix());

            return ResponseFrom(properties).AddSuccessMessage($"Permissions Updated and {tagsToAdd.Count()} Roles Added and {tagsToRemove.Count()} Roles Removed");
        }
    }

    public record ContentFolderMemberPermissionTagItem(string TagTitle, string TagName, int TagId);

    public class ContentFolderMemberRoleProperties : TemplateClientProperties
    {
        public bool BreakInheritance { get; set; } = false;
        public bool RequireAuthentication { get; set; } = false;
        public IEnumerable<int> MemberRoleTagIds { get; set; } = [];
        public IEnumerable<ContentFolderMemberPermissionTagItem> AvailableRoles { get; set; } = [];
        public MemberRolesPermissionSummaryClientProperties MemberRolePermissionSummary { get; set; } = new MemberRolesPermissionSummaryClientProperties();


    }

    public class ContentFolderMemberRoleData : TemplateClientProperties
    {
        public bool BreakInheritance { get; set; } = false;
        public bool RequireAuthentication { get; set; } = false;
        public IEnumerable<int> MemberRoleTagIds { get; set; } = [];
        public MemberRolesPermissionSummaryClientProperties MemberRolePermissionSummary { get; set; } = new MemberRolesPermissionSummaryClientProperties();
    }
}


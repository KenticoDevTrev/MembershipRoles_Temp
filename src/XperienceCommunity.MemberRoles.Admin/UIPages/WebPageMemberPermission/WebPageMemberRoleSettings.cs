using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Websites.UIPages;
using XperienceCommunity.MemberRoles.Admin.ActionComponents;
using XperienceCommunity.MemberRoles.Admin.Extensions;
using XperienceCommunity.MemberRoles.Admin.UIPages.WebPageMemberPermission;
using XperienceCommunity.MemberRoles.Repositories;

[assembly: UIPage(
    parentType: typeof(WebPagePropertiesTab),
    slug: "member-permissions",
    uiPageType: typeof(WebPageItemMemberPermissionPageTemplate),
    name: "Member Permissions",
    templateName: "@memberroles/web-admin/WebPageItemMemberSecurityPage",
    order: 350)]

namespace XperienceCommunity.MemberRoles.Admin.UIPages.WebPageMemberPermission
{
    public class WebPageItemMemberPermissionPageTemplate(IInfoProvider<WebPageItemRoleTagInfo> webPageItemRoleTagInfoProvider,
        IInfoProvider<WebPageItemMemberPermissionSettingInfo> webPageItemMemberPermissionSettingInfoProvider,
        IInfoProvider<TagInfo> tagInfoProvider,
        IMemberPermissionSummaryRepository memberPermissionSummaryRepository) : Page<WebPageMemberRoleProperties>
    {
        private readonly IInfoProvider<WebPageItemRoleTagInfo> _webPageItemRoleTagInfoProvider = webPageItemRoleTagInfoProvider;
        private readonly IInfoProvider<WebPageItemMemberPermissionSettingInfo> _webPageItemMemberPermissionSettingInfoProvider = webPageItemMemberPermissionSettingInfoProvider;
        private readonly IInfoProvider<TagInfo> _tagInfoProvider = tagInfoProvider;
        private readonly IMemberPermissionSummaryRepository _memberPermissionSummaryRepository = memberPermissionSummaryRepository;

        [PageParameter(typeof(WebPageModelBinder))]
        public int WebPageItemID { get; set; }

        public async override Task<WebPageMemberRoleProperties> ConfigureTemplateProperties(WebPageMemberRoleProperties properties)
        {
            var allRoleTags = await _tagInfoProvider.Get()
                .Source(x => x.Join<TaxonomyInfo>(nameof(TagInfo.TagTaxonomyID), nameof(TaxonomyInfo.TaxonomyID)))
                .WhereEquals(nameof(TaxonomyInfo.TaxonomyName), MemberRoleConstants._MemberRoleTaxonomy)
                .GetEnumerableTypedResultAsync();

            var selectedTags = await _tagInfoProvider.Get()
                .Source(x => x.InnerJoin<WebPageItemRoleTagInfo>(nameof(TagInfo.TagID), nameof(WebPageItemRoleTagInfo.WebPageItemRoleTagTagID)))
                .WhereEquals(nameof(WebPageItemRoleTagInfo.WebPageItemRoleTagWebPageItemID), WebPageItemID)
                .GetEnumerableTypedResultAsync();

            var currentConfiguration = (await _webPageItemMemberPermissionSettingInfoProvider.Get()
                .WhereEquals(nameof(WebPageItemMemberPermissionSettingInfo.WebPageItemMemberPermissionSettingWebPageItemID), WebPageItemID)
                .GetEnumerableTypedResultAsync())
                .FirstOrDefault();

            properties.MemberRoleTagIds = selectedTags.Select(x => x.TagID).ToList();
            properties.AvailableRoles = allRoleTags.Select(x => new WebPageMemberPermissionTagItem(x.TagTitle, x.TagName, x.TagID));

            if (currentConfiguration != null)
            {
                properties.BreakInheritance = currentConfiguration.WebPageItemMemberPermissionSettingBreakInheritance;
                properties.RequireAuthentication = currentConfiguration.WebPageItemMemberPermissionSettingIsSecured;
            }

            properties.MemberRolePermissionSummary = (await _memberPermissionSummaryRepository.GetMemberRolePermissionSummaryByWebPageItem(WebPageItemID)).ToClientProperties();

            return properties;
        }

        [PageCommand]
        public async Task<ICommandResponse<WebPageMemberRoleData>> SetProperties(WebPageMemberRoleData properties)
        {
            var currentConfiguration = (await _webPageItemMemberPermissionSettingInfoProvider.Get()
                .WhereEquals(nameof(WebPageItemMemberPermissionSettingInfo.WebPageItemMemberPermissionSettingWebPageItemID), WebPageItemID)
                .GetEnumerableTypedResultAsync())
                .FirstOrDefault() ?? new WebPageItemMemberPermissionSettingInfo();

            currentConfiguration.WebPageItemMemberPermissionSettingBreakInheritance = properties.BreakInheritance;
            currentConfiguration.WebPageItemMemberPermissionSettingIsSecured = properties.RequireAuthentication;
            currentConfiguration.WebPageItemMemberPermissionSettingWebPageItemID = WebPageItemID;

            if (currentConfiguration.HasChanged)
            {
                _webPageItemMemberPermissionSettingInfoProvider.Set(currentConfiguration);
            }

            // handle taxonomies
            var allRoleTags = (await _tagInfoProvider.Get()
                            .Source(x => x.Join<TaxonomyInfo>(nameof(TagInfo.TagTaxonomyID), nameof(TaxonomyInfo.TaxonomyID)))
                            .WhereEquals(nameof(TaxonomyInfo.TaxonomyName), MemberRoleConstants._MemberRoleTaxonomy)
                            .GetEnumerableTypedResultAsync()).Select(x => x.TagID);

            var selectedTags = (await _tagInfoProvider.Get()
                .Source(x => x.InnerJoin<WebPageItemRoleTagInfo>(nameof(TagInfo.TagID), nameof(WebPageItemRoleTagInfo.WebPageItemRoleTagTagID)))
                .WhereEquals(nameof(WebPageItemRoleTagInfo.WebPageItemRoleTagWebPageItemID), WebPageItemID)
                .GetEnumerableTypedResultAsync()).Select(x => x.TagID);

            // Add new items
            var tagsToAdd = properties.MemberRoleTagIds.Except(selectedTags);
            foreach (var tagToAdd in tagsToAdd)
            {
                _webPageItemRoleTagInfoProvider.Set(new WebPageItemRoleTagInfo()
                {
                    WebPageItemRoleTagWebPageItemID = WebPageItemID,
                    WebPageItemRoleTagTagID = tagToAdd
                });
            }

            // Remove old items
            var tagsToRemove = await _webPageItemRoleTagInfoProvider.Get()
               .WhereEquals(nameof(WebPageItemRoleTagInfo.WebPageItemRoleTagWebPageItemID), WebPageItemID)
               .WhereIn(nameof(WebPageItemRoleTagInfo.WebPageItemRoleTagTagID), selectedTags.Except(properties.MemberRoleTagIds))
               .GetEnumerableTypedResultAsync();
            foreach (var tagToRemove in tagsToRemove)
            {
                _webPageItemRoleTagInfoProvider.Delete(tagToRemove);
            }

            // regenerate summary
            properties.MemberRolePermissionSummary = (await _memberPermissionSummaryRepository.GetMemberRolePermissionSummaryByWebPageItem(WebPageItemID)).ToClientProperties();

            return ResponseFrom(properties).AddSuccessMessage($"Permissions Updated and {tagsToAdd.Count()} Roles Added and {tagsToRemove.Count()} Roles Removed");
        }


    }

    public record WebPageMemberPermissionTagItem(string TagTitle, string TagName, int TagId);


    public class WebPageMemberRoleProperties : TemplateClientProperties
    {
        public bool BreakInheritance { get; set; } = false;
        public bool RequireAuthentication { get; set; } = false;
        public IEnumerable<int> MemberRoleTagIds { get; set; } = [];
        public IEnumerable<WebPageMemberPermissionTagItem> AvailableRoles { get; set; } = [];
        public MemberRolesPermissionSummaryClientProperties MemberRolePermissionSummary { get; set; } = new MemberRolesPermissionSummaryClientProperties();
    }

    public class WebPageMemberRoleData : TemplateClientProperties
    {
        public bool BreakInheritance { get; set; } = false;
        public bool RequireAuthentication { get; set; } = false;
        public IEnumerable<int> MemberRoleTagIds { get; set; } = [];
        public MemberRolesPermissionSummaryClientProperties MemberRolePermissionSummary { get; set; } = new MemberRolesPermissionSummaryClientProperties();
    }
}


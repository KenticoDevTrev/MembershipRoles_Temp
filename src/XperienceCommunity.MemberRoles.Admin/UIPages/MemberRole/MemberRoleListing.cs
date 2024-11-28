
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using XperienceCommunity.MemberRoles.Admin.UIPages;
using XperienceCommunity.MemberRoles.Admin.UIPages.MemberRole;

[assembly: UIPage(
    parentType: typeof(MemberRolesApplication),
    slug: "list",
    uiPageType: typeof(MemberRoleListing),
    name: "Member Roles",
    templateName: TemplateNames.LISTING,
    order: UIPageOrder.First
    )]

namespace XperienceCommunity.MemberRoles.Admin.UIPages.MemberRole
{
    [UIPermission(SystemPermissions.VIEW)]
    [UIPermission(SystemPermissions.CREATE)]
    [UIPermission(SystemPermissions.DELETE)]
    [UIPermission(SystemPermissions.UPDATE)]
    public class MemberRoleListing(IInfoProvider<TaxonomyInfo> taxonomyInfoProvider) : ListingPage
    {
        public const string IDENTIFIER = "XperienceCommunity.MemberRoleApplication";
        private readonly IInfoProvider<TaxonomyInfo> _taxonomyInfoProvider = taxonomyInfoProvider;

        protected override string ObjectType => TagInfo.OBJECT_TYPE;

        public async override Task ConfigurePage()
        {
            var taxonomy = (await _taxonomyInfoProvider.Get()
                .WhereEquals(nameof(TaxonomyInfo.TaxonomyName), MemberRoleConstants._MemberRoleTaxonomy)
                .GetEnumerableTypedResultAsync())
                .FirstOrDefault();

            if (taxonomy == null)
            {
                PageConfiguration.Callouts.Add(new CalloutConfiguration()
                {
                    Content = "Member Role Taxonomy is missing!",
                    Type = CalloutType.FriendlyWarning,
                    Placement = CalloutPlacement.OnDesk
                });
                PageConfiguration.QueryModifiers.AddModifier(query => query.Where("0 = 1"));
                return;
            }

            PageConfiguration.ColumnConfigurations
                .AddColumn(nameof(TagInfo.TagTitle), "Display Name", searchable: true, sortable: true)
                .AddColumn(nameof(TagInfo.TagName), "Code Name", searchable: true, sortable: true)
                .AddColumn(nameof(TagInfo.TagDescription), "Description");
            PageConfiguration.QueryModifiers.AddModifier(query =>
                query.WhereEquals(nameof(TagInfo.TagTaxonomyID), taxonomy.TaxonomyID)
                );


            var parameters = new PageParameterValues {
                { typeof(TaxonomyEditSection), taxonomy.TaxonomyID },
                { typeof(TagEditLayout), 0 }
            };

            PageConfiguration.HeaderActions.AddLink<TagCreate>("Add Role Tag", parameters: parameters);
            PageConfiguration.AddEditRowAction<MemberRoleEditSection>();

            await base.ConfigurePage();

        }
    }
}

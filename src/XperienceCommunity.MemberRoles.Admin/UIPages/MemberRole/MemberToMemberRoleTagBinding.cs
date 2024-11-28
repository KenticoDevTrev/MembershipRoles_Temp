using CMS.ContentEngine;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using XperienceCommunity.MemberRoles.Admin.UIPages.MemberRole;

[assembly: UIPage(
    parentType: typeof(MemberEditSection),
    slug: "roles-members",
    uiPageType: typeof(MemberToMemberRoleTagBinding),
    name: "Roles",
    templateName: TemplateNames.BINDING,
    order: 200)]

namespace XperienceCommunity.MemberRoles.Admin.UIPages.MemberRole
{
    public class MemberToMemberRoleTagBinding : InfoBindingPage<MemberRoleTagInfo, TagInfo>
    {
        // ID of the edited object
        [PageParameter(typeof(IntPageModelBinder))]
        public override int EditedObjectId { get; set; }

        protected override string SourceBindingColumn => nameof(MemberRoleTagInfo.MemberRoleTagMemberID);

        protected override string TargetBindingColumn => nameof(MemberRoleTagInfo.MemberRoleTagTagID);

        // UI page configuration
        public override Task ConfigurePage()
        {
            // Columns
            PageConfiguration.ExistingBindingsListing.ColumnConfigurations
                .AddColumn(nameof(TagInfo.TagTitle), "Title", searchable: true, defaultSortDirection: SortTypeEnum.Asc)
                .AddColumn(nameof(TagInfo.TagName), "Name", searchable: true, defaultSortDirection: SortTypeEnum.Asc);

            // Sets the caption for the 'Add items' button
            PageConfiguration.AddBindingButtonText = "Add roles";

            // Sets the heading of the page
            PageConfiguration.ExistingBindingsListing.Caption = "Add roles to member";

            // Columns
            PageConfiguration.BindingSidePanelListing.ColumnConfigurations
                .AddColumn(nameof(TagInfo.TagTitle), "Title", searchable: true, defaultSortDirection: SortTypeEnum.Asc)
                .AddColumn(nameof(TagInfo.TagName), "Name", searchable: true, defaultSortDirection: SortTypeEnum.Asc);

            PageConfiguration.BindingSidePanelListing.QueryModifiers.AddModifier(query => query
                .Source(x => x.InnerJoin<TaxonomyInfo>(nameof(TagInfo.TagTaxonomyID), nameof(TaxonomyInfo.TaxonomyID)))
                .WhereEquals(nameof(TaxonomyInfo.TaxonomyName), MemberRoleConstants._MemberRoleTaxonomy)
            );

            // Sets the caption for the side panel 'Add items' button
            PageConfiguration.SaveBindingsButtonText = "Add roles";

            // Sets the heading of the side panel
            PageConfiguration.BindingSidePanelListing.Caption = "Add roles to member";

            return base.ConfigurePage();
        }
    }
}

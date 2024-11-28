using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using XperienceCommunity.MemberRoles.Admin.UIPages.MemberRole;

[assembly: UIPage(
    parentType: typeof(MemberRoleEditSection),
    slug: "roles-members",
    uiPageType: typeof(MemberRoleTagsBinding),
    name: "Assigned members",
    templateName: TemplateNames.BINDING,
    order: 200)]

namespace XperienceCommunity.MemberRoles.Admin.UIPages.MemberRole
{
    public class MemberRoleTagsBinding : InfoBindingPage<MemberRoleTagInfo, MemberInfo>
    {
        // ID of the edited object
        [PageParameter(typeof(IntPageModelBinder))]
        public override int EditedObjectId { get; set; }

        protected override string SourceBindingColumn => nameof(MemberRoleTagInfo.MemberRoleTagTagID);

        protected override string TargetBindingColumn => nameof(MemberRoleTagInfo.MemberRoleTagMemberID);

        // UI page configuration
        public override Task ConfigurePage()
        {
            // Columns
            PageConfiguration.ExistingBindingsListing.ColumnConfigurations
                .AddColumn(nameof(MemberInfo.MemberEmail), "Email", searchable: true, defaultSortDirection: SortTypeEnum.Asc)
                .AddColumn(nameof(MemberInfo.MemberName), "Name", searchable: true, defaultSortDirection: SortTypeEnum.Asc);

            // Sets the caption for the 'Add items' button
            PageConfiguration.AddBindingButtonText = "Add members";

            // Sets the heading of the page
            PageConfiguration.ExistingBindingsListing.Caption = "Add members to segment";

            // Columns
            PageConfiguration.BindingSidePanelListing.ColumnConfigurations
                .AddColumn(nameof(MemberInfo.MemberEmail), "Email", searchable: true, defaultSortDirection: SortTypeEnum.Asc)
                .AddColumn(nameof(MemberInfo.MemberName), "Name", searchable: true, defaultSortDirection: SortTypeEnum.Asc);

            // Sets the caption for the side panel 'Add items' button
            PageConfiguration.SaveBindingsButtonText = "Add members";

            // Sets the heading of the side panel
            PageConfiguration.BindingSidePanelListing.Caption = "Add members to segment";

            return base.ConfigurePage();
        }
    }
}

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages;
using XperienceCommunity.MemberRoles.Admin.UIPages;


[assembly: UIApplication(
    identifier: MemberRolesApplication.IDENTIFIER,
    type: typeof(MemberRolesApplication),
    slug: MemberRolesApplication.SLUG,
    name: "Member Role Managements",
    category: DigitalMarketingApplicationCategories.DIGITAL_MARKETING,
    icon: Icons.PermissionList,
    templateName: TemplateNames.SECTION_LAYOUT)]

namespace XperienceCommunity.MemberRoles.Admin.UIPages
{
    public class MemberRolesApplication : ApplicationPage
    {
        // Unique identifier of the application
        public const string IDENTIFIER = "XperienceCommunity.MemberRoles";
        public const string SLUG = "member-roles";
    }
}

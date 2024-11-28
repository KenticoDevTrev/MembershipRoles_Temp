using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages;
using XperienceCommunity.MemberRoles.Admin.UIPages;

[assembly: UIApplication(
    identifier: FolderPermissionsApplication.IDENTIFIER,
    type: typeof(FolderPermissionsApplication),
    slug: FolderPermissionsApplication.SLUG,
    name: "Folder Permissions",
    category: DigitalMarketingApplicationCategories.DIGITAL_MARKETING,
    icon: Icons.Folder,
    templateName: TemplateNames.SECTION_LAYOUT)]


namespace XperienceCommunity.MemberRoles.Admin.UIPages
{
    public class FolderPermissionsApplication : ApplicationPage
    {
        // Unique identifier of the application
        public const string IDENTIFIER = "XperienceCommunity.MemberRolesFolderPermissions";
        public const string SLUG = "content-folder-permissions";
    }
}
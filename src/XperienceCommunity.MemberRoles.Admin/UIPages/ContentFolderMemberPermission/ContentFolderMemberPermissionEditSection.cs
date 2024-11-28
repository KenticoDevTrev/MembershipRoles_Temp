using CMS.ContentEngine;
using Kentico.Xperience.Admin.Base;
using XperienceCommunity.MemberRoles.Admin.UIPages.ContentFolderMemberPermission;

[assembly: UIPage(
    parentType: typeof(ContentFolderMemberPermissionListing),
    slug: PageParameterConstants.PARAMETERIZED_SLUG,
    uiPageType: typeof(ContentFolderMemberPermissionEditSection),
    name: "Member permissions",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 300)]

namespace XperienceCommunity.MemberRoles.Admin.UIPages.ContentFolderMemberPermission
{
    public class ContentFolderMemberPermissionEditSection : EditSectionPage<ContentFolderInfo>
    {

    }
}

using CMS.ContentEngine;
using Kentico.Xperience.Admin.Base;
using XperienceCommunity.MemberRoles.Admin.UIPages.MemberRole;

[assembly: UIPage(
    parentType: typeof(MemberRoleListing),
    slug: PageParameterConstants.PARAMETERIZED_SLUG,
    uiPageType: typeof(MemberRoleEditSection),
    name: "Edit section for member roles objects",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 300)]

namespace XperienceCommunity.MemberRoles.Admin.UIPages.MemberRole
{
    public class MemberRoleEditSection : EditSectionPage<TagInfo>
    {

    }
}

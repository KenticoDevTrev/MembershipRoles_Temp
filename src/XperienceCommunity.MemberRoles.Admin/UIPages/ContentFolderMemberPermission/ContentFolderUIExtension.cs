using Kentico.Web.Mvc.Internal;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using XperienceCommunity.MemberRoles.Admin.ActionComponents;
using XperienceCommunity.MemberRoles.Admin.UIPages.ContentFolderMemberPermission;

[assembly: PageExtender(typeof(ContentFolderUIExtension))]


namespace XperienceCommunity.MemberRoles.Admin.UIPages.ContentFolderMemberPermission
{
    public class ContentFolderUIExtension(IAdminPathRetriever adminPathRetriever) : PageExtender<ContentHubList>
    {
        private readonly IAdminPathRetriever _adminPathRetriever = adminPathRetriever;

        public override Task ConfigurePage()
        {
            if (Page.FolderId.FolderType == FolderType.Content && Page.FolderId.Id > 0)
            {
                Page.PageConfiguration.HeaderActions.AddActionWithCustomComponent(new RedirectActionComponent()
                {
                    Properties = new RedirectActionComponentProperties($"/{_adminPathRetriever.GetAdminPrefix().Trim('/')}/{FolderPermissionsApplication.SLUG}/{ContentFolderMemberPermissionListing.SLUG}/{Page.FolderId.Id.Value}/{ContentFolderMemberSecurityPageTemplate.SLUG}")
                }, "Manage Member Permissions");
                
                /*
                // Couldn't get below to work due to error
                var folderIdParameter = new PageParameterValues() {
                    { typeof(ContentFolderModelBinder), Page.FolderId.Id.Value },
                };
                Page.PageConfiguration.HeaderActions.Add<ContentFolderMemberPermissionPageTemplate>("Manage Folder Permissions", parameters: folderIdParameter);

                */
            }

            return base.ConfigurePage();
        }
    }
}

using XperienceCommunity.MemberRoles.Models;

namespace XperienceCommunity.MemberRoles.Repositories
{
    public interface IMemberPermissionSummaryRepository
    {
        /// <summary>
        /// Primarily used for Admin UI (But can be used for current page context checking), Retrieves a summary of the current configuration for the given ContentItemID and Language
        /// </summary>
        /// <param name="contentItemId"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        Task<MemberRolePermissionSummary> GetMemberRolePermissionSummaryByContentItem(int contentItemId, string language);

        /// <summary>
        /// Used for Admin UI, retrieves a summary of the Content Folder's Permissions
        /// </summary>
        /// <param name="contentFolderId"></param>
        /// <returns></returns>
        Task<MemberRolePermissionSummary> GetMemberRolePermissionSummaryByContentFolder(int contentFolderId);

        /// <summary>
        /// Used for Admin UI, retrieves a summary of the Web Page Item's Permissions
        /// </summary>
        /// <param name="webPageItemId"></param>
        /// <returns></returns>
        Task<MemberRolePermissionSummary> GetMemberRolePermissionSummaryByWebPageItem(int webPageItemId);
    }
}

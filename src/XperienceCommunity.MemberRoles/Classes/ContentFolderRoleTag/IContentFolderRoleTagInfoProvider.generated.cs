using System.Threading;
using System.Threading.Tasks;

using CMS.DataEngine;

namespace XperienceCommunity.MemberRoles
{
    /// <summary>
    /// Declares members for <see cref="ContentFolderRoleTagInfo"/> management.
    /// </summary>
    public partial interface IContentFolderRoleTagInfoProvider : IInfoProvider<ContentFolderRoleTagInfo>
    {
        /// <summary>
        /// Gets an instance of the <see cref="ContentFolderRoleTagInfo"/> binding structure.
        /// </summary>
        /// <param name="contentfolderId">Content folder ID.</param>
        /// <param name="tagId">Tag ID.</param>
        /// <returns>Returns an instance of <see cref="ContentFolderRoleTagInfo"/> corresponding to given identifiers or null.</returns>
        ContentFolderRoleTagInfo Get(int contentfolderId, int tagId);


        /// <summary>
        /// Asynchronously gets an instance of the <see cref="ContentFolderRoleTagInfo"/> binding structure.
        /// </summary>
        /// <param name="contentfolderId">Content folder ID.</param>
        /// <param name="tagId">Tag ID.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>Returns a task returning either an instance of <see cref="ContentFolderRoleTagInfo"/> corresponding to given identifiers or null.</returns>
        Task<ContentFolderRoleTagInfo> GetAsync(int contentfolderId, int tagId, CancellationToken? cancellationToken = null);


        /// <summary>
        /// Deletes <see cref="ContentFolderRoleTagInfo"/> binding.
        /// </summary>
        /// <param name="contentfolderId">Content folder ID.</param>
        /// <param name="tagId">Tag ID.</param>
        void Remove(int contentfolderId, int tagId);


        /// <summary>
        /// Creates <see cref="ContentFolderRoleTagInfo"/> binding.
        /// </summary>
        /// <param name="contentfolderId">Content folder ID.</param>
        /// <param name="tagId">Tag ID.</param>
        void Add(int contentfolderId, int tagId);
    }
}
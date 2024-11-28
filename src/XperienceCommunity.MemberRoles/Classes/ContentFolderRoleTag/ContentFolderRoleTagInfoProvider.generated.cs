using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CMS.DataEngine;

namespace XperienceCommunity.MemberRoles
{
    /// <summary>
    /// Class providing <see cref="ContentFolderRoleTagInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IContentFolderRoleTagInfoProvider))]
    public partial class ContentFolderRoleTagInfoProvider : AbstractInfoProvider<ContentFolderRoleTagInfo, ContentFolderRoleTagInfoProvider>, IContentFolderRoleTagInfoProvider
    {
        /// <summary>
        /// Gets an instance of the <see cref="ContentFolderRoleTagInfo"/> binding structure.
        /// </summary>
        /// <param name="contentfolderId">Content folder ID.</param>
        /// <param name="tagId">Tag ID.</param>
        /// <returns>Returns an instance of <see cref="ContentFolderRoleTagInfo"/> corresponding to given identifiers or null.</returns>
        public virtual ContentFolderRoleTagInfo Get(int contentfolderId, int tagId)
        {
            return GetObjectQuery().TopN(1)
                .WhereEquals("ContentFolderRoleTagContentFolderID", contentfolderId)
                .WhereEquals("ContentFolderRoleTagTagID", tagId)
                .FirstOrDefault();
        }


        /// <summary>
        /// Asynchronously gets an instance of the <see cref="ContentFolderRoleTagInfo"/> binding structure.
        /// </summary>
        /// <param name="contentfolderId">Content folder ID.</param>
        /// <param name="tagId">Tag ID.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>Returns a task returning either an instance of <see cref="ContentFolderRoleTagInfo"/> corresponding to given identifiers or null.</returns>
        public async virtual Task<ContentFolderRoleTagInfo> GetAsync(int contentfolderId, int tagId, CancellationToken? cancellationToken = null)
        {
            var query = await GetObjectQuery().TopN(1)
                .WhereEquals("ContentFolderRoleTagContentFolderID", contentfolderId)
                .WhereEquals("ContentFolderRoleTagTagID", tagId)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return query.FirstOrDefault();
        }


        /// <summary>
        /// Deletes <see cref="ContentFolderRoleTagInfo"/> binding.
        /// </summary>
        /// <param name="contentfolderId">Content folder ID.</param>
        /// <param name="tagId">Tag ID.</param>
        public virtual void Remove(int contentfolderId, int tagId)
        {
            var infoObj = Get(contentfolderId, tagId);
            if (infoObj != null)
            {
                Delete(infoObj);
            }
        }


        /// <summary>
        /// Creates <see cref="ContentFolderRoleTagInfo"/> binding.
        /// </summary>
        /// <param name="contentfolderId">Content folder ID.</param>
        /// <param name="tagId">Tag ID.</param>
        public virtual void Add(int contentfolderId, int tagId)
        {
            // Create new binding
            var infoObj = new ContentFolderRoleTagInfo();
            infoObj.ContentFolderRoleTagContentFolderID = contentfolderId;
            infoObj.ContentFolderRoleTagTagID = tagId;

            // Save to the database
            Set(infoObj);
        }
    }
}
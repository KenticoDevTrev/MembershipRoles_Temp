using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CMS.DataEngine;

namespace XperienceCommunity.MemberRoles
{
    /// <summary>
    /// Class providing <see cref="WebPageItemRoleTagInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IWebPageItemRoleTagInfoProvider))]
    public partial class WebPageItemRoleTagInfoProvider : AbstractInfoProvider<WebPageItemRoleTagInfo, WebPageItemRoleTagInfoProvider>, IWebPageItemRoleTagInfoProvider
    {
        /// <summary>
        /// Gets an instance of the <see cref="WebPageItemRoleTagInfo"/> binding structure.
        /// </summary>
        /// <param name="webpageitemId">Web page item ID.</param>
        /// <param name="tagId">Tag ID.</param>
        /// <returns>Returns an instance of <see cref="WebPageItemRoleTagInfo"/> corresponding to given identifiers or null.</returns>
        public virtual WebPageItemRoleTagInfo Get(int webpageitemId, int tagId)
        {
            return GetObjectQuery().TopN(1)
                .WhereEquals("WebPageItemRoleTagWebPageItemID", webpageitemId)
                .WhereEquals("WebPageItemRoleTagTagID", tagId)
                .FirstOrDefault();
        }


        /// <summary>
        /// Asynchronously gets an instance of the <see cref="WebPageItemRoleTagInfo"/> binding structure.
        /// </summary>
        /// <param name="webpageitemId">Web page item ID.</param>
        /// <param name="tagId">Tag ID.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>Returns a task returning either an instance of <see cref="WebPageItemRoleTagInfo"/> corresponding to given identifiers or null.</returns>
        public async virtual Task<WebPageItemRoleTagInfo> GetAsync(int webpageitemId, int tagId, CancellationToken? cancellationToken = null)
        {
            var query = await GetObjectQuery().TopN(1)
                .WhereEquals("WebPageItemRoleTagWebPageItemID", webpageitemId)
                .WhereEquals("WebPageItemRoleTagTagID", tagId)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return query.FirstOrDefault();
        }


        /// <summary>
        /// Deletes <see cref="WebPageItemRoleTagInfo"/> binding.
        /// </summary>
        /// <param name="webpageitemId">Web page item ID.</param>
        /// <param name="tagId">Tag ID.</param>
        public virtual void Remove(int webpageitemId, int tagId)
        {
            var infoObj = Get(webpageitemId, tagId);
            if (infoObj != null)
            {
                Delete(infoObj);
            }
        }


        /// <summary>
        /// Creates <see cref="WebPageItemRoleTagInfo"/> binding.
        /// </summary>
        /// <param name="webpageitemId">Web page item ID.</param>
        /// <param name="tagId">Tag ID.</param>
        public virtual void Add(int webpageitemId, int tagId)
        {
            // Create new binding
            var infoObj = new WebPageItemRoleTagInfo();
            infoObj.WebPageItemRoleTagWebPageItemID = webpageitemId;
            infoObj.WebPageItemRoleTagTagID = tagId;

            // Save to the database
            Set(infoObj);
        }
    }
}
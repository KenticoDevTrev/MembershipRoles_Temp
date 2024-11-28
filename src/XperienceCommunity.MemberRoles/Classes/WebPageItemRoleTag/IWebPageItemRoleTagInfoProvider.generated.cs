using System.Threading;
using System.Threading.Tasks;

using CMS.DataEngine;

namespace XperienceCommunity.MemberRoles
{
    /// <summary>
    /// Declares members for <see cref="WebPageItemRoleTagInfo"/> management.
    /// </summary>
    public partial interface IWebPageItemRoleTagInfoProvider : IInfoProvider<WebPageItemRoleTagInfo>
    {
        /// <summary>
        /// Gets an instance of the <see cref="WebPageItemRoleTagInfo"/> binding structure.
        /// </summary>
        /// <param name="webpageitemId">Web page item ID.</param>
        /// <param name="tagId">Tag ID.</param>
        /// <returns>Returns an instance of <see cref="WebPageItemRoleTagInfo"/> corresponding to given identifiers or null.</returns>
        WebPageItemRoleTagInfo Get(int webpageitemId, int tagId);


        /// <summary>
        /// Asynchronously gets an instance of the <see cref="WebPageItemRoleTagInfo"/> binding structure.
        /// </summary>
        /// <param name="webpageitemId">Web page item ID.</param>
        /// <param name="tagId">Tag ID.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>Returns a task returning either an instance of <see cref="WebPageItemRoleTagInfo"/> corresponding to given identifiers or null.</returns>
        Task<WebPageItemRoleTagInfo> GetAsync(int webpageitemId, int tagId, CancellationToken? cancellationToken = null);


        /// <summary>
        /// Deletes <see cref="WebPageItemRoleTagInfo"/> binding.
        /// </summary>
        /// <param name="webpageitemId">Web page item ID.</param>
        /// <param name="tagId">Tag ID.</param>
        void Remove(int webpageitemId, int tagId);


        /// <summary>
        /// Creates <see cref="WebPageItemRoleTagInfo"/> binding.
        /// </summary>
        /// <param name="webpageitemId">Web page item ID.</param>
        /// <param name="tagId">Tag ID.</param>
        void Add(int webpageitemId, int tagId);
    }
}
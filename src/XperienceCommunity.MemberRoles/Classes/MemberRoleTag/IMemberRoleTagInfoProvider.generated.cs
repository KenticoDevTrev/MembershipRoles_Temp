using System.Threading;
using System.Threading.Tasks;

using CMS.DataEngine;

namespace XperienceCommunity.MemberRoles
{
    /// <summary>
    /// Declares members for <see cref="MemberRoleTagInfo"/> management.
    /// </summary>
    public partial interface IMemberRoleTagInfoProvider : IInfoProvider<MemberRoleTagInfo>
    {
        /// <summary>
        /// Gets an instance of the <see cref="MemberRoleTagInfo"/> binding structure.
        /// </summary>
        /// <param name="memberId">Member ID.</param>
        /// <param name="tagId">Tag ID.</param>
        /// <returns>Returns an instance of <see cref="MemberRoleTagInfo"/> corresponding to given identifiers or null.</returns>
        MemberRoleTagInfo Get(int memberId, int tagId);


        /// <summary>
        /// Asynchronously gets an instance of the <see cref="MemberRoleTagInfo"/> binding structure.
        /// </summary>
        /// <param name="memberId">Member ID.</param>
        /// <param name="tagId">Tag ID.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>Returns a task returning either an instance of <see cref="MemberRoleTagInfo"/> corresponding to given identifiers or null.</returns>
        Task<MemberRoleTagInfo> GetAsync(int memberId, int tagId, CancellationToken? cancellationToken = null);


        /// <summary>
        /// Deletes <see cref="MemberRoleTagInfo"/> binding.
        /// </summary>
        /// <param name="memberId">Member ID.</param>
        /// <param name="tagId">Tag ID.</param>
        void Remove(int memberId, int tagId);


        /// <summary>
        /// Creates <see cref="MemberRoleTagInfo"/> binding.
        /// </summary>
        /// <param name="memberId">Member ID.</param>
        /// <param name="tagId">Tag ID.</param>
        void Add(int memberId, int tagId);
    }
}
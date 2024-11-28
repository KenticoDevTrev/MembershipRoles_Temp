using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CMS.DataEngine;

namespace XperienceCommunity.MemberRoles
{
    /// <summary>
    /// Class providing <see cref="MemberRoleTagInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IMemberRoleTagInfoProvider))]
    public partial class MemberRoleTagInfoProvider : AbstractInfoProvider<MemberRoleTagInfo, MemberRoleTagInfoProvider>, IMemberRoleTagInfoProvider
    {
        /// <summary>
        /// Gets an instance of the <see cref="MemberRoleTagInfo"/> binding structure.
        /// </summary>
        /// <param name="memberId">Member ID.</param>
        /// <param name="tagId">Tag ID.</param>
        /// <returns>Returns an instance of <see cref="MemberRoleTagInfo"/> corresponding to given identifiers or null.</returns>
        public virtual MemberRoleTagInfo Get(int memberId, int tagId)
        {
            return GetObjectQuery().TopN(1)
                .WhereEquals("MemberRoleTagMemberID", memberId)
                .WhereEquals("MemberRoleTagTagID", tagId)
                .FirstOrDefault();
        }


        /// <summary>
        /// Asynchronously gets an instance of the <see cref="MemberRoleTagInfo"/> binding structure.
        /// </summary>
        /// <param name="memberId">Member ID.</param>
        /// <param name="tagId">Tag ID.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>Returns a task returning either an instance of <see cref="MemberRoleTagInfo"/> corresponding to given identifiers or null.</returns>
        public async virtual Task<MemberRoleTagInfo> GetAsync(int memberId, int tagId, CancellationToken? cancellationToken = null)
        {
            var query = await GetObjectQuery().TopN(1)
                .WhereEquals("MemberRoleTagMemberID", memberId)
                .WhereEquals("MemberRoleTagTagID", tagId)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return query.FirstOrDefault();
        }


        /// <summary>
        /// Deletes <see cref="MemberRoleTagInfo"/> binding.
        /// </summary>
        /// <param name="memberId">Member ID.</param>
        /// <param name="tagId">Tag ID.</param>
        public virtual void Remove(int memberId, int tagId)
        {
            var infoObj = Get(memberId, tagId);
            if (infoObj != null)
            {
                Delete(infoObj);
            }
        }


        /// <summary>
        /// Creates <see cref="MemberRoleTagInfo"/> binding.
        /// </summary>
        /// <param name="memberId">Member ID.</param>
        /// <param name="tagId">Tag ID.</param>
        public virtual void Add(int memberId, int tagId)
        {
            // Create new binding
            var infoObj = new MemberRoleTagInfo();
            infoObj.MemberRoleTagMemberID = memberId;
            infoObj.MemberRoleTagTagID = tagId;

            // Save to the database
            Set(infoObj);
        }
    }
}
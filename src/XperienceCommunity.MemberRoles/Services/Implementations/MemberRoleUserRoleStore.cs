using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using Kentico.Membership;
using Microsoft.AspNetCore.Identity;

namespace XperienceCommunity.MemberRoles.Services.Implementations
{
    public class MemberRoleUserRoleStore<TUser>(IInfoProvider<MemberRoleTagInfo> memberRoleTagInfoProvider,
        IInfoProvider<TagInfo> tagInfoProvider,
        IProgressiveCache progressiveCache,
        IMemberInfoProvider memberInfoProvider,
        IUserLoginStore<TUser> userLoginStore
        ) : IUserRoleStore<TUser> where TUser : ApplicationUser, new()
    {
        private readonly IInfoProvider<MemberRoleTagInfo> _memberRoleTagInfoProvider = memberRoleTagInfoProvider;
        private readonly IInfoProvider<TagInfo> _tagInfoProvider = tagInfoProvider;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IMemberInfoProvider _memberInfoProvider = memberInfoProvider;
        private readonly IUserLoginStore<TUser> _userLoginStore = userLoginStore;
        private bool _disposed;

        #region "IUserRoleStore"

        private async Task<Dictionary<string, int>> GetRoleTagNameToTagID(CancellationToken cancellationToken)
        {
            return await _progressiveCache.LoadAsync(async (cs, cancellationToken) => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{TagInfo.OBJECT_TYPE}|all");
                }
                return (await _tagInfoProvider.Get()
                .Source(x => x.InnerJoin<TaxonomyInfo>(nameof(TagInfo.TagTaxonomyID), nameof(TaxonomyInfo.TaxonomyID)))
                .WhereEquals(nameof(TaxonomyInfo.TaxonomyName), MemberRoleConstants._MemberRoleTaxonomy)
                .Columns(nameof(TagInfo.TagName), nameof(TagInfo.TagID))
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
                .GroupBy(x => x.TagName.ToLowerInvariant())
                .ToDictionary(key => key.Key, value => value.First().TagID);
            }, new CacheSettings(30, "MemberRoles_RoleTagNameToTagID"), cancellationToken: cancellationToken);
        }

        private async Task<MemberInfo?> ApplicationUserToMember(TUser user, CancellationToken cancellationToken)
        {
            // Kentico's Member Store sets the user.Id to the MemberId if present.
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{MemberInfo.OBJECT_TYPE}|byid|{user.Id}");
                }

                return (await _memberInfoProvider.Get()
                .WhereEquals(nameof(MemberInfo.MemberID), user.Id)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken)
                ).FirstOrDefault();
            }, new CacheSettings(20, "ApplicationuserToMember", user.Id));

        }

        public async Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            var member = await ApplicationUserToMember(user, cancellationToken) ?? throw new ArgumentException("No Member Found with Id " + user.Id);

            var roles = await GetRoleTagNameToTagID(cancellationToken);
            if(!roles.TryGetValue(roleName.ToLowerInvariant(), out var roleTagId)) {
                throw new ArgumentException($"There is no Member role of name {roleName}, please add prior to assigning.");
            }
            // see if they are on it or not.
            var memberRoleTag = (await _memberRoleTagInfoProvider.Get()
                .WhereEquals(nameof(MemberRoleTagInfo.MemberRoleTagMemberID), member.MemberID)
                .WhereEquals(nameof(MemberRoleTagInfo.MemberRoleTagTagID), roleTagId)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken)).FirstOrDefault();
            if (memberRoleTag == null) {
                memberRoleTag = new MemberRoleTagInfo() {
                    MemberRoleTagMemberID = member.MemberID,
                    MemberRoleTagTagID = roleTagId
                };
                _memberRoleTagInfoProvider.Set(memberRoleTag);
            }
        }

        public async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
        {
            var member = await ApplicationUserToMember(user, cancellationToken) ?? throw new ArgumentException("No Member Found with Id " + user.Id);

            // Kentico's Member Store sets the user.Id to the MemberId if present.
            return await _progressiveCache.LoadAsync(async(cs, cancellationToken) => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{MemberRoleTagInfo.OBJECT_TYPE}|all");
                }

                return (await _tagInfoProvider.Get()
                .Source(x => x.InnerJoin<MemberRoleTagInfo>(nameof(TagInfo.TagID), nameof(MemberRoleTagInfo.MemberRoleTagTagID)))
                .Source(x => x.InnerJoin<MemberInfo>(nameof(MemberRoleTagInfo.MemberRoleTagMemberID), nameof(MemberInfo.MemberID)))
                .WhereEquals(nameof(MemberInfo.MemberID), user.Id)
                .Columns(nameof(TagInfo.TagName))
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken)
                ).Select(x => x.TagName).ToList();
            }, new CacheSettings(20, "MemberRoleUserStore_GetRolesAsync", user.Id), cancellationToken);
        }

        public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var roles = await GetRoleTagNameToTagID(cancellationToken);
            if (!roles.TryGetValue(roleName.ToLowerInvariant(), out var roleTagId)) {
                return [];
            }

            var members = await _progressiveCache.LoadAsync(async (cs, cancellationToken) => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{MemberRoleTagInfo.OBJECT_TYPE}|all");
                }

                return (await _memberInfoProvider.Get()
                .Source(x => x.InnerJoin<MemberRoleTagInfo>(nameof(MemberInfo.MemberID), nameof(MemberRoleTagInfo.MemberRoleTagMemberID)))
                .WhereEquals(nameof(MemberRoleTagInfo.MemberRoleTagTagID), roleTagId)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken)
                );
            }, new CacheSettings(20, "MemberRoleUserStore_GetUsersInRoleAsync", roleTagId), cancellationToken: cancellationToken);
                
            // Map using the MapFromMemberInfo as that's overridable by user
            return members.Select(x => {
                var user = new ApplicationUser();
                user.MapFromMemberInfo(x);
                return (TUser)user;
            }).ToList();
        }

        public async Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            var roles = await GetRoleTagNameToTagID(cancellationToken);

            if(!roles.TryGetValue(roleName.ToLowerInvariant(), out var roleTagId)) {
                return false;
            }

            return await _progressiveCache.LoadAsync(async (cs, cancellationToken) => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{MemberRoleTagInfo.OBJECT_TYPE}|all");
                }

                return (await _memberRoleTagInfoProvider.Get()
                .WhereEquals(nameof(MemberRoleTagInfo.MemberRoleTagTagID), roleTagId)
                .WhereEquals(nameof(MemberRoleTagInfo.MemberRoleTagMemberID), user.Id)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken)
                ).Any();
            }, new CacheSettings(20, "MemberRoleUserStore_IsInRoleAsync", roleName, user.Id), cancellationToken: cancellationToken);
        }

        public async Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            var member = await ApplicationUserToMember(user, cancellationToken);

            var roles = await GetRoleTagNameToTagID(cancellationToken);

            // no need to throw an exception as if the user or role doesn't exist, then removing is successful
            if (member == null || !roles.TryGetValue(roleName.ToLowerInvariant(), out var roleTagId)) {
                return;
            }
            // see if they are on it or not.
            var memberRoleTag = (await _memberRoleTagInfoProvider.Get()
                .WhereEquals(nameof(MemberRoleTagInfo.MemberRoleTagMemberID), member.MemberID)
                .WhereEquals(nameof(MemberRoleTagInfo.MemberRoleTagTagID), roleTagId)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken)).FirstOrDefault();

            if (memberRoleTag != null) {
                _memberRoleTagInfoProvider.Delete(memberRoleTag);
            }
        }

        #endregion

        #region "Kentico Implemented These Already in their IUserLoginStore"

        public Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken) => _userLoginStore.CreateAsync(user, cancellationToken);

        public Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken) => _userLoginStore.DeleteAsync(user, cancellationToken);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this); 
        }

        public Task<TUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) => _userLoginStore.FindByIdAsync(userId, cancellationToken);

        public Task<TUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => _userLoginStore.FindByNameAsync(normalizedUserName, cancellationToken);

        public Task<string?> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken) => _userLoginStore.GetNormalizedUserNameAsync(user, cancellationToken);

        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken) => _userLoginStore.GetUserIdAsync(user, cancellationToken);

        public Task<string?> GetUserNameAsync(TUser user, CancellationToken cancellationToken) => _userLoginStore.GetUserNameAsync(user, cancellationToken);

        public Task SetNormalizedUserNameAsync(TUser user, string? normalizedName, CancellationToken cancellationToken) => _userLoginStore.SetNormalizedUserNameAsync(user, normalizedName, cancellationToken);

        public Task SetUserNameAsync(TUser user, string? userName, CancellationToken cancellationToken) => _userLoginStore.SetUserNameAsync(user, userName, cancellationToken);

        public Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken) => _userLoginStore.UpdateAsync(user, cancellationToken);

        #endregion

        #region "Dispose Logic"

        /// <summary>
        /// Disposes the user store.
        /// </summary>
        /// <param name="disposing">Describes whether or not should the managed resources be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            _disposed = true;
        }

        protected void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, GetType().Name);
        }


        #endregion
    }
}

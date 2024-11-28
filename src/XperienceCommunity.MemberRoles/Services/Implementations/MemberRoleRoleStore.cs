using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using Microsoft.AspNetCore.Identity;
using XperienceCommunity.MemberRoles.Models;

namespace XperienceCommunity.MemberRoles.Services.Implementations
{
    public class MemberRoleRoleStore<TRole>(
        IInfoProvider<TagInfo> tagInfoProvider,
        IProgressiveCache progressiveCache,
        IInfoProvider<TaxonomyInfo> taxonomyInfoProvider) : IRoleStore<TRole> where TRole : TagApplicationUserRole, new()
    {
        private readonly IInfoProvider<TagInfo> _tagInfoProvider = tagInfoProvider;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IInfoProvider<TaxonomyInfo> _taxonomyInfoProvider = taxonomyInfoProvider;
        private bool _disposed;

        public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
        {
            var roleTags = await GetRoleTags(cancellationToken);

            if (roleTags.Any(x => x.TagID == role.Id || x.TagName.Equals(role.Name, StringComparison.OrdinalIgnoreCase) || x.TagName.Equals(role.NormalizedName, StringComparison.OrdinalIgnoreCase))) {
                // Already exists
                return IdentityResult.Failed(new IdentityError() { Code = "ROLE_EXISTS", Description = "Role Already Exists by that name" });
            }
            // Create;
            var roleTag = new TagInfo() {
                TagName = role.NormalizedName,
                TagTitle = role.Name,
                TagTaxonomyID = await GetRoleTagTaxonomyId(cancellationToken),
                TagOrder = roleTags.Count() + 1,
                TagMetadata = "{\"Translations\":{}}"
            };
            _tagInfoProvider.Set(roleTag);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
        {
            var roleTags = await GetRoleTags(cancellationToken);
            var foundTag = roleTags.FirstOrDefault(x => x.TagID == role.Id || x.TagName.Equals(role.Name, StringComparison.OrdinalIgnoreCase) || x.TagName.Equals(role.NormalizedName, StringComparison.OrdinalIgnoreCase));
            if (foundTag == null) {
                // Couldn't find
                return IdentityResult.Failed(new IdentityError() { Code = "ROLE_DOESNTEXIST", Description = "Role by that Id and/or name was not found, cannot delete" });
            }
            _tagInfoProvider.Delete(foundTag);
            return IdentityResult.Success;
        }

        public async Task<TRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            var roleTags = await GetRoleTags(cancellationToken);
            if (int.TryParse(roleId, out var roleIdVal)) {
                var foundTag = roleTags.FirstOrDefault(x => x.TagID == roleIdVal);
                if (foundTag != null) {
                    return TagToRole(foundTag);
                }
            }
            return null;
        }

        public async Task<TRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            var roleTags = await GetRoleTags(cancellationToken);
            var foundTag = roleTags.FirstOrDefault(x => x.TagName.Equals(normalizedRoleName, StringComparison.OrdinalIgnoreCase));
            return foundTag != null ? TagToRole(foundTag) : null;
        }

        public async Task<string?> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            return (await FindMatchingTag(role, cancellationToken))?.TagName.Normalize();
        }

        public async Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
        {
            return (await FindMatchingTag(role, cancellationToken))?.TagID.ToString() ?? string.Empty;
        }

        public async Task<string?> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            return (await FindMatchingTag(role, cancellationToken))?.TagName;
        }

        public async Task SetNormalizedRoleNameAsync(TRole role, string? normalizedName, CancellationToken cancellationToken)
        {
            var foundRole = await FindMatchingTag(role, cancellationToken);
            if(foundRole != null && !foundRole.TagName.Equals(normalizedName, StringComparison.OrdinalIgnoreCase)) {
                foundRole.TagName = normalizedName;
                _tagInfoProvider.Set(foundRole);
            }
        }

        public async Task SetRoleNameAsync(TRole role, string? roleName, CancellationToken cancellationToken)
        {
            var foundRole = await FindMatchingTag(role, cancellationToken);
            if (foundRole != null && !foundRole.TagName.Equals(roleName, StringComparison.OrdinalIgnoreCase)) {
                foundRole.TagName = roleName;
                _tagInfoProvider.Set(foundRole);
            }
        }

        public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
        {
            var foundRole = await FindMatchingTag(role, cancellationToken);
            if (foundRole != null &&
                (
                    !foundRole.TagName.Equals(role.Name, StringComparison.OrdinalIgnoreCase) 
                    || !foundRole.TagLastModified.ToString().Equals(role.ConcurrencyStamp, StringComparison.OrdinalIgnoreCase)
                )
                ){
                foundRole.TagName = role.Name;
                foundRole.TagTitle = role.Name;
                _tagInfoProvider.Set(foundRole);
                return IdentityResult.Success;
            }
            return IdentityResult.Failed(new IdentityError() { Code = "ROLE_NOCHANGE", Description = "No change detected" });
        }

        private async Task<TagInfo?> FindMatchingTag(TRole role, CancellationToken cancellationToken)
        {
            var roleTags = await GetRoleTags(cancellationToken);
            return roleTags.FirstOrDefault(x => x.TagID == role.Id || x.TagName.Equals(role.Name, StringComparison.OrdinalIgnoreCase) || x.TagName.Equals(role.NormalizedName, StringComparison.OrdinalIgnoreCase));
        }

        private TRole TagToRole(TagInfo tag)
        {
            var role = new TRole();
            role.SetRoleFromTag<TRole>(tag);
            return role;
        }

        private async Task<IEnumerable<TagInfo>> GetRoleTags(CancellationToken cancellationToken)
        {
            return await _progressiveCache.LoadAsync(async (cs, cancellationToken) => {

                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{TagInfo.OBJECT_TYPE}|all");
                }

                return await _tagInfoProvider.Get()
                    .Source(x => x.InnerJoin<TaxonomyInfo>(nameof(TagInfo.TagTaxonomyID), nameof(TaxonomyInfo.TaxonomyID)))
                    .WhereEquals(nameof(TaxonomyInfo.TaxonomyName), MemberRoleConstants._MemberRoleTaxonomy)
                    .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

            }, new CacheSettings(60, "MemberRoleRolesStore_GetRoleTags"), cancellationToken: cancellationToken);
        }

        private async Task<int> GetRoleTagTaxonomyId(CancellationToken cancellationToken)
        {
            return await _progressiveCache.LoadAsync(async (cs, cancellationToken) => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{TaxonomyInfo.OBJECT_TYPE}|all");
                }
                return (await _taxonomyInfoProvider.GetAsync(MemberRoleConstants._MemberRoleTaxonomy, cancellationToken))?.TaxonomyID ?? 0;
            }, new CacheSettings(60, "MemberRoleRolesStore_GetRoleTagTaxonomyID"), cancellationToken: cancellationToken);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

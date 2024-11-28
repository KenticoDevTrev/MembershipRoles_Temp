namespace XperienceCommunity.MemberRoles.Models
{
    /// <summary>
    /// This interface can be used for custom objects when passing through the IMemberAuthorizationFilter.  Implementing this will give the filter the required information to determine access
    /// </summary>
    public interface IMemberPermissionConfiguration : IPermissionConfigurationBase
    {
        /// <summary>
        /// The ContentItem ID of the object
        /// </summary>
        /// <returns></returns>
        public int GetContentID();

        /// <summary>
		/// If permissions should be checked at all (usually true if you are using this interface)
		/// </summary>
		public bool GetCheckPermissions();


        /// <summary>
		/// Usually derived from the MemberPermissionsOverride field on the CMS_ContentItemCommonData, If the Permission is overwritten on this content item (don't check inheritance).  If this is true, should set the GetMemberPermissionIsSecure and GetMemberPermissionRoleTags. 
		/// </summary>
		public bool GetMemberPermissionOverride();


        /// <summary>
        /// Usually derived from the MemberPermissionsIsSecure field on the CMS_ContentItemCommonData, If the Item requires Authentication.  Only checked if GetMemberPermissionOverride() is true.
        /// </summary>
        public bool GetMemberPermissionIsSecure();


        /// <summary>
        /// Usually derived from the MemberPermissionRoleTags field on the CMS_ContentItemCommonData (and converted to the tag names), determines what roles can access this item, none = no role check. Only checked if GetMemberPermissionOverride() is true.
        /// </summary>
        public IEnumerable<string> GetMemberPermissionRoleTags();

    }
}

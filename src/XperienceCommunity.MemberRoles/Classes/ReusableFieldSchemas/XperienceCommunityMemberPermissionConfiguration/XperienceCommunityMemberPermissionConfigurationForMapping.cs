using CMS.ContentEngine;

namespace XperienceCommunity.MemberRoles
{
    /// <summary>
    /// Used only as a "Fake" content type for use with the The IContentItemQueryResultMapper to be able to "map" any source that inherits the member permissions configuration and retrieve.
    /// </summary>
    [RegisterContentTypeMapping(CONTENT_TYPE_NAME)]
    public partial class XperienceCommunityMemberPermissionConfigurationConfigurationForMapping : IContentItemFieldsSource, IXperienceCommunityMemberPermissionConfiguration
    {
        /// <summary>
        /// Code name of the content type.
        /// </summary>
        public const string CONTENT_TYPE_NAME = "XperienceCommunity.MemberPermissionConfiguration.ForMapping";


        /// <summary>
        /// Represents system properties for a web page item.
        /// </summary>
        [SystemField]
        public ContentItemFields SystemFields { get; set; } = new ContentItemFields();

        /// <summary>
		/// MemberPermissionOverride.
		/// </summary>
		public bool MemberPermissionOverride { get; set; }


        /// <summary>
        /// MemberPermissionIsSecure.
        /// </summary>
        public bool MemberPermissionIsSecure { get; set; }


        /// <summary>
        /// MemberPermissionRoleTags.
        /// </summary>
        public IEnumerable<TagReference> MemberPermissionRoleTags { get; set; } = [];
    }
}

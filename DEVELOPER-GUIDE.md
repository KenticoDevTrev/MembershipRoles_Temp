# Developer Guide

In this guide, we will be covering how you can leverage the Member Roles system in code, and how to perform filtering on item(s) given the current member context.

## Installation
Please see the main [Read Me](README.md) for installation.

## Nuget Package Split
The XperienceCommunity.MemberRoles package is split up into 3 individual packges, with different Xperience Dependencies.  This is listed out on the  [Read Me](README.md).

To go a little further into details, you may want to install the `XperienceCommunity.MemberRoles.Core` (which is Kentico Agnostic) so you have access to these items on your generic DTOs:

**IMemberPermissionConfiguration**: Any model inheriting this interface will also be filterable through the `IMemberAuthorizationFilter.RemoveUnauthorizedItems` methods.
**IMemberAuthenticationContext**: This DI service allows a simplified user context for filtering, giving you the 3 core pieces of information you often need for things like Route Filtering (Authenticated, Username, and Roles)

## IXperienceCommunityMemberPermissionConfiguration Reusable Schema
The application of this reusable schema to a Content Type is the indicator that this content type should be filtered based on the current member context.

Once this reusable schema is applied to a Content Type in the Xperience Admin UI, it is important to [regenerate the page type code](https://docs.kentico.com/developers-and-admins/api/generate-code-files-for-system-objects#generate-code-files) so the compiler knows that your model has the appropriate fields to check permissions.

## Filtering 
When filtering a list of items, you should retrieve your items into a list, then pass that list to `IMemberAuthorizationFilter.RemoveUnauthorizedItems`.

This will do a type check on each item to see if it inherits the `IPermissionConfigurationBase` (which both the `IXperienceCommunityMemberPermissionConfiguration` and `IMemberPermissionConfiguration` inherit).  This is why it's important to cast your objects on the retrieval properly!

If it inherits, it will perform the necessary logic to check permissions against the current member context, if not, it will just return it as if it had no permissions.

Here are two examples of retrieving individual items, and multiple items:

```csharp
// For single type query, use the .IncludeMemberAuthorization() to ensure the columns are returned that are needed for parsing.
var singleTypeQuery = new ContentItemQueryBuilder().ForContentType(BasicPage.CONTENT_TYPE_NAME, query => 
    query.Columns(nameof(BasicPage.PageName))
         .IncludeMemberAuthorization() // This ensures the right columns are returned!!
);
var itemsSingle = await _contentQueryExecutor.GetMappedWebPageResult<BasicPage>(singleTypeQuery);
var filteredItemsSingleList = await _memberAuthorizationFilter.RemoveUnauthorizedItems(itemsSingle);
```

```csharp
// For Multi Type Querys, the Reusable Field Schema is returned in the data by default
var multiTypeQuery = new ContentItemQueryBuilder().ForContentTypes(parameters =>
    parameters
        // Basic Page and Web Page inherit the IXperienceCommunityMemberPermissionConfiguration, Navigation does NOT
        .OfContentType(BasicPage.CONTENT_TYPE_NAME, WebPage.CONTENT_TYPE_NAME, Navigation.CONTENT_TYPE_NAME)
        .WithContentTypeFields()
    );
// Returning a list of Objects as we have different types in here, some BasicPage, some WebPage, some Navigation
var itemsMultiType = await _contentQueryExecutor.GetResult<object>(multiTypeQuery, (selector) => {
    // Important to use the _ContentQueryResultMapper to map your items so the IMemberAuthorizationFilter.RemoveUnauthorizedItems can do type checking
    if (selector.ContentTypeName.Equals(BasicPage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase)) {
        return _contentQueryResultMapper.Map<BasicPage>(selector);
    } else if(selector.ContentTypeName.Equals(WebPage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase)) {
        return _contentQueryResultMapper.Map<WebPage>(selector);
    }
    return selector as IContentItemFieldsSource;
});
var filteredItemsMultiList = await _memberAuthorizationFilter.RemoveUnauthorizedItems(itemsMultiType);
```

## Member Context and Member Role Management in Code
The Member Roles is integrated with the Microsoft Identity system.  You can inject IUserStore, IUserRoleStore, and IRoleStore to get and manipulate roles.

Here is a code snippet of some testing I did myself, the system properly created the Role Taxonomy Tags, assigned, etc:

```csharp
public class TestController(
    IContentQueryExecutor contentQueryExecutor,
    IMemberAuthorizationFilter memberAuthorizationFilter,
    IContentQueryResultMapper contentQueryResultMapper,
    IMemberAuthenticationContext memberAuthenticationContext,
    IRoleStore<TagApplicationUserRole> roleStore,
    IUserRoleStore<ApplicationUser> userRoleStore,
    IUserStore<ApplicationUser> userStore
    ) : Controller
{
    private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
    private readonly IMemberAuthorizationFilter _memberAuthorizationFilter = memberAuthorizationFilter;
    private readonly IContentQueryResultMapper _contentQueryResultMapper = contentQueryResultMapper;
    private readonly IMemberAuthenticationContext _memberAuthenticationContext = memberAuthenticationContext;
    private readonly IRoleStore<TagApplicationUserRole> _roleStore = roleStore;
    private readonly IUserRoleStore<ApplicationUser> _userRoleStore = userRoleStore;
    private readonly IUserStore<ApplicationUser> _userStore = userStore;

    public async Task<string> Index()
    {
        // Get Member
        var testMember = await _userStore.FindByNameAsync("TestMember", CancellationToken.None);

        // Get Roles
        var roles = await _userRoleStore.GetRolesAsync(testMember, CancellationToken.None);

        // Create Roles
        var studentRoleStatus = await _roleStore.CreateAsync(new TagApplicationUserRole() {
            Name = "TestNewRole",
            NormalizedName = "testnewrole"
        }, CancellationToken.None);

        // Assign
        if(studentRoleStatus.Succeeded) {
            var newRole = await _roleStore.FindByNameAsync("TestNewRole", CancellationToken.None);
            if(newRole != null) {
                await _userRoleStore.AddToRoleAsync(testMember, "students", CancellationToken.None);
            }
        } else {
            // Remove
            var newRole = await _roleStore.FindByNameAsync("TestNewRole", CancellationToken.None);
            if (newRole != null) {
                await _userRoleStore.AddToRoleAsync(testMember, newRole.Name, CancellationToken.None);
            }
        }

        // Get current user context
        var currentUser = await _memberAuthenticationContext.GetAuthenticationContext();
      return string.Empty;
    }
}
```

## Route Handling
As mentioned in the main [Read Me](README.md), this system does not leverage the Web Page Permissions in routing requests.

I will be working on the updated version of the [XperienceCommunity.Authorization](https://github.com/KenticoDevTrev/KenticoAuthorization#xperiencecommunityauthorization) package (probably wil be renamed XperienceCommunity.DevTools.Authorization) shortly that will then leverage these to introduce Controller Based and Tree Routing Based request filtering, tying into Page Permissions.

## Optimization and Cache Dependencies
All code methods called should be highly optimized and usable as is (Caching is already applied appropriately).  

It is recommended that you apply filtering outside of any IProgressiveCache retrieval of content, OR if you do want to filter within, please consider adding these 4 dependency keys to your caching:

```
$"{WebPageItemMemberPermissionSettingInfo.OBJECT_TYPE}|all",
$"{WebPageItemRoleTagInfo.OBJECT_TYPE}|all",
$"{ContentFolderMemberPermissionSettingInfo.OBJECT_TYPE}|all",
$"{ContentFolderRoleTagInfo.OBJECT_TYPE}|all",
```

Also found as a constant ReadOnlyCollection<string> in `MemberRoleConstants._InheritanceCacheDependencies`

These will cover any possible inheritance changes, and in all likelyhood won't change often.  

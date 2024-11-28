
# XperienceCommunity.MemberRoles

## Description

This is a community created package to allow Member Roles and permissions in Xperience by Kentico (until this logic is baked into the product).

Roles can be created, assigned to Members, and Content Items can be secured with Authentication and Member Role Permissions applied to themselves or inherited from a parent Content Folders and Web Page Items.

## Library Version Matrix

This project is using [Xperience Version v29.7.0](https://docs.kentico.com/changelog#refresh-november-14-2024).

| Xperience Version | Library Version |
| ----------------- | --------------- |
| >= 29.7.*         | 1.0.0           |

## Package Installation

Add the package to your application using the .NET CLI
```powershell
dotnet add package XperienceCommunity.MemberRoles.Admin
```

Additionally, you can elect to install only the required packages on specific projects if you have separation of concerns:

**XperienceCommunity.MemberRoles.Core** : No Xperience Dependencies
**XperienceCommunity.MemberRoles**: Kentico.Xperience.WebApp Dependent (No Admin)
**XperienceCommunity.MemberRoles.Admin** : Kentico.Xperience.Admin (Admin Items)

## Quick Start
In your startup, when you call the `.AddIdentity<TUser,TRole>` ...

1. Use `TagApplicationUserRole` as the `TRole`  (still use Kentico's `Applicationuser` as `TUser`)
2. Call the extension `.AddMemberRolesStores<TUser, TRole>()`  off your `IdentityBuilder`

This will hook up all the interfaces (including `IUserRoleStore`, and `IRoleStore`) and run the installation logic on application run.  Below is the basic Kentico Authentication Hookup with Member Roles.

``` csharp
public static void AddStandardKenticoAuthentication(WebApplicationBuilder builder)
{
    // Adds Basic Kentico Authentication, needed for user context and some tools
    builder.Services.AddAuthentication();

    // XperienceCommunity.MemberRoles, make sure Role is TagApplicationUserRole or an inherited member here
    builder.Services.AddIdentity<ApplicationUser, TagApplicationUserRole>(options => {
        // Ensures that disabled member accounts cannot sign in
        options.SignIn.RequireConfirmedAccount = true;
        // Ensures unique emails for registered accounts
        options.User.RequireUniqueEmail = true;
    })
        .AddUserStore<ApplicationUserStore<ApplicationUser>>()
        .AddMemberRolesStores<ApplicationUser, TagApplicationUserRole>() // XperienceCommunity.MemberRoles
        .AddUserManager<UserManager<ApplicationUser>>()
        .AddSignInManager<SignInManager<ApplicationUser>>();

    // Adds authorization support to the app
    builder.Services.AddAuthorization();
}
```

## Using Web Page Route Security

While this package includes the ability to create and assign Roles to Users, to define Content Item Permissions, and Interfaces that can be used to determine access and filter out items, it does not have the systems to automatically apply Web Page Security rules for a page request.

I will be working on the updated version of the [XperienceCommunity.Authorization](https://github.com/KenticoDevTrev/KenticoAuthorization#xperiencecommunityauthorization) package (probably wil be renamed `XperienceCommunity.DevTools.Authorization`) shortly that will then leverage these to introduce Controller Based and Tree Routing Based request filtering, tying into Page Permissions.  

## Screenshots
[TODO]

## Guide
Please see the [Admin Guide](ADMIN-GUIDE.md) for Xperience Users or the [Developer Guide](DEVELOPER_GUIDE.md) for instructions.

## Contributing

Please feel free to create a pull request if you find any bugs.  If you are on the Xperience Community Slack, that's the best place to hit me up so I get eyes on it.

## License

Distributed under the MIT License. See [`LICENSE.md`](./LICENSE.md) for more
information.

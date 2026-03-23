
# XperienceCommunity.MemberRoles

## OBSOLETE / Migration

Xperience by Kentico's March 26 Refresh (31.3.0) now contains Membership Roles.  

For those who have used this package, please know that the capabilities are nearly one to one.  The three major differences are:

1. There is no Membership Roles on Content Folders (that then apply to all children).  These roles are on individual Content Items, but you CAN apply roles in mass easily (select all items in the folder and apply a role)
2. Similarly for Web Page Items, there isn't a role you set on the parent that always applies to children, but you CAN apply role changes to all children under a web page when you make changes.  Just be careful of cases where you 'break' inheritance on a child, because re-applying a role change on the parent will reset inheritance to the parent for any children.
3. Although less used, the ability to apply membership roles on ANY C# object (via an interface) is also not part of the Xperience version...honestly I would be surprised if many people used this.

I would encourage you to migrate to the Built in package as long as it's possible.  My version of the Membership Roles is still there if you need it, but I myself will be migrating off at some point.

Additionally for Baseline Users, the baseline will not be updated for this refresh yet.  I plan (or hope to) build an Xperience only Baseline project on .net 10 once things slow down, probably another Christmas present launch.

Xperience may be creating a migration script, but it's not guarenteed.  Please let me know if you need help with migrating, I won't be migrating off until later.

## Description

This is a community created package to allow Member Roles and permissions in Xperience by Kentico (until this logic is baked into the product).

Roles can be created, assigned to Members, and Content Items can be secured with Authentication and Member Role Permissions applied to themselves or inherited from a parent Content Folders and Web Page Items.

## Library Version Matrix

This project is using [Xperience Version v31.0.0](https://docs.kentico.com/changelog).

| Xperience Version  | Library Version |
| ------------------ | --------------- |
| >= 31.0.*          | 2.6.0           |
|    30.9.0-30.12.3  | 2.5.0           |
|    30.6.*          | 2.4.0           |
|    30.0.0-30.5.4   | 2.0.0-2.3.2     |
|    29.7.*          | 1.0.0           |

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

## Guide
Please see the [Admin Guide](ADMIN-GUIDE.md) for Xperience Users or the [Developer Guide](DEVELOPER-GUIDE.md) for instructions.

## Using Web Page Route Security

While this package includes the ability to create and assign Roles to Users, to define Content Item Permissions, and Interfaces that can be used to determine access and filter out items, it does not have the systems to automatically apply Web Page Security rules for a page request.

I will be working on the updated version of the [XperienceCommunity.Authorization](https://github.com/KenticoDevTrev/KenticoAuthorization#xperiencecommunityauthorization) package (probably wil be renamed `XperienceCommunity.DevTools.Authorization`) shortly that will then leverage these to introduce Controller Based and Tree Routing Based request filtering, tying into Page Permissions.  

## Screenshots
![image](https://github.com/user-attachments/assets/2d83fdc5-0431-4222-b4b4-241680669e13)
![image](https://github.com/user-attachments/assets/4b97fd41-2420-4356-9be2-2c2379bc23b4)
![image](https://github.com/user-attachments/assets/c5a5e61f-1c65-4d19-b20a-2df8d62069a3)
![image](https://github.com/user-attachments/assets/396aec30-0d6c-4fef-86b6-17201b4009df)
![image](https://github.com/user-attachments/assets/723de39c-7038-453a-be95-cd93f4f2684c)
![image](https://github.com/user-attachments/assets/d489263b-69d6-4620-8684-bf4118593500)
![image](https://github.com/user-attachments/assets/39521bad-aa45-459c-9773-06c14b41febc)
![image](https://github.com/user-attachments/assets/556ccfc6-649b-4d14-91bd-63c6f672f6a8)
![image](https://github.com/user-attachments/assets/a6b8740a-5d83-4679-9bf0-3ef8cdc2e990)
![image](https://github.com/user-attachments/assets/5937ffe9-387f-4aed-9b6a-2ce444de5910)

## Existing "Is Secure" on Content Items
Kentico released the "Is Secure" on Content Items (Properties -> Membership (Web Channel) or just Properties (Content Hub)) which was intended to mark items as needing Member Authentication to accesses.  I was not able to leverage this field, so for all intents and purposes, this field is ignored by Member Roles in it's filtering.  The new UIs have their own "Is Secure" field which you should use.

## Contributing

Please feel free to create a pull request if you find any bugs.  If you are on the Xperience Community Slack, that's the best place to hit me up so I get eyes on it.

## License

Distributed under the MIT License. See [`LICENSE.md`](./LICENSE.md) for more
information.

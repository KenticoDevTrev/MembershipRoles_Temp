# Admin Guide
This document outlines how to use the Member Roles system within the Xperience Admin.

## Role Management
For Members, Roles are Taxonomy Tags under the Taxonomy `MemberRoles`.

To manage Member Roles, use the `Member Role Management` Application under `Digital Marketing`

![image](https://github.com/user-attachments/assets/6aac4e15-e46a-4507-9d03-d68ecd3b6a19)

### Add Roles
Click the `Add Role Tag` in the `Member Role Management` Application.  This will bring you to the normal Taxonomies Tag handler under the Member Roles Taxonomy.
![image](https://github.com/user-attachments/assets/bcc483e2-70b0-4d6d-9373-ad598af45286)
![image](https://github.com/user-attachments/assets/bfc776a9-0088-4708-9484-3cb1a3b79f76)

After you add roles, you will need to go back `Home` and go to the `Member Roles` and click on the new role tag to assign members.

### Add/Remove Members to Role
In the `Member Role Management` Application, click on the role you wish to manage.  This will bring up the `Assigned members` binding screen where you can click `Add Members` to add, or the `X` to remove them.

![image](https://github.com/user-attachments/assets/55176532-25ba-46d4-9f66-7b2d0dbaac94)

### Add/Remove Roles to Member
Likewise, you can manage roles on the member.  Go to the `Member` Application under `Digital Marketing`, select your member, and click on the `Roles` option on the side bar.

![image](https://github.com/user-attachments/assets/7a933321-3e84-4554-9fad-c18b9b532ded)
![image](https://github.com/user-attachments/assets/cf6b0ab8-1a9f-4e66-97bc-5c8f2c7b28ed)

## Permissions
The Permissions interfaces will allow you to designate Web Pages and Content Items as requiring special permissions in order to access.

**NOTE**: While the Admin interfaces allow you to manage these permissions, it is up to the developers to leverage these to actually secure your content on your website.

## Designating a Content Item Type for Permissions
In this system, you must opt-into permissions on your content type.  To do this, you must do the following:

1. Go to `Configuration` - `Content Types`
2. Select the Content Type you wish to add Permission Logic to
3. Click `Add Reusable Field Schema`
4. Select the `Member Permission Configuration` and Confirm
5. Have your developers re-generate the Content Type's model (so the model shows it inherits this type)

![image](https://github.com/user-attachments/assets/1e0fd974-3801-4a52-a8ec-c0c9b4d0cfae)
![image](https://github.com/user-attachments/assets/d7bb9320-3d9b-46f2-ba0b-ee6166c95edc)

Note that even if you do not designate a Web Page Item's Content Type as using Permissions, you can still set Web Page Permissions for inheritance purposes.

## Inheriting vs. Overriding and Version/Language
When a Content Type is designated as having permissions, It takes the default posture of **inheriting** permissions.

You can override this behavior in the `Content Tab` by checking `Override Inherited Member Permissions`.  

Overriding is a property of the Content Item's Common Data which means it is **language and version specific and subject to workflows**.  When Xperience releases the Content Syncing feature, these changes should sync across environments.
![image](https://github.com/user-attachments/assets/70af815d-5fb5-4e06-9860-c7648ffb2c2c)

If the content item is not overriding the permissions, it will inherit from the nearest specified permission on the `Web Page` or `Content Folder` hierarchy.

Web Page Items and Content Folders are **language and version agnostic and NOT subject to workflows**, any changes take affect immediately.  When Xperience releases the Content Syncing feature, these settings may need to be manually replicated across environments.

**Permission Flow**
1. Is the current item's `Override Inherited Member Permissions` true?  Use the `Is Secure`, `Member Roles` that are on the `Content` Tab
2. If not Overriden and the Content Item is on a Web Channel, look at it's Web Page Item Security (Properties - Member Permissions) and any ancestor until it finds one with 'Break Inheritance` checked and use those permissions
3. If not Overriden and the Content Item is in the Content Hub, look at it's containing Content Folder's Item Security (Folder Permissions) and any ancestor until it finds one with 'Break Inheritance` checked and use those permissions 
4. If no ancestors found, then assume full access (no permission requirements)

## Web Page Item Security
To add Security on a Web Page (that will impact any descendent that doesn't **Break Inheritance**), do the following:

1. Go into your `Web` Channel
2. Click on any page
3. Go to the Properties Tab on the right
4. Under the `Member Permission` accordion, check `Break Inheritance`, configure and hit save.

![image](https://github.com/user-attachments/assets/44bc5527-7dbf-4bd7-b436-823c0e4a0597)


## Content Folder Security
To add Security on Content Folders (that will impact any descendent Content Folder that doesn't **Break Inheritance**, and any content items within them), you can either:

1. Go to the Content Hub
2. Add or Select the Folder
3. Click on `Manage Member Permissions`
4. Check `Break Inheritance`, configure and hit save.

OR

1. Go to `Folder Permissions` App
2. Select your Folder
3. Check `Break Inheritance`, configure and hit save.

![image](https://github.com/user-attachments/assets/b14a0cfe-ef16-4733-94d9-4ab7a55b07d9)
![image](https://github.com/user-attachments/assets/81fdef08-ff44-459b-8459-ebebf70324ba)

using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.Modules;
using MemberInfo = CMS.Membership.MemberInfo;

namespace XperienceCommunity.MemberRoles
{
    public class MemberRolesInstaller(IInfoProvider<ResourceInfo> resourceInfoProvider,
        IInfoProvider<TaxonomyInfo> taxonomyInfoProvider)
    {
        private readonly IInfoProvider<ResourceInfo> _resourceInfoProvider = resourceInfoProvider;
        private readonly IInfoProvider<TaxonomyInfo> _taxonomyInfoProvider = taxonomyInfoProvider;

        public void Install()
        {
            var resource = _resourceInfoProvider.Get("XperienceCommunity.MemberRoles") ?? new ResourceInfo();
            InitializeMemberRolesResource(resource);
            InitializeMemberRoleTaxonomy();
            InitializeModuleClasses(resource);
            InitializeReusableSchema();
        }

        private void InitializeMemberRolesResource(ResourceInfo resource)
        {
            resource.ResourceDisplayName = "XperienceCommunity.MemberRoles";
            resource.ResourceName = "XperienceCommunity.MemberRoles";
            resource.ResourceDescription = "Temporary Membership Extension, these classes are part of XperienceCommunity.MemberRoles and will be replaced by Kentico once assimilated.";
            resource.ResourceIsInDevelopment = false;

            if (resource.HasChanged) {
                _resourceInfoProvider.Set(resource);
            }
        }

        private void InitializeMemberRoleTaxonomy()
        {
            var memberRoleTaxonomy = _taxonomyInfoProvider.Get(MemberRoleConstants._MemberRoleTaxonomy) ?? new TaxonomyInfo();
            memberRoleTaxonomy.TaxonomyName = MemberRoleConstants._MemberRoleTaxonomy;
            memberRoleTaxonomy.TaxonomyGUID = Guid.Parse(MemberRoleConstants._MemberRoleTaxonomyGuid);
            memberRoleTaxonomy.TaxonomyTitle = "Member Roles";
            memberRoleTaxonomy.TaxonomyDescription = "Roles for Member Permissions";
            if(memberRoleTaxonomy.HasChanged) {
                _taxonomyInfoProvider.Set(memberRoleTaxonomy);
            }
        }

        private void InitializeModuleClasses(ResourceInfo resource)
        {
            var dataClassesByName = DataClassInfoProvider.GetClasses().WhereIn(nameof(DataClassInfo.ClassName), 
                    ["XperienceCommunity.ContentFolderMemberPermissionSetting", "XperienceCommunity.ContentFolderRoleTag", "XperienceCommunity.MemberRoleTag", "XperienceCommunity.WebPageItemMemberPermissionSetting", "XperienceCommunity.WebPageItemRoleTag"]).GetEnumerableTypedResult()
                .ToDictionary(key => key.ClassName, value => value);
            
            var contentFolderMemberPermissionSettings = dataClassesByName.TryGetValue("XperienceCommunity.ContentFolderMemberPermissionSetting", out var item1) ? item1 : DataClassInfo.New("XperienceCommunity.ContentFolderMemberPermissionSetting");
            var contentFolderRoleTag = dataClassesByName.TryGetValue("XperienceCommunity.ContentFolderRoleTag", out var item2) ? item2 : DataClassInfo.New("XperienceCommunity.ContentFolderRoleTag");
            var memberRoleTag = dataClassesByName.TryGetValue("XperienceCommunity.MemberRoleTag", out var item3) ? item3 : DataClassInfo.New("XperienceCommunity.MemberRoleTag");
            var webPageItemMemberPermissionSetting = dataClassesByName.TryGetValue("XperienceCommunity.WebPageItemMemberPermissionSetting", out var item4) ? item4 : DataClassInfo.New("XperienceCommunity.WebPageItemMemberPermissionSetting");
            var webPageItemRoleTag = dataClassesByName.TryGetValue("XperienceCommunity.WebPageItemRoleTag", out var item5) ? item5 : DataClassInfo.New("XperienceCommunity.WebPageItemRoleTag");

            InitializeContentFolderMemberPermissionSettings(contentFolderMemberPermissionSettings, resource.ResourceID);
            InitializeContentFolderRoleTag(contentFolderRoleTag, resource.ResourceID);
            InitializeMemberRoleTag(memberRoleTag, resource.ResourceID);
            InitializeWebPageItemMemberPermissionSetting(webPageItemMemberPermissionSetting, resource.ResourceID);
            InitializeWebPageItemRoleTag(webPageItemRoleTag, resource.ResourceID);

        }

        private void InitializeMemberRoleTag(DataClassInfo info, int resourceId)
        {
            info.ClassDisplayName = "Member Role Tags";
            info.ClassName = "XperienceCommunity.MemberRoleTag";
            info.ClassTableName = "XperienceCommunity_MemberRoleTag";
            info.ClassResourceID = resourceId;
            info.ClassType = ClassType.OTHER;

            var formInfo = FormHelper.GetBasicFormDefinition(nameof(MemberRoleTagInfo.MemberRoleTagID));
            var formItem = new FormFieldInfo {
                Name = nameof(MemberRoleTagInfo.MemberRoleTagMemberID),
                AllowEmpty = false,
                Visible = true,
                Precision = 0,
                DataType = "integer",
                ReferenceType = ObjectDependencyEnum.Binding,
                ReferenceToObjectType = MemberInfo.OBJECT_TYPE,
                Enabled = true
            };
            formInfo.AddFormItem(formItem);
            formItem = new FormFieldInfo {
                Name = nameof(MemberRoleTagInfo.MemberRoleTagTagID),
                AllowEmpty = false,
                Visible = true,
                Precision = 0,
                DataType = "integer",
                ReferenceType = ObjectDependencyEnum.Binding,
                ReferenceToObjectType = TagInfo.OBJECT_TYPE,
                Enabled = true
            };
            formInfo.AddFormItem(formItem);

            SetFormDefinition(info, formInfo);

            if (info.HasChanged) {
                DataClassInfoProvider.SetDataClassInfo(info);
                try {
                    // run SQL to set foreign keys
                    var foreignKeySql =
    @"
IF(OBJECT_ID('FK_XperienceCommunity_MemberRoleTag_CMS_Member', 'F') IS NULL)
BEGIN
	ALTER TABLE [dbo].[XperienceCommunity_MemberRoleTag]  WITH CHECK ADD  CONSTRAINT [FK_XperienceCommunity_MemberRoleTag_CMS_Member] FOREIGN KEY([MemberRoleTagMemberID])
	REFERENCES [dbo].[CMS_Member] ([MemberID])
	ON UPDATE CASCADE
	ON DELETE CASCADE

	ALTER TABLE [dbo].[XperienceCommunity_MemberRoleTag] CHECK CONSTRAINT [FK_XperienceCommunity_MemberRoleTag_CMS_Member]
END

IF(OBJECT_ID('FK_XperienceCommunity_MemberRoleTag_CMS_Tag', 'F') IS NULL)
BEGIN
	ALTER TABLE [dbo].[XperienceCommunity_MemberRoleTag]  WITH CHECK ADD  CONSTRAINT [FK_XperienceCommunity_MemberRoleTag_CMS_Tag] FOREIGN KEY([MemberRoleTagTagID])
	REFERENCES [dbo].[CMS_Tag] ([TagID])
	ON UPDATE CASCADE
	ON DELETE CASCADE

	ALTER TABLE [dbo].[XperienceCommunity_MemberRoleTag] CHECK CONSTRAINT [FK_XperienceCommunity_MemberRoleTag_CMS_Tag]
END
";
                    ConnectionHelper.ExecuteNonQuery(foreignKeySql, [], QueryTypeEnum.SQLQuery);
                } catch (Exception ex) {
                    EventLogProvider.LogEvent(new EventLogInfo("E", "MemberRoles", "InitializeMemberRoleTag Error") {
                        Exception = ex
                    });
                }
            }
        }
 
        private void InitializeWebPageItemMemberPermissionSetting(DataClassInfo info, int resourceId)
        {
            info.ClassDisplayName = "Web Page Item Member Permission Settings";
            info.ClassName = "XperienceCommunity.WebPageItemMemberPermissionSetting";
            info.ClassTableName = "XperienceCommunity_WebPageItemMemberPermissionSetting";
            info.ClassResourceID = resourceId;
            info.ClassType = ClassType.OTHER;

            var formInfo = FormHelper.GetBasicFormDefinition(nameof(WebPageItemMemberPermissionSettingInfo.WebPageItemMemberPermissionSettingID));
            var formItem = new FormFieldInfo {
                Name = nameof(WebPageItemMemberPermissionSettingInfo.WebPageItemMemberPermissionSettingWebPageItemID),
                AllowEmpty = false,
                Visible = true,
                Precision = 0,
                DataType = "integer",
                ReferenceType = ObjectDependencyEnum.Required,
                ReferenceToObjectType = "cms.webpageitem",
                Enabled = true,
                DefaultValue = "0"
            };
            formInfo.AddFormItem(formItem);
            formItem = new FormFieldInfo {
                Name = nameof(WebPageItemMemberPermissionSettingInfo.WebPageItemMemberPermissionSettingBreakInheritance),
                AllowEmpty = false,
                Visible = true,
                Precision = 0,
                DataType = "boolean",
                Enabled = true
            };
            formInfo.AddFormItem(formItem);
            formItem = new FormFieldInfo {
                Name = nameof(WebPageItemMemberPermissionSettingInfo.WebPageItemMemberPermissionSettingIsSecured),
                AllowEmpty = false,
                Visible = true,
                Precision = 0,
                DataType = "boolean",
                Enabled = true
            };
            formInfo.AddFormItem(formItem);

            SetFormDefinition(info, formInfo);

            if (info.HasChanged) {
                DataClassInfoProvider.SetDataClassInfo(info);
                try {
                    // run SQL to set foreign keys
                    var foreignKeySql =
    @"
IF(OBJECT_ID('FK_XperienceCommunity_WebPageItemMemberPermissionSetting_CMS_WebPageItem', 'F') IS NULL)
BEGIN
	ALTER TABLE [dbo].[XperienceCommunity_WebPageItemMemberPermissionSetting]  WITH CHECK ADD  CONSTRAINT [FK_XperienceCommunity_WebPageItemMemberPermissionSetting_CMS_WebPageItem] FOREIGN KEY([WebPageItemMemberPermissionSettingWebPageItemID])
	REFERENCES [dbo].[CMS_WebPageItem] ([WebPageItemID])
	ON UPDATE CASCADE
	ON DELETE CASCADE


	ALTER TABLE [dbo].[XperienceCommunity_WebPageItemMemberPermissionSetting] CHECK CONSTRAINT [FK_XperienceCommunity_WebPageItemMemberPermissionSetting_CMS_WebPageItem]
END
";
                    ConnectionHelper.ExecuteNonQuery(foreignKeySql, [], QueryTypeEnum.SQLQuery);
                } catch (Exception ex) {
                    EventLogProvider.LogEvent(new EventLogInfo("E", "MemberRoles", "InitializeWebPageItemMemberPermissionSetting Error") {
                        Exception = ex
                    });
                }
            }
        }

        private void InitializeWebPageItemRoleTag(DataClassInfo info, int resourceId)
        {
            info.ClassDisplayName = "Web Page Item Role Tags";
            info.ClassName = "XperienceCommunity.WebPageItemRoleTag";
            info.ClassTableName = "XperienceCommunity_WebPageItemRoleTag";
            info.ClassResourceID = resourceId;
            info.ClassType = ClassType.OTHER;

            var formInfo = FormHelper.GetBasicFormDefinition(nameof(WebPageItemRoleTagInfo.WebPageItemRoleTagID));
            var formItem = new FormFieldInfo {
                Name = nameof(WebPageItemRoleTagInfo.WebPageItemRoleTagWebPageItemID),
                AllowEmpty = false,
                Visible = true,
                Precision = 0,
                DataType = "integer",
                ReferenceType = ObjectDependencyEnum.Binding,
                ReferenceToObjectType = "cms.webpageitem",
                Enabled = true
            };
            formInfo.AddFormItem(formItem);
            formItem = new FormFieldInfo {
                Name = nameof(WebPageItemRoleTagInfo.WebPageItemRoleTagTagID),
                AllowEmpty = false,
                Visible = true,
                Precision = 0,
                DataType = "integer",
                ReferenceType = ObjectDependencyEnum.Binding,
                ReferenceToObjectType = TagInfo.OBJECT_TYPE,
                Enabled = true
            };
            formInfo.AddFormItem(formItem);

            SetFormDefinition(info, formInfo);

            if (info.HasChanged) {
                DataClassInfoProvider.SetDataClassInfo(info);
                try {
                    // run SQL to set foreign keys
                    var foreignKeySql =
    @"
IF(OBJECT_ID('FK_XperienceCommunity_WebPageItemRoleTag_CMS_WebPageItem', 'F') IS NULL)
BEGIN
	ALTER TABLE [dbo].[XperienceCommunity_WebPageItemRoleTag]  WITH CHECK ADD  CONSTRAINT [FK_XperienceCommunity_WebPageItemRoleTag_CMS_WebPageItem] FOREIGN KEY([WebPageItemRoleTagWebPageItemID])
	REFERENCES [dbo].[CMS_WebPageItem] ([WebPageItemID])
	ON UPDATE CASCADE
	ON DELETE CASCADE

	ALTER TABLE [dbo].[XperienceCommunity_WebPageItemRoleTag] CHECK CONSTRAINT [FK_XperienceCommunity_WebPageItemRoleTag_CMS_WebPageItem]
END

IF(OBJECT_ID('FK_XperienceCommunity_WebPageItemRoleTag_CMS_Tag', 'F') IS NULL)
BEGIN
	ALTER TABLE [dbo].[XperienceCommunity_WebPageItemRoleTag]  WITH CHECK ADD  CONSTRAINT [FK_XperienceCommunity_WebPageItemRoleTag_CMS_Tag] FOREIGN KEY([WebPageItemRoleTagTagID])
	REFERENCES [dbo].[CMS_Tag] ([TagID])
	ON UPDATE CASCADE
	ON DELETE CASCADE

	ALTER TABLE [dbo].[XperienceCommunity_WebPageItemRoleTag] CHECK CONSTRAINT [FK_XperienceCommunity_WebPageItemRoleTag_CMS_Tag]
END
";
                    ConnectionHelper.ExecuteNonQuery(foreignKeySql, [], QueryTypeEnum.SQLQuery);
                } catch (Exception ex) {
                    EventLogProvider.LogEvent(new EventLogInfo("E", "RelationshipsExtended", "InitializeContentItemCategory Error") {
                        Exception = ex
                    });
                }
            }
        }

        private void InitializeContentFolderMemberPermissionSettings(DataClassInfo info, int resourceId)
        {
            info.ClassDisplayName = "Content Folder Member Permission Settings";
            info.ClassName = "XperienceCommunity.ContentFolderMemberPermissionSetting";
            info.ClassTableName = "XperienceCommunity_ContentFolderMemberPermissionSetting";
            info.ClassResourceID = resourceId;
            info.ClassType = ClassType.OTHER;

            var formInfo = FormHelper.GetBasicFormDefinition(nameof(ContentFolderMemberPermissionSettingInfo.ContentFolderMemberPermissionSettingID));
            var formItem = new FormFieldInfo {
                Name = nameof(ContentFolderMemberPermissionSettingInfo.ContentFolderMemberPermissionContentFolderID),
                AllowEmpty = false,
                Visible = true,
                Precision = 0,
                DataType = "integer",
                ReferenceType = ObjectDependencyEnum.Required,
                ReferenceToObjectType = ContentFolderInfo.OBJECT_TYPE,
                Enabled = true,
                DefaultValue = "0"
            };
            formInfo.AddFormItem(formItem);
            formItem = new FormFieldInfo {
                Name = nameof(ContentFolderMemberPermissionSettingInfo.ContentFolderMemberPermissionSettingBreakInheritance),
                AllowEmpty = false,
                Visible = true,
                Precision = 0,
                DataType = "boolean",
                Enabled = true
            };
            formInfo.AddFormItem(formItem);
            formItem = new FormFieldInfo {
                Name = nameof(ContentFolderMemberPermissionSettingInfo.ContentFolderMemberPermissionSettingIsSecured),
                AllowEmpty = false,
                Visible = true,
                Precision = 0,
                DataType = "boolean",
                Enabled = true
            };
            formInfo.AddFormItem(formItem);

            SetFormDefinition(info, formInfo);

            if (info.HasChanged) {
                DataClassInfoProvider.SetDataClassInfo(info);
                try {
                    // run SQL to set foreign keys
                    var foreignKeySql =
    @"
IF(OBJECT_ID('FK_XperienceCommunity_ContentFolderMemberPermissionSetting_CMS_ContentFolder', 'F') IS NULL)
BEGIN

ALTER TABLE [dbo].[XperienceCommunity_ContentFolderMemberPermissionSetting]  WITH CHECK ADD  CONSTRAINT [FK_XperienceCommunity_ContentFolderMemberPermissionSetting_CMS_ContentFolder] FOREIGN KEY([ContentFolderMemberPermissionContentFolderID])
REFERENCES [dbo].[CMS_ContentFolder] ([ContentFolderID])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[XperienceCommunity_ContentFolderMemberPermissionSetting] CHECK CONSTRAINT [FK_XperienceCommunity_ContentFolderMemberPermissionSetting_CMS_ContentFolder]
END
";
                    ConnectionHelper.ExecuteNonQuery(foreignKeySql, [], QueryTypeEnum.SQLQuery);
                } catch (Exception ex) {
                    EventLogProvider.LogEvent(new EventLogInfo("E", "MemberRoles", "InitializeContentFolderMemberPermissionSettings Error") {
                        Exception = ex
                    });
                }
            }
        }

        private void InitializeContentFolderRoleTag(DataClassInfo info, int resourceId)
        {
            info.ClassDisplayName = "Content Folder Role Tags";
            info.ClassName = "XperienceCommunity.ContentFolderRoleTag";
            info.ClassTableName = "XperienceCommunity_ContentFolderRoleTag";
            info.ClassResourceID = resourceId;
            info.ClassType = ClassType.OTHER;

            var formInfo = FormHelper.GetBasicFormDefinition(nameof(ContentFolderRoleTagInfo.ContentFolderRoleTagID));
            var formItem = new FormFieldInfo {
                Name = nameof(ContentFolderRoleTagInfo.ContentFolderRoleTagContentFolderID),
                AllowEmpty = false,
                Visible = true,
                Precision = 0,
                DataType = "integer",
                ReferenceType = ObjectDependencyEnum.Binding,
                ReferenceToObjectType = ContentFolderInfo.OBJECT_TYPE,
                Enabled = true
            };
            formInfo.AddFormItem(formItem);
            formItem = new FormFieldInfo {
                Name = nameof(ContentFolderRoleTagInfo.ContentFolderRoleTagTagID),
                AllowEmpty = false,
                Visible = true,
                Precision = 0,
                DataType = "integer",
                ReferenceType = ObjectDependencyEnum.Binding,
                ReferenceToObjectType = TagInfo.OBJECT_TYPE,
                Enabled = true
            };
            formInfo.AddFormItem(formItem);
            SetFormDefinition(info, formInfo);

            if (info.HasChanged) {
                DataClassInfoProvider.SetDataClassInfo(info);
                try {
                    // run SQL to set foreign keys
                    var foreignKeySql =
    @"
IF(OBJECT_ID('FK_XperienceCommunity_ContentFolderRoleTag_CMS_ContentFolder', 'F') IS NULL)
BEGIN
	ALTER TABLE [dbo].[XperienceCommunity_ContentFolderRoleTag]  WITH CHECK ADD  CONSTRAINT [FK_XperienceCommunity_ContentFolderRoleTag_CMS_ContentFolder] FOREIGN KEY([ContentFolderRoleTagContentFolderID])
	REFERENCES [dbo].[CMS_ContentFolder] ([ContentFolderID])
	ON UPDATE CASCADE
	ON DELETE CASCADE

	ALTER TABLE [dbo].[XperienceCommunity_ContentFolderRoleTag] CHECK CONSTRAINT [FK_XperienceCommunity_ContentFolderRoleTag_CMS_ContentFolder]
END


IF(OBJECT_ID('FK_XperienceCommunity_ContentFolderRoleTag_CMS_Tag', 'F') IS NULL)
BEGIN
	ALTER TABLE [dbo].[XperienceCommunity_ContentFolderRoleTag]  WITH CHECK ADD  CONSTRAINT [FK_XperienceCommunity_ContentFolderRoleTag_CMS_Tag] FOREIGN KEY([ContentFolderRoleTagTagID])
	REFERENCES [dbo].[CMS_Tag] ([TagID])
	ON UPDATE CASCADE
	ON DELETE CASCADE


	ALTER TABLE [dbo].[XperienceCommunity_ContentFolderRoleTag] CHECK CONSTRAINT [FK_XperienceCommunity_ContentFolderRoleTag_CMS_Tag]
END

";
                    ConnectionHelper.ExecuteNonQuery(foreignKeySql, [], QueryTypeEnum.SQLQuery);
                } catch (Exception ex) {
                    EventLogProvider.LogEvent(new EventLogInfo("E", "MemberRoles", "InitializeContentFolderRoleTag Error") {
                        Exception = ex
                    });
                }
            }
        }

        private void InitializeReusableSchema()
        {
            var contentItemCommonData = DataClassInfoProvider.GetClasses().WhereEquals(nameof(DataClassInfo.ClassName), ContentItemCommonDataInfo.OBJECT_TYPE).FirstOrDefault() ?? throw new Exception("No Content Item Common Data Class Found, you got bigger problems than installing Member Roles!");
            var contentItemCommonDataForm = FormHelper.GetFormInfo(ContentItemCommonDataInfo.OBJECT_TYPE, false);
            var schemaGuid = Guid.Parse("d4ac5928-d389-4c43-9e15-8923c11b760e");

            // Add Schema
            var schema = contentItemCommonDataForm.GetFormSchema("XperienceCommunity.MemberPermissionConfiguration");
            if (schema == null) {
                contentItemCommonDataForm.ItemsList.Add(new FormSchemaInfo() {
                    Name = "XperienceCommunity.MemberPermissionConfiguration",
                    Description = @"This controls the Membership Permission Configuration for the given Content Item.
If 'Override Inherited Member Permissions' is checked, it will honor the individual setting, otherwise (default) it will inherit the permissions found on the Web Page Item and Content Folders, respecting any breaks in inheritance.

If custom Member Permissions are assigned, these will only impact this Content Item Language, and will not be used in inheritance.",
                    Caption = "Member Permission Configuration",
                    Guid = schemaGuid
                });
            }


            // Add or Update Override Field
            var existingoverriedField = contentItemCommonDataForm.GetFormField(nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionOverride));
            var overrideField = existingoverriedField ?? new FormFieldInfo();
            overrideField.Name = nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionOverride);
            overrideField.AllowEmpty = true;
            overrideField.Precision = 0;
            overrideField.DataType = "boolean";
            overrideField.Enabled = true;
            overrideField.Visible = true;
            overrideField.SetComponentName("XperienceCommunity.MemberRoles.CheckboxWithPermissionsSummary");

            overrideField.SetPropertyValue(FormFieldPropertyEnum.DefaultValue, "False");
            overrideField.SetPropertyValue(FormFieldPropertyEnum.ExplanationText, "If checked, will honor the settings on here. Unchecked (default) will use any inherited permissions.");
            overrideField.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            overrideField.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Override Inherited Member Permissions");
            overrideField.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            overrideField.Properties["kxp_schema_identifier"] = schemaGuid.ToString().ToLower();

            if (existingoverriedField != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionOverride), overrideField);
            } else {
                contentItemCommonDataForm.AddFormItem(overrideField);
            }

            // Add or Update IsSecure Field
            var existingIsSecureField = contentItemCommonDataForm.GetFormField(nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionIsSecure));
            var isSecureField = existingIsSecureField ?? new FormFieldInfo();
            isSecureField.Name = nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionIsSecure);
            isSecureField.AllowEmpty = true;
            isSecureField.Precision = 0;
            isSecureField.DataType = "boolean";
            isSecureField.Enabled = true;
            isSecureField.Visible = true;
            isSecureField.SetComponentName("Kentico.Administration.Checkbox");

            isSecureField.SetPropertyValue(FormFieldPropertyEnum.DefaultValue, "False");
            isSecureField.SetPropertyValue(FormFieldPropertyEnum.ExplanationText, "Only applicable if you are not Inheriting Member Permissions");
            isSecureField.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            isSecureField.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Is Secure (Requires Authentication)");
            isSecureField.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            isSecureField.Properties["kxp_schema_identifier"] = schemaGuid.ToString().ToLower();

            if (existingIsSecureField != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionIsSecure), isSecureField);
            } else {
                contentItemCommonDataForm.AddFormItem(isSecureField);
            }

            // Add or Update Role Tags Field
            var existingRoleTagsField = contentItemCommonDataForm.GetFormField(nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionRoleTags));
            var roleTagField = existingRoleTagsField ?? new FormFieldInfo();
            roleTagField.Name = nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionRoleTags);
            roleTagField.AllowEmpty = true;
            roleTagField.Precision = 0;
            roleTagField.DataType = "taxonomy";
            roleTagField.Enabled = true;
            roleTagField.Visible = true;
            roleTagField.SetComponentName("Kentico.Administration.TagSelector");
            roleTagField.Settings["TaxonomyGroup"] = $"[\"{MemberRoleConstants._MemberRoleTaxonomyGuid}\"]";

            roleTagField.SetPropertyValue(FormFieldPropertyEnum.ExplanationText, "Only applicable if you are overriding Member Permissions");
            roleTagField.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            roleTagField.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Member Roles");
            roleTagField.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            roleTagField.Properties["kxp_schema_identifier"] = schemaGuid.ToString().ToLower();

            if (existingRoleTagsField != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionRoleTags), roleTagField);
            } else {
                contentItemCommonDataForm.AddFormItem(roleTagField);
            }
            contentItemCommonData.ClassFormDefinition = contentItemCommonDataForm.GetXmlDefinition();

            if (contentItemCommonData.HasChanged) {
                DataClassInfoProvider.SetDataClassInfo(contentItemCommonData);
            }
        }

        private static void SetFormDefinition(DataClassInfo info, FormInfo form)
        {
            if (info.ClassID > 0) {
                var existingForm = new FormInfo(info.ClassFormDefinition);
                existingForm.CombineWithForm(form, new());
                info.ClassFormDefinition = existingForm.GetXmlDefinition();
            } else {
                info.ClassFormDefinition = form.GetXmlDefinition();
            }
        }
    }
}

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using XperienceCommunity.MemberRoles.Admin.ActionComponents;
using XperienceCommunity.MemberRoles.Admin.Extensions;
using XperienceCommunity.MemberRoles.Admin.UIPages.ContentFolderMemberPermission;
using XperienceCommunity.MemberRoles.Models;
using XperienceCommunity.MemberRoles.Repositories;

[assembly: RegisterFormComponent(CheckboxWithPermissionComponent.IDENTIFIER, typeof(CheckboxWithPermissionComponent), "Checkbox with Member Role Permission Summary")]

namespace XperienceCommunity.MemberRoles.Admin.ActionComponents
{
    public class CheckboxWithPermissionComponent(IMemberPermissionSummaryRepository memberPermissionSummaryRepository) : FormComponent<CheckboxWithMemberRoleSummaryClientProperties, bool>
    {
        public const string IDENTIFIER = "XperienceCommunity.MemberRoles.CheckboxWithPermissionsSummary";
        private readonly IMemberPermissionSummaryRepository _memberPermissionSummaryRepository = memberPermissionSummaryRepository;

        public override string ClientComponentName => "@memberroles/web-admin/CheckboxWithPermission";

        protected override async Task ConfigureClientProperties(CheckboxWithMemberRoleSummaryClientProperties clientProperties)
        {
#pragma warning disable CS0618 // Type or member is obsolete - The one it currently uses is internal so have to use this
            if (FormContext is IContentItemFormContext context) {
                var contentItemId = context.ItemId;
                var language = context.LanguageName;
                clientProperties.MemberRolePermissionSummary = (await _memberPermissionSummaryRepository.GetMemberRolePermissionSummaryByContentItem(contentItemId, language)).ToClientProperties();
            }
#pragma warning restore CS0618 // Type or member is obsolete

            await base.ConfigureClientProperties(clientProperties);
        }

        [FormComponentCommand]
        public async Task<ICommandResponse<MemberRolesPermissionSummaryClientProperties>> RefreshPermissions()
        {
            if (FormContext is IContentItemFormContext context) {
                var contentItemId = context.ItemId;
                var language = context.LanguageName;
                return ResponseFrom((await _memberPermissionSummaryRepository.GetMemberRolePermissionSummaryByContentItem(contentItemId, language)).ToClientProperties());
            }
            return ResponseFrom(new MemberRolesPermissionSummaryClientProperties());
        }
    }

    public class CheckboxWithMemberRoleSummaryClientProperties : CheckBoxClientProperties
    {
        public MemberRolesPermissionSummaryClientProperties MemberRolePermissionSummary { get; set; } = new MemberRolesPermissionSummaryClientProperties();
    }

    public class MemberRolesPermissionSummaryClientProperties
    {
        public MemberRolesPermissionSummaryClientProperties()
        {

        }

        public MemberRolesPermissionSummaryClientProperties(string inheritingFrom, bool requiresAuthentication, string[] memberRoles, string editLink)
        {
            InheritingFrom = inheritingFrom;
            RequiresAuthentication = requiresAuthentication;
            MemberRoles = memberRoles;
            EditLink = editLink;
        }

        public string InheritingFrom { get; set; } = "No Inheritance";
        public bool RequiresAuthentication { get; set; } = false;
        public string[] MemberRoles { get; set; } = [];
        public string EditLink { get; } = "";
    }
}

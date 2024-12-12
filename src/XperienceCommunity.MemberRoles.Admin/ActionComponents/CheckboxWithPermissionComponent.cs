using Kentico.Web.Mvc.Internal;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using XperienceCommunity.MemberRoles.Admin.ActionComponents;
using XperienceCommunity.MemberRoles.Admin.Extensions;
using XperienceCommunity.MemberRoles.Repositories;

[assembly: RegisterFormComponent(CheckboxWithPermissionComponent.IDENTIFIER, typeof(CheckboxWithPermissionComponent), "Checkbox with Member Role Permission Summary")]

namespace XperienceCommunity.MemberRoles.Admin.ActionComponents
{
    public class CheckboxWithPermissionComponent(IMemberPermissionSummaryRepository memberPermissionSummaryRepository, IAdminPathRetriever adminPathRetriever) : FormComponent<CheckboxWithMemberRoleSummaryClientProperties, bool>
    {
        public const string IDENTIFIER = "XperienceCommunity.MemberRoles.CheckboxWithPermissionsSummary";
        private readonly IMemberPermissionSummaryRepository _memberPermissionSummaryRepository = memberPermissionSummaryRepository;
        private readonly IAdminPathRetriever _adminPathRetriever = adminPathRetriever;

        public override string ClientComponentName => "@memberroles/web-admin/CheckboxWithPermission";

        protected override async Task ConfigureClientProperties(CheckboxWithMemberRoleSummaryClientProperties clientProperties)
        {
            if (FormContext != null) {
                var itemIdProp = FormContext.GetType().GetProperties().FirstOrDefault(x => x.Name == "ItemId");
                var langProp = FormContext.GetType().GetProperties().FirstOrDefault(x => x.Name == "LanguageName");
                if (itemIdProp != null && langProp != null) {
                    var contentItemId = (int)(itemIdProp.GetValue(FormContext) ?? 0);
                    var language = (string)(langProp.GetValue(FormContext) ?? "");
                    clientProperties.MemberRolePermissionSummary = (await _memberPermissionSummaryRepository.GetMemberRolePermissionSummaryByContentItem(contentItemId, language)).ToClientProperties(_adminPathRetriever.GetAdminPrefix());
                }
            }

            await base.ConfigureClientProperties(clientProperties);
        }

        [FormComponentCommand]
        public async Task<ICommandResponse<MemberRolesPermissionSummaryClientProperties>> RefreshPermissions()
        {
            if(FormContext != null) {
                var itemIdProp = FormContext.GetType().GetProperties().FirstOrDefault(x => x.Name == "ItemId");
                var langProp = FormContext.GetType().GetProperties().FirstOrDefault(x => x.Name == "LanguageName");
                if(itemIdProp != null && langProp != null) {
                    var contentItemId = (int)(itemIdProp.GetValue(FormContext) ?? 0);
                    var language = (string)(langProp.GetValue(FormContext) ?? "");
                    return ResponseFrom((await _memberPermissionSummaryRepository.GetMemberRolePermissionSummaryByContentItem(contentItemId, language)).ToClientProperties(_adminPathRetriever.GetAdminPrefix()));
                }
            }
            return ResponseFrom(new MemberRolesPermissionSummaryClientProperties() {
                InheritingFrom = "Sorry, this functionality is not available due to Xperience Code Changes"
            });
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

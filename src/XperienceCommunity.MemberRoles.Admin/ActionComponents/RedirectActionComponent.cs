using Kentico.Xperience.Admin.Base;

namespace XperienceCommunity.MemberRoles.Admin.ActionComponents
{
    public class RedirectActionComponent() : ActionComponent<RedirectActionComponentProperties, RedirectActionComponentClientProperties>, IActionComponent
    {
        public override string ClientComponentName => "@memberroles/web-admin/Redirect";

        protected override Task ConfigureClientProperties(RedirectActionComponentClientProperties clientProperties)
        {
            clientProperties.RedirectUrl = this.Properties.RedirectionUrl;
            return base.ConfigureClientProperties(clientProperties);
        }
    }

    public class RedirectActionComponentProperties(string redirectionUrl) : IActionComponentProperties
    {
        public string RedirectionUrl { get; } = redirectionUrl;
    }

    public class RedirectActionComponentClientProperties : IActionComponentClientProperties
    {
        public string RedirectUrl { get; set; } = "";

        public string ComponentName { get; init; } = "@memberroles/web-admin/Redirect";
    }
}

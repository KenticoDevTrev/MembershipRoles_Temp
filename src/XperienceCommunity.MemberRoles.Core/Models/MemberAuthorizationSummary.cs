namespace XperienceCommunity.MemberRoles.Models
{
    public record MemberAuthorizationSummary(bool RequiresAuthentication, IEnumerable<string> Roles);
}

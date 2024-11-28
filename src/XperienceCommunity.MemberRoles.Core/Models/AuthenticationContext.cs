namespace XperienceCommunity.MemberRoles.Models
{
    public record AuthenticationContext(bool IsAuthenticated, string Username, string[] Roles);
}

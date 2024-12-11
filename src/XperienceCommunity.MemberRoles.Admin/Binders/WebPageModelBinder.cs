namespace Kentico.Xperience.Admin.Base
{
    /// <summary>
    /// Binds integer URL parameters into page properties.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of the <see cref="IntPageModelBinder"/>.
    /// </remarks>
    /// <param name="parameterName">Parameter name</param>
    public class WebPageModelBinder(string parameterName) : PageModelBinder<int>(parameterName)
    {
        /// <inheritdoc/>
        public override Task<int> Bind(PageRouteValues routeValues)
        {
            ArgumentNullException.ThrowIfNull(routeValues);

            if (!routeValues.TryGet(parameterName, out var parameterValue)) {
                throw new InvalidOperationException($"Value for '{parameterName}' parameter not found.");
            }
            var intParameterValue = parameterValue.Contains('_') && int.TryParse(parameterValue.Split('_')[1], out var webPageItem) ? webPageItem : int.TryParse(parameterValue, out var id) ? id : 0;

            return Task.FromResult(intParameterValue);
        }
    }
}

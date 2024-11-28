namespace Kentico.Xperience.Admin.Base
{
    /// <summary>
    /// Binds integer URL parameters into page properties.
    /// </summary>
    public class WebPageModelBinder : PageModelBinder<int>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="IntPageModelBinder"/>.
        /// </summary>
        /// <param name="parameterName">Parameter name</param>
        public WebPageModelBinder(string parameterName)
            : base(parameterName)
        {
        }


        /// <inheritdoc/>
        public override Task<int> Bind(PageRouteValues routeValues)
        {
            if (routeValues == null) {
                throw new ArgumentNullException(nameof(routeValues));
            }

            if (!routeValues.TryGet(parameterName, out var parameterValue)) {
                throw new InvalidOperationException($"Value for '{parameterName}' parameter not found.");
            }
            var intParameterValue = parameterValue.Contains("_") && int.TryParse(parameterValue.Split('_')[1], out var webPageItem) ? webPageItem : int.TryParse(parameterValue, out var id) ? id : 0;

            return Task.FromResult(intParameterValue);
        }
    }
}

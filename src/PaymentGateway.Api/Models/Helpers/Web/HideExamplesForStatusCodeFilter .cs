using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PaymentGateway.Api.Models.Helpers.Web
{
    public class HideExamplesForStatusCodeFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (var response in operation.Responses)
            {
                if (response.Key == "404") // Customize for 404
                {
                    response.Value.Content.Clear(); // Remove examples
                }
            }
        }
    }
}

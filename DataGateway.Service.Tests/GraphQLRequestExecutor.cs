using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.DataGateway.Config;
using Azure.DataGateway.Service.Configurations;

namespace Azure.DataGateway.Service.Tests
{
    internal static class GraphQLRequestExecutor
    {
        public static async Task<JsonElement> PostGraphQLRequestAsync(
            HttpClient client,
            RuntimeConfigProvider configProvider,
            string queryName,
            string query,
            Dictionary<string, object> variables = null,
            string authToken = null)
        {
            object payload = variables == null ?
                new { query } :
                new
                {
                    query,
                    variables
                };

            string graphQLEndpoint = configProvider
                .GetRuntimeConfiguration()
                .GraphQLGlobalSettings.Path;

            HttpRequestMessage request = new(HttpMethod.Post, graphQLEndpoint)
            {
                Content = JsonContent.Create(payload)
            };

            if (!string.IsNullOrEmpty(authToken))
            {
                request.Headers.Add(AuthenticationConfig.CLIENT_PRINCIPAL_HEADER, authToken);
            }

            HttpResponseMessage response = await client.SendAsync(request);
            string body = await response.Content.ReadAsStringAsync();

            JsonElement graphQLResult = JsonSerializer.Deserialize<JsonElement>(body);

            if (graphQLResult.TryGetProperty("errors", out JsonElement errors))
            {
                // to validate expected errors and error message
                return errors;
            }

            return graphQLResult.GetProperty("data").GetProperty(queryName);
        }
    }
}

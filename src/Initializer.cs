using Flurl.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kros.DummyData.Initializer
{
    /// <summary>
    /// Dummy data initialzizer.
    /// </summary>
    public static class Initializer
    {
        /// <summary>
        /// Runs the asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        public async static Task RunAsync(InitializerContext context)
        {
            context.Logger.LogInformation("Initialization start.");

            await foreach (Request request in context.GetRequestsAsync())
            {
                using var _ = context.Logger.BeginScope("Request '{request}' =>", request.Description);
                var httpRequest = await context.GetHttpRequestAsync(request);
                LogRequest(context, httpRequest);

                int i = 1;
                await foreach (JsonElement body in context.GetRequestBodiesAsync(request))
                {
                    string requestId = GetRequestId(body) ?? i.ToString();
                    context.Logger.LogTrace("RequestId: '{id}'", requestId);

                    string resounseBody = await GetResponseAsync(request, body, httpRequest, context.Logger);

                    if (request.CanExtractResponse && resounseBody != null)
                    {
                        var result = JsonSerializer
                            .Deserialize<JsonElement>(resounseBody)
                            .GetProperty(request.ExtractResponseProperty).GetRawText();
                        context.Logger.LogTrace("Response content: '{id}'", result);
                        context.AddOutput(request, requestId, result);
                    }
                    i++;
                }
                context.Logger.LogInformation("End request executing.");
            }
        }

        private static void LogRequest(InitializerContext context, IFlurlRequest httpRequest)
        {
            context.Logger.LogInformation("Start request executing.");
            context.Logger.LogTrace("URL: {0}", httpRequest.Url);
            context.Logger.LogTrace("Headers: {0}", httpRequest.Headers);
        }

        private static async Task<string> GetResponseAsync(
            Request request,
            JsonElement body,
            IFlurlRequest httpRequest,
            ILogger logger)
        {
            try
            {
                var response = await httpRequest.PostStringAsync(body.GetRawText());
                logger.LogTrace("Response status code: '{code}'", response.ResponseMessage.StatusCode);

                var resounseBody = await response.ResponseMessage.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

                return resounseBody;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                if (!request.ContinueOnError)
                {
                    throw;
                }
                return null;
            }
        }

        private static string GetRequestId(JsonElement body)
        {
            string requestId = null;
            if (body.TryGetProperty(Constants.RequestIdPropertyName, out var id))
            {
                requestId = id.GetString();
            }

            return requestId;
        }
    }
}

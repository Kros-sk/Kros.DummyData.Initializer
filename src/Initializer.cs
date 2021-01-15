using Flurl.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
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
            context.Logger.LogInformation("Start.");
            var swTotal = Stopwatch.StartNew();

            await foreach (Request request in context.GetRequestsAsync())
            {
                using var _ = context.Logger.BeginScope("Request '{request}' =>", request.Description);
                var sw = Stopwatch.StartNew();

                await ExecuteBodies(context, request);
                context.Logger.LogInformation("End request executing ({duration}).", sw.Elapsed);
            }

            context.Logger.LogInformation("Finish ({duration}).", swTotal.Elapsed);
        }

        private static async Task ExecuteBodies(InitializerContext context, Request request)
        {
            using var concurrencySemaphore = new SemaphoreSlim(context.Options.MaxConcurrencyCount);
            var tasks = new List<Task>();
            await LogRequestAsync(context, request);

            await foreach (JsonElement body in context.GetRequestBodiesAsync(request))
            {
                concurrencySemaphore.Wait();

                tasks.Add(ExecuteRequesAsync(context, request, body, concurrencySemaphore));
            }

            Task.WaitAll(tasks.ToArray());
        }

        private static async Task ExecuteRequesAsync(
            InitializerContext context,
            Request request,
            JsonElement body,
            SemaphoreSlim semaphore)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                Dictionary<string, string> variables = GetRequestVariables(body);
                var httpRequest = await context.GetHttpRequestAsync(request, variables);

                string requestId = GetRequestId(body) ?? Guid.NewGuid().ToString();
                using var _ = context.Logger.BeginScope("RequestId '{id}' =>", requestId);
                context.Logger.LogTrace("Start");

                string resounseBody = await GetResponseAsync(request, body, httpRequest, context.Logger, sw);

                if (request.CanExtractResponse && resounseBody != null)
                {
                    var result = JsonSerializer
                        .Deserialize<JsonElement>(resounseBody)
                        .GetProperty(request.ExtractResponseProperty).GetRawText();
                    context.Logger.LogTrace("Response content: '{id}'", result);
                    context.AddOutput(request, requestId, result);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static async Task LogRequestAsync(InitializerContext context, Request request)
        {
            context.Logger.LogInformation("Start request group executing.");
            context.Logger.LogTrace("URL: {0}", await context.GetUrlAsync(request, Dictionary.Default));
        }

        private static async Task<string> GetResponseAsync(
            Request request,
            JsonElement body,
            IFlurlRequest httpRequest,
            ILogger logger,
            Stopwatch stopwatch)
        {
            try
            {
                var response = await httpRequest.PostStringAsync(body.GetRawText());
                logger.LogTrace("Finish ({duration}ms).", stopwatch.ElapsedMilliseconds);
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

        private static Dictionary<string, string> GetRequestVariables(JsonElement body)
        {
            Dictionary<string, string> variables = Dictionary.Default;
            if (body.TryGetProperty(Constants.Variables, out var vars))
            {
                variables = JsonSerializer.Deserialize<Dictionary<string, string>>(vars.ToString());
            }

            return variables;
        }
    }
}

using Flurl;
using Flurl.Http;
using Kros.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kros.DummyData.Initializer
{
    /// <summary>
    /// Dummy data initializer context.
    /// </summary>
    public class InitializerContext : ITemplateContext
    {
        private static FileReader _fileReader;
        private DirectoryInfo _directoryInfo;
        private IAuthentificationHandler _authentificationHandler;

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        public InitializerOptions Options { get; private set; }

        /// <summary>
        /// Gets the variables.
        /// </summary>
        public IDictionary<string, string> Variables { get; private set; }
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the variables.
        /// </summary>
        public IDictionary<string, string> Outputs { get; private set; }
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Creates the context.
        /// </summary>
        /// <param name="sourceDirectory">The directory.</param>
        /// <param name="loggerFactory">Logger.</param>
        public async static Task<InitializerContext> CreateAsync(
            DirectoryInfo sourceDirectory,
            ILoggerFactory loggerFactory)
        {
            Check.NotNull(sourceDirectory, nameof(sourceDirectory));
            Check.NotNull(loggerFactory, nameof(loggerFactory));
            CheckDirectory(sourceDirectory);

            _fileReader = new FileReader(loggerFactory.CreateLogger<FileReader>());
            var options = await CheckAndLoadOptionsAsync(sourceDirectory);
            FlurlHttp.Configure(s =>
                s.HttpClientFactory = new PollyHttpClientFactory(
                    options.RequestTimeOut,
                    options.Retrying,
                    options.Proxy,
                    loggerFactory.CreateLogger("Polly")));

            var context = new InitializerContext
            {
                _directoryInfo = sourceDirectory,
                _authentificationHandler = AuthentificationHandlerFactory.Create(options.AuthOptions),
                Logger = loggerFactory.CreateLogger(""),
                Options = options
            };
            context.MergeVariables(options.Variables);

            context.Logger.LogInformation("Context created. Source directory: '{0}'", sourceDirectory.FullName);

            return context;
        }

        /// <summary>
        /// Adds the output.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="requestId">The request identifier.</param>
        /// <param name="response">The response.</param>
        public void AddOutput(Request request, string requestId, string response)
            => Outputs.Add($"{request.Id}_{requestId}", response);

        /// <summary>
        /// Gets the requests asynchronous.
        /// </summary>
        public IAsyncEnumerable<Request> GetRequestsAsync()
            => GetRequestsAsync(_directoryInfo.GetDirectories());

        /// <summary>
        /// Gets the request bodies asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        public async IAsyncEnumerable<JsonElement> GetRequestBodiesAsync(Request request)
        {
            foreach (FileInfo file in request.Directory.GetFiles("*.json")
                .Where(p => !p.Name.Equals(Constants.RequestFileName, StringComparison.OrdinalIgnoreCase)))
            {
                IEnumerable<JsonElement> data = await _fileReader.DeserializeAsync<IEnumerable<JsonElement>>(
                    file.FullName,
                    this.CloneAndMerge(request.Variables));

                foreach (var item in data)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Gets the request bodies asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        public async IAsyncEnumerable<(FileInfo fileInfo, string fileContent)> GetFiles(Request request)
        {
            foreach (FileInfo file in request.Directory.GetFiles("*.json")
                .Where(p => !p.Name.Equals(Constants.RequestFileName, StringComparison.OrdinalIgnoreCase)))
            {
                yield return (file, await _fileReader.ReadAsync(
                    file.FullName,
                    this.CloneAndMerge(request.Variables)));
            }
        }

        /// <summary>
        /// Gets the HTTP request asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<IFlurlRequest> GetHttpRequestAsync(Request request)
        {
            Url url = (request.BasePath ?? Options.BaseUrl).AppendPathSegment(request.Path);
            url = AddQueryParams(request, url);
            IFlurlRequest httpRequest = url.ConfigureRequest((o) =>{});

            AddHeaders(request, httpRequest);

            await _authentificationHandler.HandleAsync(httpRequest, request, this);

            return httpRequest;
        }

        private void AddHeaders(Request request, IFlurlRequest httpRequest)
        {
            var headers = new Dictionary<string, string>(Options.DefaultHeaders);

            headers.MergeDictionary(request.Headers);
            httpRequest.WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Json);

            foreach (var header in headers)
            {
                httpRequest.WithHeader(header.Key, header.Value);
            }
        }

        private static Url AddQueryParams(Request request, Url url)
        {
            foreach (var queryParam in request.QueryParams)
            {
                url = url.SetQueryParam(queryParam.Key, queryParam.Value);
            }

            return url;
        }

        private async IAsyncEnumerable<Request> GetRequestsAsync(DirectoryInfo[] directoryInfos)
        {
            foreach (DirectoryInfo directory in directoryInfos.OrderBy(p => p.Name))
            {
                Request request = await GetRequestAsync(directory);
                if (request != null)
                {
                    request.Directory = directory;
                    yield return request;
                }

                await foreach (Request r in GetRequestsAsync(directory.GetDirectories()))
                {
                    yield return r;
                }
            }
        }

        private async Task<Request> GetRequestAsync(DirectoryInfo directory)
        {
            if (!directory.TryGetFilePath(Constants.RequestFileName, out string path))
            {
                return null;
            }

            return await _fileReader.DeserializeAsync<Request>(path, this);
        }

        private async static Task<InitializerOptions> CheckAndLoadOptionsAsync(DirectoryInfo directory)
        {
            if (!directory.TryGetFilePath(Constants.SettingsFileName, out string fileName))
            {
                throw new FileNotFoundException($"Settings file `{fileName}` is required");
            }

            return await _fileReader.DeserializeAsync<InitializerOptions>(fileName);
        }

        private static void CheckDirectory(DirectoryInfo directory)
        {
            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException($"Directory `{directory.FullName}` doesn't exist.");
            }
        }
    }
}

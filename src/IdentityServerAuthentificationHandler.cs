using Flurl.Http;
using Flurl.Http.Configuration;
using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Kros.DummyData.Initializer
{
    /// <summary>
    /// Identity server authorization handler.
    /// </summary>
    /// <seealso cref="Kros.DummyData.Initializer.IAuthentificationHandler" />
    public class IdentityServerAuthentificationHandler : IAuthentificationHandler
    {
        private readonly AuthentificationOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityServerAuthentificationHandler"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        public IdentityServerAuthentificationHandler(AuthentificationOptions options, IHttpClientFactory httpClientFactory)
        {
            _options = options;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Handles.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        public async Task HandleAsync(IFlurlRequest httpRequest, Request request, InitializerContext context)
        {
            User user = request.User ?? _options.User;

            if (!_cache.TryGetValue(user.Name, out string token))
            {
                token = await GetTokenAsync(user, context.Logger);
                _cache.Set(user.Name, token, TimeSpan.FromSeconds(_options.TokenLifeTime));
            }

            httpRequest.WithHeader("authorization", $"Bearer {token}");
        }

        private async Task<string> GetTokenAsync(User user, ILogger logger)
        {
            using var client = _httpClientFactory.CreateHttpClient(_httpClientFactory.CreateMessageHandler());

            var disco = await client.GetDiscoveryDocumentAsync(_options.AuthServer.AbsoluteUri);
            if (disco.IsError)
            {
                logger.LogError(disco.Error);
                throw new Exception(disco.Error);
            }

            var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = _options.ClientId,
                GrantType = "password",
                UserName = user.Name,
                Password = user.Password
            });

            if (tokenResponse.IsError)
            {
                logger.LogError(disco.Error);
                throw new Exception(tokenResponse.Error);
            }

            return tokenResponse.Json.GetProperty("access_token").GetString();
        }
    }
}

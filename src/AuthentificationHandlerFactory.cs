using Flurl.Http;
using Flurl.Http.Configuration;
using System.Threading.Tasks;

namespace Kros.DummyData.Initializer
{
    /// <summary>
    /// Authentification handler factory. Factory for creating <see cref="IAuthentificationHandler"/>.
    /// </summary>
    public class AuthentificationHandlerFactory
    {
        /// <summary>
        /// Creates the specified auhentification handler.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="httpClientFactory">HTTP Client factory.</param>
        public static IAuthentificationHandler Create(
            AuthentificationOptions options,
            IHttpClientFactory httpClientFactory)
        {
            if (options is null)
            {
                return DummyAuthentificationHandler.Default;
            }

            return new IdentityServerAuthentificationHandler(options, httpClientFactory);
        }

        private class DummyAuthentificationHandler : IAuthentificationHandler
        {
            public static IAuthentificationHandler Default = new DummyAuthentificationHandler();

            public Task HandleAsync(IFlurlRequest httpRequest, Request request, InitializerContext context)
                => Task.CompletedTask;
        }
    }
}

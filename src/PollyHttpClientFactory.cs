using Flurl.Http.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Kros.DummyData.Initializer
{
    internal class PollyHttpClientFactory : DefaultHttpClientFactory
    {
        private readonly int _timeoute;
        private readonly IEnumerable<int> _retry;
        private readonly WebProxy _webProxy;
        private readonly ILogger _logger;

        public PollyHttpClientFactory(int timeoute, IEnumerable<int> retry, WebProxy webProxy, ILogger logger)
        {
            _timeoute = timeoute;
            _retry = retry;
            _webProxy = webProxy;
            _logger = logger;
        }

        public override HttpMessageHandler CreateMessageHandler()
            => new PolicyHandler(_timeoute, _retry, _webProxy, _logger);
    }
}

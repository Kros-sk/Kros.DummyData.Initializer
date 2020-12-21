using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kros.DummyData.Initializer
{
    internal class PolicyHandler : HttpClientHandler
    {
        private readonly int _timeoute;
        private readonly IEnumerable<int> _retry;
        private readonly ILogger _logger;

        public PolicyHandler(int timeoute, IEnumerable<int> retry, WebProxy webProxy, ILogger logger)
        {
            _timeoute = timeoute;
            _retry = retry;
            _logger = logger;
            if (webProxy != null)
            {
                Proxy = webProxy;
                UseProxy = true;
            }
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Policies
                .PolicyStrategy(_timeoute, _retry, _logger)
                .ExecuteAsync(ct => base.SendAsync(request, ct), cancellationToken);
    }
}

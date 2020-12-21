using System;
using System.Collections.Generic;
using System.Net;

namespace Kros.DummyData.Initializer
{
    /// <summary>
    /// Dummy data initializer options
    /// </summary>
    public class InitializerOptions
    {
        /// <summary>
        /// Gets or sets the base URL to server.
        /// </summary>
        public Uri BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the variables.
        /// </summary>
        public Dictionary<string, string> Variables { get; set; }
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the default headers.
        /// </summary>
        public Dictionary<string, string> DefaultHeaders { get; set; }

        /// <summary>
        /// Gets or sets the proxy.
        /// </summary>
        public WebProxy Proxy { get; set; }

        /// <summary>
        /// Gets or sets the retrying.
        /// </summary>
        public List<int> Retrying { get; set; } = new List<int>() { 1, 3 };

        /// <summary>
        /// Gets or sets the request time out in second.
        /// </summary>
        public int RequestTimeOut { get; set; } = 5;

        /// <summary>
        /// Gets or sets the authentification options.
        /// </summary>
        public AuthentificationOptions AuthOptions { get; set; }
    }
}

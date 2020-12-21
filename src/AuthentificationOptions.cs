using System;

namespace Kros.DummyData.Initializer
{
    /// <summary>
    /// Authentification options.
    /// </summary>
    public class AuthentificationOptions
    {
        /// <summary>
        /// Gets or sets the authentication server URI.
        /// </summary>
        /// <value>
        /// The authentication server URI.
        /// </value>
        public Uri AuthServer { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the token life time. (In seconds).
        /// </summary>
        /// <value>
        /// The token life time.
        /// </value>
        public int TokenLifeTime { get; set; } = 60;

        /// <summary>
        /// Gets or sets the user credential.
        /// </summary>
        /// <value>
        /// The user.
        /// </value>
        public User User { get; set; }
    }
}

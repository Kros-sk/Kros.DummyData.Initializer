using System.Collections.Generic;

namespace Kros.DummyData.Initializer
{
    /// <summary>
    /// Definition for repeating.
    /// </summary>
    public class RepeatDefinition
    {
        /// <summary>
        /// Gets the default.
        /// </summary>
        /// <value>
        /// The default.
        /// </value>
        public static IEnumerable<RepeatDefinition> Default => new List<RepeatDefinition>() { new RepeatDefinition() };

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the variables.
        /// </summary>
        public IDictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    }
}

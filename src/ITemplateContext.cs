using System.Collections.Generic;

namespace Kros.DummyData.Initializer
{
    /// <summary>
    /// Template context.
    /// </summary>
    public interface ITemplateContext
    {
        /// <summary>
        /// Gets the variables.
        /// </summary>
        IDictionary<string, string> Variables { get; }

        /// <summary>
        /// Gets the outputs variables.
        /// </summary>
        IDictionary<string, string> Outputs { get; }
    }
}

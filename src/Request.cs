using Kros.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace Kros.DummyData.Initializer
{
    /// <summary>
    /// Request info.
    /// </summary>
    public class Request
    {
        private string _id;
        private IEnumerable<RepeatDefinition> _repeatDefinitions;

        internal string Id
        {
            get
            {
                if (_id is null)
                {
                    _id = Name.ToLower().Replace(" ", "_");
                }

                return _id;
            }
        }

        internal DirectoryInfo Directory { get; set; }

        /// <summary>
        /// Gets or sets the base path.
        /// </summary>
        public Uri BasePath { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the query parameters.
        /// </summary>
        public Dictionary<string, string> QueryParams { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the variables.
        /// </summary>
        public IDictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the extract response property.
        /// </summary>
        public string ExtractResponseProperty { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance can extract response.
        /// </summary>
        public bool CanExtractResponse => !ExtractResponseProperty.IsNullOrWhiteSpace();

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [continue on error].
        /// </summary>
        public bool ContinueOnError { get; set; } = false;

        internal void AddRepeatDefinitions(IEnumerable<RepeatDefinition> repeatDefinitions)
        {
            _repeatDefinitions = repeatDefinitions;
        }

        /// <summary>
        /// Gets the repeats.
        /// </summary>
        public IEnumerable<RepeatDefinition> Repeats
        {
            get
            {
                if (_repeatDefinitions is null)
                {
                    _repeatDefinitions = RepeatDefinition.Default;
                }
                return _repeatDefinitions;
            }
        }
    }
}

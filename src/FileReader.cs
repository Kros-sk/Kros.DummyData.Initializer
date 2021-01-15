using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kros.DummyData.Initializer
{
    /// <summary>
    /// File reader, which postprocess readed file with scriban.
    /// </summary>
    public class FileReader
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileReader"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public FileReader(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Deserializes.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="filePath">The file path.</param>
        /// <param name="variables">The variables.</param>
        public async Task<TValue> DeserializeAsync<TValue>(string filePath, IDictionary<string, string> variables)
            => await DeserializeAsync<TValue>(filePath, TemporaryTemplateContext.FromVariables(variables));

        /// <summary>
        /// Deserializes.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="filePath">The file path.</param>
        public async Task<TValue> DeserializeAsync<TValue>(string filePath)
            => await DeserializeAsync<TValue>(filePath, TemporaryTemplateContext.Empty);

        /// <summary>
        /// Deserializes.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="filePath">The file path.</param>
        /// <param name="context">The context.</param>
        public async Task<TValue> DeserializeAsync<TValue>(string filePath, ITemplateContext context)
            => JsonSerializer.Deserialize<TValue>(await ReadAsync(filePath, context));

        /// <summary>
        /// Reads.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="variables">The variables.</param>
        public async Task<string> ReadAsync(string filePath, IDictionary<string, string> variables)
            => await ReadAsync(filePath, TemporaryTemplateContext.FromVariables(variables));

        /// <summary>
        /// Reads.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public async Task<string> ReadAsync(string filePath)
            => await ReadAsync(filePath, TemporaryTemplateContext.Empty);

        /// <summary>
        /// Reads.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="context">The context.</param>
        public async Task<string> ReadAsync(string filePath, ITemplateContext context)
        {
            _logger.LogTrace("Read file: '{0}'", filePath);

            string templateText = await File.ReadAllTextAsync(filePath);
            string result = await TemplateProcessor.ProcessAsync(templateText, context, _logger);

            return result;
        }
    }
}

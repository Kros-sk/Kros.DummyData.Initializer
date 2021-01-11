using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Runtime;
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
        private readonly TemplateFunctions _functions = new TemplateFunctions();

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

            Template template = Template.Parse(await File.ReadAllTextAsync(filePath));
            TemplateContext c = CreateScribanContext(context);

            string result = await template.RenderAsync(c);

            ReportErrors(template);

            return result;
        }

        private TemplateContext CreateScribanContext(ITemplateContext context)
        {
            var c = new CustomTemplateContext();
            var scriptObject = new ScriptObject();
            scriptObject.Import(new
            {
                variables = context.Variables,
                outputs = context.Outputs
            });
            c.PushGlobal(scriptObject);
            c.PushGlobal(_functions);

            return c;
        }

        private void ReportErrors(Template template)
        {
            if (template.HasErrors)
            {
                foreach (Scriban.Parsing.LogMessage error in template.Messages)
                {
                    _logger.LogWarning(error.Message);
                }
            }
        }
    }
}

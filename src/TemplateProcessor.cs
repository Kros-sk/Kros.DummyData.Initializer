using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Runtime;
using System.Threading.Tasks;

namespace Kros.DummyData.Initializer
{
    internal class TemplateProcessor
    {
        private static readonly TemplateFunctions _functions = new TemplateFunctions();

        public static async Task<string> ProcessAsync(string templateText, ITemplateContext context, ILogger logger)
        {
            var template = Template.Parse(templateText);
            TemplateContext c = CreateScribanContext(context);

            string result = await template.RenderAsync(c);

            ReportErrors(template, logger);

            return result;
        }

        private static TemplateContext CreateScribanContext(ITemplateContext context)
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

        private static void ReportErrors(Template template, ILogger logger)
        {
            if (template.HasErrors)
            {
                foreach (Scriban.Parsing.LogMessage error in template.Messages)
                {
                    logger.LogWarning(error.Message);
                }
            }
        }
    }
}

using System.Collections.Generic;

namespace Kros.DummyData.Initializer
{
    internal class TemporaryTemplateContext : ITemplateContext
    {
        public IDictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();

        public IDictionary<string, string> Outputs { get; set; } = new Dictionary<string, string>();

        public static ITemplateContext Empty => new TemporaryTemplateContext();

        public static ITemplateContext FromVariables(IDictionary<string, string> variables)
            => new TemporaryTemplateContext() { Variables = variables };

        public static ITemplateContext FromContext(ITemplateContext context)
            => new TemporaryTemplateContext()
            {
                Variables = new Dictionary<string, string>(context.Variables),
                Outputs = new Dictionary<string, string>(context.Outputs)
            };
    }
}

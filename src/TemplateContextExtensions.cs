using System.Collections.Generic;

namespace Kros.DummyData.Initializer
{
    internal static class TemplateContextExtensions
    {
        public static ITemplateContext MergeVariables(this ITemplateContext context, IDictionary<string, string> variables)
        {
            context.Variables.MergeDictionary(variables);
            return context;
        }

        public static ITemplateContext Merge(this ITemplateContext context, ITemplateContext newContext)
        {
            context.Variables.MergeDictionary(newContext.Variables);
            context.Outputs.MergeDictionary(newContext.Outputs);

            return context;
        }

        public static ITemplateContext CloneAndMerge(this ITemplateContext context, IDictionary<string, string> variables)
            => TemporaryTemplateContext.FromContext(context).MergeVariables(variables);

        public static void MergeDictionary(this IDictionary<string, string> value, IDictionary<string, string> newValues)
        {
            foreach (KeyValuePair<string, string> item in newValues)
            {
                value[item.Key] = item.Value;
            }
        }
    }
}

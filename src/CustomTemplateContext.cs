using Scriban;
using Scriban.Syntax;
using System.Threading.Tasks;

namespace Kros.DummyData.Initializer
{
    internal class CustomTemplateContext: TemplateContext
    {
        public override object Evaluate(ScriptNode scriptNode, bool aliasReturnedFunction)
            => base.Evaluate(scriptNode, aliasReturnedFunction);

        public async override ValueTask<object> EvaluateAsync(ScriptNode scriptNode, bool aliasReturnedFunction)
        {
            var value = await base.EvaluateAsync(scriptNode, aliasReturnedFunction);
            if ((scriptNode is ScriptMemberExpression sm)
                && (sm.Target is ScriptVariableGlobal v)
                && (v.BaseName == Constants.Outputs || v.BaseName == Constants.Variables)
                && (value is null))
            {
                value = string.Concat("{{", scriptNode.ToString(), "}}");
            }

            return value;
        }

        public override string ObjectToString(object value, bool nested = false)
            => base.ObjectToString(value, nested);
    }
}

using OpenFeature.Model;

namespace OpenFeature.E2ETests.Utils;

public class ContextEvaluatorUtility
{
    // Very small expression translator for patterns like:
    // "email == 'ballmer@macrosoft.com' ? 'zero' : ''"
    // "!customer && email == 'x' && age > 10 ? 'internal' : ''"
    public static Func<EvaluationContext, string>? BuildContextEvaluator(string expression)
    {
        // Split "condition ? 'trueVariant' : 'falseVariant'"
        var qIndex = expression.IndexOf('?');
        var colonIndex = expression.LastIndexOf(':');
        if (qIndex < 0 || colonIndex < 0 || colonIndex < qIndex)
            return null; // unsupported format, ignore

        var conditionPart = expression.Substring(0, qIndex).Trim();
        var truePart = ExtractQuoted(expression.Substring(qIndex + 1, colonIndex - qIndex - 1));
        var falsePart = ExtractQuoted(expression.Substring(colonIndex + 1));

        var conditions = conditionPart.Split(new[] { "&&" }, StringSplitOptions.RemoveEmptyEntries);

        return ctx =>
        {
            foreach (var raw in conditions)
            {
                if (!EvaluateSingle(raw.Trim(), ctx))
                    return falsePart;
            }
            return truePart;
        };
    }

    private static string ExtractQuoted(string segment)
    {
        segment = segment.Trim();
        if (segment.StartsWith("'") && segment.EndsWith("'") && segment.Length >= 2)
            return segment.Substring(1, segment.Length - 2);
        return segment;
    }

    private static bool EvaluateSingle(string expr, EvaluationContext ctx)
    {
        // Supported fragments:
        // !key
        // key
        // key == 'string'
        // key != 'string'
        // key > number, key < number, key >= number, key <= number
        expr = expr.Trim();

        bool negate = false;
        if (expr.StartsWith("!"))
        {
            negate = true;
            expr = expr.Substring(1).Trim();
        }

        bool result;
        if (TryParseComparison(expr, ctx, out result))
        {
            return negate ? !result : result;
        }

        // Treat raw key presence / truthiness
        if (!ctx.TryGetValue(expr, out var value) || value == null || value.IsNull)
            result = false;
        else if (value.IsBoolean)
            result = value.AsBoolean == true;
        else if (value.IsString)
            result = !string.IsNullOrEmpty(value.AsString);
        else if (value.IsNumber)
            result = value.AsDouble.GetValueOrDefault() != 0.0;
        else
            result = true;

        return negate ? !result : result;
    }

    // Supported operations
    static readonly string[] _operations = ["==", "!=", ">=", "<=", ">", "<"];

    private static bool TryParseComparison(string expr, EvaluationContext ctx, out bool result)
    {
        result = false;

        foreach (var op in _operations)
        {
            var idx = expr.IndexOf(op, StringComparison.Ordinal);
            if (idx <= 0) continue;

            var left = expr.Substring(0, idx).Trim();
            var right = expr.Substring(idx + op.Length).Trim();

            if (!ctx.TryGetValue(left, out var val) || val == null)
                return true; // treat missing as false; caller will interpret

            if (right.StartsWith("'") && right.EndsWith("'"))
            {
                var literal = right.Substring(1, right.Length - 2);
                var strVal = val.AsString;
                result = op switch
                {
                    "==" => strVal == literal,
                    "!=" => strVal != literal,
                    _ => false
                };

                return true;
            }

            if (double.TryParse(right, out var numRight))
            {
                var numLeft = val.AsDouble ?? val.AsInteger;
                if (numLeft == null)
                    return true;

                result = op switch
                {
                    ">" => numLeft > numRight,
                    "<" => numLeft < numRight,
                    ">=" => numLeft >= numRight,
                    "<=" => numLeft <= numRight,
                    "==" => Math.Abs(numLeft.Value - numRight) < double.Epsilon,
                    "!=" => Math.Abs(numLeft.Value - numRight) >= double.Epsilon,
                    _ => false
                };

                return true;
            }

            return true;
        }

        return false;
    }
}

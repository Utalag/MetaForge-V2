// ---------------------------------------------------------------------------
// MetaForge.Core — ReflectionBasedInvariantEvaluator
// Default IInvariantEvaluator implementation using reflection to resolve 
// property paths and evaluate the boolean AST.
// Vrstva: Core / Specifications
// 
// PROPOSAL: PROP-036 — Core Specification Layer
// ---------------------------------------------------------------------------

using System.Diagnostics;
using System.Reflection;

namespace MetaForge.Core.Specifications;

/// <summary>
/// Default invariant evaluator that uses .NET reflection to resolve property paths
/// and evaluate the boolean AST against a target object.
/// 
/// Thread-safe and stateless — can be registered as Singleton.
/// </summary>
public sealed class ReflectionBasedInvariantEvaluator : IInvariantEvaluator
{
    /// <inheritdoc />
    public EvaluationResult Evaluate(
        object target,
        InvariantEvaluationContext context,
        IReadOnlyList<InvariantDefinition> invariants)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(invariants);

        var sw = Stopwatch.StartNew();
        var violations = new List<InvariantViolation>();
        var targetType = target.GetType();
        var targetKind = targetType.Name; // Convention: type name matches TargetKind

        foreach (var invariant in invariants)
        {
            // Filter: only evaluate invariants targeting this element kind
            if (!string.IsNullOrEmpty(invariant.TargetKind) &&
                !string.Equals(invariant.TargetKind, targetKind, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Check if the When condition is met (if When is null, always check)
            if (invariant.When != null)
            {
                var whenResult = EvaluateExpression(invariant.When, target, context);
                if (whenResult is not true)
                    continue; // When condition not met — invariant does not apply
            }

            // Check the Must condition
            if (invariant.Must != null)
            {
                var mustResult = EvaluateExpression(invariant.Must, target, context);
                if (mustResult is not true)
                {
                    var elementPath = BuildElementPath(target, context);
                    violations.Add(InvariantViolation.Create(invariant, elementPath,
                        detail: $"Expected: {FormatExpression(invariant.Must)}, Actual: {mustResult}"));
                }
            }
        }

        sw.Stop();
        return new EvaluationResult
        {
            Violations = violations,
            TotalEvaluated = invariants.Count,
            EvaluationTime = sw.Elapsed
        };
    }

    /// <summary>
    /// Evaluates an invariant expression tree against the target and context.
    /// Returns true/false for boolean expressions, or the resolved value for property refs.
    /// </summary>
    private object? EvaluateExpression(
        InvariantExpression expr,
        object target,
        InvariantEvaluationContext context)
    {
        return expr switch
        {
            ConstantExpression c => c.Value,
            PropertyRef p => ResolvePropertyPath(p.Path, target, context),
            EqExpression eq => EvaluateEq(eq, target, context),
            NotExpression n => EvaluateNot(n, target, context),
            AndExpression a => EvaluateAnd(a, target, context),
            OrExpression o => EvaluateOr(o, target, context),
            ExistsExpression e => EvaluateExists(e, target, context),
            _ => throw new NotSupportedException($"Unknown invariant expression type: {expr.GetType().Name}")
        };
    }

    private object? EvaluateEq(EqExpression eq, object target, InvariantEvaluationContext context)
    {
        var left = EvaluateExpression(eq.Left, target, context);
        var right = EvaluateExpression(eq.Right, target, context);
        return Equals(left, right);
    }

    private object? EvaluateNot(NotExpression not, object target, InvariantEvaluationContext context)
    {
        var inner = EvaluateExpression(not.Inner, target, context);
        // Boolean: invert. Non-boolean / null: treat as falsy → NOT(falsy) = true.
        return inner is bool b ? !b : true;
    }

    private object? EvaluateAnd(AndExpression and, object target, InvariantEvaluationContext context)
    {
        foreach (var item in and.Items)
        {
            var result = EvaluateExpression(item, target, context);
            if (result is not bool)
                throw new InvalidOperationException(
                    $"AND expression requires boolean sub-expressions. Got {result?.GetType().Name ?? "null"} from {item.GetType().Name}.");
            if ((bool)result == false) return false;
        }
        return true;
    }

    private object? EvaluateOr(OrExpression or, object target, InvariantEvaluationContext context)
    {
        foreach (var item in or.Items)
        {
            var result = EvaluateExpression(item, target, context);
            if (result is not bool)
                throw new InvalidOperationException(
                    $"OR expression requires boolean sub-expressions. Got {result?.GetType().Name ?? "null"} from {item.GetType().Name}.");
            if ((bool)result == true) return true;
        }
        return false;
    }

    private object? EvaluateExists(ExistsExpression exists, object target, InvariantEvaluationContext context)
    {
        var value = ResolvePropertyPath(exists.Path, target, context);
        // "Exists" means the value is non-null (for reference types) or has a value.
        // For bool properties, existence means the property itself exists.
        // For collection/list properties: non-null and has elements.
        if (value is null) return false;
        if (value is System.Collections.ICollection col) return col.Count > 0;
        if (value is string s) return !string.IsNullOrEmpty(s);
        return true; // non-null, non-empty
    }

    /// <summary>
    /// Resolves a JSONPath-like property path against the target object using reflection.
    /// Supports: "$.PropertyName", "$.Property.SubProperty", "$.Collection.Length".
    /// </summary>
    private static object? ResolvePropertyPath(
        string path,
        object target,
        InvariantEvaluationContext context)
    {
        // Strip leading "$." if present
        var segments = path.TrimStart('$', '.').Split('.');
        object? current = target;

        foreach (var segment in segments)
        {
            if (current is null) return null;

            var type = current.GetType();
            var prop = type.GetProperty(segment,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (prop is null)
            {
                // Try on context parent for scoped evaluation
                if (context.Parent is not null)
                {
                    var parentProp = context.Parent.GetType().GetProperty(segment,
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (parentProp is not null)
                    {
                        current = parentProp.GetValue(context.Parent);
                        continue;
                    }
                }

                // Property not found — treat as null
                return null;
            }

            current = prop.GetValue(current);
        }

        return current;
    }

    private static string BuildElementPath(object target, InvariantEvaluationContext context)
    {
        var type = target.GetType();
        var nameProp = type.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
        var name = nameProp?.GetValue(target)?.ToString() ?? target.GetType().Name;

        if (context.Parent is not null)
        {
            var parentType = context.Parent.GetType();
            var parentName = parentType.GetProperty("Name",
                BindingFlags.Public | BindingFlags.Instance)?.GetValue(context.Parent)?.ToString() ?? "?";
            return $"{parentName}/{name}";
        }

        return name;
    }

    private static string FormatExpression(InvariantExpression expr) => expr switch
    {
        PropertyRef p => p.Path,
        ConstantExpression c => c.Value?.ToString() ?? "null",
        NotExpression n => $"NOT({FormatExpression(n.Inner)})",
        ExistsExpression e => $"EXISTS({e.Path})",
        EqExpression eq => $"{FormatExpression(eq.Left)} == {FormatExpression(eq.Right)}",
        AndExpression a => $"({string.Join(" AND ", a.Items.Select(FormatExpression))})",
        OrExpression o => $"({string.Join(" OR ", o.Items.Select(FormatExpression))})",
        _ => expr.ToString() ?? "?"
    };
}

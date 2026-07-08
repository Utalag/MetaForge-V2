using System.Text.RegularExpressions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Primitives;

namespace MetaForge.Core.Inference.Boundary.DomainAnalyzers;

/// <summary>
/// Univerzální pravidlový analyzér — jádro hybridní architektury BoundaryAnalyzer.
///
/// Pokrývá jednoduché boundary případy konfigurovatelnou sadou <see cref="BoundaryRule"/>
/// bez nutnosti psát vlastní IDomainAnalyzer třídu.
/// Pro komplexní doménovou logiku slouží specializované pluginy (Finance, Math, ...).
///
/// Registrace pravidel:
/// <code>
/// // Přes orchestrátor (doporučeno):
/// methodBoundaryAnalyzer.AddRule(new BoundaryRule
/// {
///     ParamNamePattern = "amount|value|price",
///     Condition        = "{param} &lt;= 0",
///     ExceptionType    = "ArgumentException",
///     ExceptionMessage = "Parametr '{param}' musí být kladné číslo."
/// });
///
/// // Nebo přímo:
/// var rules = new RulesBoundaryAnalyzer();
/// rules.AddRule(myRule);
/// analyzer.Register(rules);
/// </code>
///
/// Priority = 50 — spouští se před specializovanými pluginy (80–100) ale po GenericBoundaryAnalyzer (0).
/// Tím specializované pluginy mohou doménová pravidla zpřesnit nebo rozšířit.
/// </summary>
public sealed class RulesBoundaryAnalyzer : IDomainAnalyzer
{
    private readonly List<BoundaryRule> _rules = new();

    public string DomainName => "rules";

    public IReadOnlyList<string> Keywords => Array.Empty<string>();

    public int Priority => 50;

    public bool IsAvailable => true;

    /// <summary>
    /// CanHandle vrátí true pokud existuje alespoň jedno pravidlo aplikovatelné na tuto metodu.
    /// </summary>
    public bool CanHandle(Method method)
    {
        return _rules.Count > 0 && method.Parameters.Count > 0;
    }

    /// <summary>
    /// Přidá pravidlo do sady. Vrátí this pro fluent řetězení.
    /// </summary>
    public RulesBoundaryAnalyzer AddRule(BoundaryRule rule)
    {
        _rules.Add(rule);
        return this;
    }

    /// <summary>
    /// Přidá více pravidel najednou.
    /// </summary>
    public RulesBoundaryAnalyzer AddRules(IEnumerable<BoundaryRule> rules)
    {
        _rules.AddRange(rules);
        return this;
    }

    /// <summary>
    /// Seznam aktuálně registrovaných pravidel (read-only).
    /// </summary>
    public IReadOnlyList<BoundaryRule> Rules => _rules.AsReadOnly();

    public Task<IReadOnlyList<MethodConstraint>> AnalyzeAsync(Method method)
    {
        var constraints = new List<MethodConstraint>();
        var methodName  = method.Name;

        foreach (var param in method.Parameters)
        {
            foreach (var rule in _rules)
            {
                if (!MatchesRule(rule, methodName, param))
                    continue;

                var resolved  = rule.Resolve(param.Name);
                var condition = resolved.Condition;

                // Přeskočit pokud je podmínka sémanticky již přítomna
                if (constraints.Any(c => c.InvalidCondition == condition))
                    continue;

                constraints.Add(new MethodConstraint
                {
                    InvalidCondition = condition,
                    Description      = resolved.Description ?? $"Pravidlo: {condition}",
                    Kind             = ConstraintKind.Precondition,
                    ExceptionType    = resolved.ExceptionType,
                    ExceptionMessage = resolved.ExceptionMessage,
                    Source           = ConstraintSource.FromRuleBased
                });
            }
        }

        return Task.FromResult<IReadOnlyList<MethodConstraint>>(constraints);
    }

    // ── Matching ─────────────────────────────────────────────────────────────

    private static bool MatchesRule(BoundaryRule rule, string methodName, Parameter param)
    {
        // Název metody musí odpovídat (pokud je pattern definován)
        if (rule.MethodNamePattern is not null &&
            !Regex.IsMatch(methodName, rule.MethodNamePattern, RegexOptions.IgnoreCase))
            return false;

        // Název parametru musí odpovídat (pokud je pattern definován)
        if (rule.ParamNamePattern is not null &&
            !Regex.IsMatch(param.Name, rule.ParamNamePattern, RegexOptions.IgnoreCase))
            return false;

        // Typ parametru musí odpovídat (pokud je definován)
        if (rule.ParamTypeName is not null &&
            !MatchesParamType(rule.ParamTypeName, param.Type.BaseType))
            return false;

        return true;
    }

    private static bool MatchesParamType(string typeName, DataType baseType)
    {
        return typeName.ToLowerInvariant() switch
        {
            "double"  or "float64" => baseType == DataType.Double,
            "float"   or "float32" => baseType == DataType.Float,
            "int"     or "integer" => baseType == DataType.Int,
            "long"                 => baseType == DataType.Long,
            "decimal"              => baseType == DataType.Decimal,
            "string"               => baseType == DataType.String,
            "numeric"              => baseType is DataType.Double or DataType.Float
                                               or DataType.Int    or DataType.Long
                                               or DataType.Decimal,
            _                      => false
        };
    }
}

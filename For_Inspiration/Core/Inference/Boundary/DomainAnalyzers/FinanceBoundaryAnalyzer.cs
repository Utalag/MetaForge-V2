using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Primitives;

namespace MetaForge.Core.Inference.Boundary.DomainAnalyzers;

/// <summary>
/// Specializovaný analyzér pro finanční metody.
/// Detekuje hraniční stavy typické pro finanční logiku:
///   - záporné nebo nulové částky (deposit, withdraw, payment...)
///   - záporné procento nebo sazba (rate, percent, bonus...)
///   - potenciálně záporný zůstatek (withdraw > balance)
///
/// Priority = 90 — mezi MathBoundaryAnalyzer (100) a CollectionBoundaryAnalyzer (80).
///
/// Příklady metod, které CanHandle vrátí true:
///   deposit, withdraw, transfer, pay, credit, debit, charge, invest,
///   calculateBonus, applyFee, processPayout, depositWithBonus
/// </summary>
public sealed class FinanceBoundaryAnalyzer : IDomainAnalyzer
{
    public string DomainName => "finance";

    public IReadOnlyList<string> Keywords => new[]
    {
        "deposit", "withdraw", "transfer", "payment", "pay",
        "credit", "debit", "charge", "invest", "fee", "bonus",
        "balance", "amount", "refund", "payout", "salary",
        "budget", "expense", "income", "revenue", "price", "cost"
    };

    public int Priority => 90;

    public bool IsAvailable => true;

    /// <summary>
    /// Metoda je finanční pokud:
    ///   - název nebo parametr obsahuje finanční klíčové slovo, NEBO
    ///   - má číselný parametr s názvem amount/value/price/fee/sum/rate.
    /// </summary>
    public bool CanHandle(Method method)
    {
        var name = method.Name.ToLowerInvariant();
        var hasFinanceKeyword = Keywords.Any(k => name.Contains(k));
        var hasFinanceParam = method.Parameters.Any(p =>
            IsNumericType(p.Type.BaseType) && IsFinanceParamName(p.Name));

        return hasFinanceKeyword || hasFinanceParam;
    }

    public Task<IReadOnlyList<MethodConstraint>> AnalyzeAsync(Method method)
    {
        var constraints = new List<MethodConstraint>();

        AnalyzePositiveAmounts(method, constraints);
        AnalyzeRateOrPercent(method, constraints);
        AnalyzeWithdrawBalance(method, constraints);

        return Task.FromResult<IReadOnlyList<MethodConstraint>>(constraints);
    }

    // ── Analýza částek ────────────────────────────────────────────────────────

    /// <summary>
    /// Finanční partial amount musí být kladná.
    /// Pro parametry: amount, value, price, fee, sum, cost, total → amount &lt;= 0.
    /// </summary>
    private static void AnalyzePositiveAmounts(Method method, List<MethodConstraint> constraints)
    {
        var amountParams = method.Parameters
            .Where(p => IsNumericType(p.Type.BaseType) && IsAmountParamName(p.Name));

        foreach (var param in amountParams)
        {
            var condition = $"{param.Name} <= 0";
            if (constraints.Any(c => c.InvalidCondition == condition)) continue;

            constraints.Add(new MethodConstraint
            {
                InvalidCondition = condition,
                Description = $"Finanční částka '{param.Name}' musí být kladné číslo (> 0).",
                Kind = ConstraintKind.Precondition,
                ExceptionType = "ArgumentException",
                ExceptionMessage = $"Parametr '{param.Name}' musí být kladné číslo.",
                Source = ConstraintSource.FromRuleBased
            });
        }
    }

    /// <summary>
    /// Procento nebo sazba musí být v platném rozsahu.
    /// Pro parametry: rate, percent, percentage, ratio, factor → rate &lt; 0 || rate > 1 (nebo > 100).
    /// </summary>
    private static void AnalyzeRateOrPercent(Method method, List<MethodConstraint> constraints)
    {
        var rateParams = method.Parameters
            .Where(p => IsNumericType(p.Type.BaseType) && IsRateParamName(p.Name));

        foreach (var param in rateParams)
        {
            var condition = $"{param.Name} < 0";
            if (constraints.Any(c => c.InvalidCondition == condition)) continue;

            constraints.Add(new MethodConstraint
            {
                InvalidCondition = condition,
                Description = $"Sazba '{param.Name}' nesmí být záporná.",
                Kind = ConstraintKind.Precondition,
                ExceptionType = "ArgumentOutOfRangeException",
                ExceptionMessage = $"Parametr '{param.Name}' musí být nezáporné číslo.",
                Source = ConstraintSource.FromRuleBased
            });
        }
    }

    /// <summary>
    /// Metody pro výběr (withdraw) by neměly vybírat více než je aktuální zůstatek.
    /// Detekce: název obsahuje "withdraw" + parametr 'amount' + třída má property Balance/Funds.
    /// </summary>
    private static void AnalyzeWithdrawBalance(Method method, List<MethodConstraint> constraints)
    {
        var name = method.Name.ToLowerInvariant();
        if (!name.Contains("withdraw") && !name.Contains("debit") && !name.Contains("deduct"))
            return;

        var amountParam = method.Parameters
            .FirstOrDefault(p => IsNumericType(p.Type.BaseType) && IsAmountParamName(p.Name));

        if (amountParam == null) return;

        var condition = $"{amountParam.Name} > Balance";
        if (constraints.Any(c => c.InvalidCondition == condition)) return;

        constraints.Add(new MethodConstraint
        {
            InvalidCondition = condition,
            Description = $"Výběr '{amountParam.Name}' nesmí přesáhnout aktuální zůstatek.",
            Kind = ConstraintKind.Precondition,
            ExceptionType = "InvalidOperationException",
            ExceptionMessage = $"Nedostatek prostředků: '{amountParam.Name}' přesahuje Balance.",
            Source = ConstraintSource.FromRuleBased
        });
    }

    // ── Pomocné metody ────────────────────────────────────────────────────────

    private static bool IsNumericType(DataType type) =>
        type is DataType.Int or DataType.Long or DataType.Double or
                DataType.Float or DataType.Decimal;

    private static bool IsFinanceParamName(string name)
    {
        var n = name.ToLowerInvariant();
        return n is "amount" or "value" or "price" or "fee" or "sum"
                  or "cost" or "total" or "rate" or "percent" or "bonus"
                  or "salary" or "budget" or "payment" or "credit";
    }

    private static bool IsAmountParamName(string name)
    {
        var n = name.ToLowerInvariant();
        return n is "amount" or "value" or "price" or "fee" or "sum"
                  or "cost" or "total" or "payment" or "credit" or "deposit";
    }

    private static bool IsRateParamName(string name)
    {
        var n = name.ToLowerInvariant();
        return n is "rate" or "percent" or "percentage" or "ratio" or "factor" or "interest";
    }
}

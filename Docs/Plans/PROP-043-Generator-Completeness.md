# PROP-043 Generator Completeness — Expression, Statement, Element gaps

Typ výsledku: Candidate Proposal
Zdroj podnětu: AI — Perplexity Deep Research (konverzace e7299554)
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-08

Priorita: High
Oblast: Generators, Core
Owner:
Datum vytvoření: 2026-07-08
Aktualizováno: 2026-07-08

Navazuje na:
- PROP-031 (Core Statement System)
- PROP-035 (C#-First Core Migration)
- PROP-037 (C# Completeness)
- PROP-040 (Core Member Consistency)
- PROP-041 (ConstructorElement + FieldElement)
- Perplexity revize: https://www.perplexity.ai/search/e7299554-47b9-465b-94ef-7c3d1de1e092

Blokuje:
- —

Související soubory:
- `Src/MetaForge.Generators/CodeGenerator.cs`
- `Src/MetaForge.Generators/ExpressionRenderer.cs`
- `Src/MetaForge.Generators/Templates/*.scriban`

## 1. Kontext

Perplexity Deep Research provedl revizi Generators vrstvy a identifikoval významné mezery v pokrytí Core → C# generování:

1. **9 z 15 expression typů** chybí nebo jsou neúplné
2. **6 ze 13 statement typů** chybí
3. **ConstructorElement a FieldElement** mají šablony, ale nejsou použity v GenerateClass
4. **DelegateElement, EventElement, OperatorElement** nejsou vůbec generovány
5. **MapType** tiše degraduje (Entity→object) místo chyby/varování
6. **MethodCallExpression** chybí Target prefix (obj.Method() místo Method())

## 2. Problém dnes

- Generátor umí generovat pouze ~50% Core modelu beze ztráty
- Chybějící expression a statement typy způsobují, že generovaný kód je nekompletní
- ConstructorElement, FieldElement nelze generovat (šablony existují, ale nejsou volány)
- MapType tiše produkuje špatný kód místo chyby

## 3. Cíl

- Plné pokrytí 15 expression typů v ExpressionRenderer
- Plné pokrytí 13 statement typů v RenderMethodBody
- Napojení Constructor.scriban a Field.scriban do GenerateClass
- Generování DelegateElement, EventElement, OperatorElement
- MapType varování místo tiché degradace
- MethodCallExpression.Target renderování

## 4. Architektonické invarianty

- Scriban zůstává templating engine (Perplexity: "Scriban je správná volba")
- Core zůstává nezměněn — pouze generátorové změny

## 5. Scope

### In scope
- Expression renderer: New, Await, Conversion, Default, IsPattern, Lambda, NullCoalescing, Switch
- Statement renderer: ForEach, Switch, TryCatch, Using, UsingDeclaration, LocalFunction
- GenerateClass: Constructors + Fields integrace
- Nové elementy: Delegate, Event, Operator
- MethodCallExpression.Target
- MapType varování

### Out of scope
- Scriban → T4 migrace (zamítnuto — Scriban zůstává)
- StrongType generování (samostatný PROP)
- RecordElement generování (není samostatný element v Core)

## 6. Návrh řešení

### Expression renderer — nové

```csharp
private string RenderExpression(Expression expr) => expr switch
{
    NewExpression newExpr => $"new {newExpr.TypeName}({RenderArgs(newExpr.ConstructorArguments)})",
    AwaitExpression awaitExpr => $"await {RenderExpression(awaitExpr.Operand)}",
    ConversionExpression conv => conv.IsExplicit
        ? $"({conv.TargetType}){RenderExpression(conv.Operand)}"
        : RenderExpression(conv.Operand),
    DefaultExpression def => $"default({def.TargetType})",
    IsPatternExpression ip => $"{RenderExpression(ip.Operand)} is {ip.TargetTypeName}",
    LambdaExpression lam => $"({string.Join(", ", lam.ParameterNames)}) => {RenderExpression(lam.Body)}",
    NullCoalescingExpression nc => $"{RenderExpression(nc.Left)} ?? {RenderExpression(nc.Right)}",
    SwitchExpression sw => $"{RenderExpression(sw.Selector)} switch {{ {RenderSwitchArms(sw.Arms)} }}",
    ...
};
```

### Statement renderer — nové

```csharp
SwitchStatement sw => $"switch ({RenderExpression(sw.Selector)}) {{ ... }}",
ForEachStatement fe => $"foreach ({fe.VariableType} {fe.VariableName} in {RenderExpression(fe.Collection)}) ...",
TryCatchStatement tc => $"try {{ ... }} catch (Ex) {{ ... }} finally {{ ... }}",
```

### GenerateClass rozšíření

```csharp
if (cls.Constructors.Count > 0)
{
    model["constructors"] = cls.Constructors.Select(c => RenderConstructor(c)).ToList();
}
if (cls.Fields.Count > 0)
{
    model["fields"] = cls.Fields.Select(f => RenderField(f)).ToList();
}
```

## 7. Implementační fáze

### Fáze 1 — Expression renderer (kritické)
- [x] NewExpression
- [x] AwaitExpression
- [ ] ConversionExpression
- [ ] NullCoalescingExpression
- [ ] MethodCallExpression.Target

### Fáze 2 — Statement renderer (kritické)
- [ ] ForEachStatement
- [ ] TryCatchStatement
- [ ] SwitchStatement
- [ ] UsingStatement, UsingDeclarationStatement
- [ ] LocalFunctionStatement

### Fáze 3 — Element integrace
- [ ] Constructor.scriban do GenerateClass
- [ ] Field.scriban do GenerateClass
- [ ] DelegateElement generování
- [ ] EventElement generování
- [ ] OperatorElement generování

### Fáze 4 — MapType a diagnostika
- [ ] MapType varování místo tiché degradace
- [ ] NullCoalescingExpression deduplikace

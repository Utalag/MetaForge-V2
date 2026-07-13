# Test scaffold

> Testovací strategie a scaffold pro nový projekt.

---

## Testovací principy

1. **Testy jsou first-class citizen** — vznikají s každou vrstvou, ne až na konci.
2. **Unit testy preferovány** — rychlé, izolované, deterministické.
3. **Integration testy pro facade** — ověření orchestrace přes vrstvy.
4. **Žádné testy závislé na AI** — AI path má graceful fallback, testuje se deterministická cesta.
5. **Replay testy** — ověření, že replay N commandů = expected state.
6. **Generator output testy** — ověření, že generovaný C# je kompilabilní.

---

## Testovací projekty

| Projekt | Co testuje | Typ testů |
|---------|-----------|-----------|
| `MetaForge.Core.Tests` | Typový model, katalog, ForgeBlock registrace, discovery, validace (CoreValidator) | Unit |
| `MetaForge.BusinessModel.Tests` | Dokument, CommandLog, replay, patches, validation | Unit |
| `MetaForge.Translator.Tests` | Facade, projekce, překlad, write-back, enrichment | Unit + Integration |
| `MetaForge.Generators.Tests` | C# output, template rendering, kompilabilnost, ExpressionRenderer (58 testů) + TemplateManager (6 testů) | Unit + Snapshot |
| **`MetaForge.Core.Integration.Tests`** | **Core → Generators pipeline (snapshot-based), AppRoot traversal, AST rendering, UPDATE_SNAPSHOTS env var** | **Integration + Snapshot** |

---

## Snapshot testování

Snapshot testy používají `SnapshotComparer` (v `Integration.Tests/Snapshots/`):
- Porovnává generovaný C# kód s `.expected.cs` soubory.
- Při prvním spuštění vytvoří expected soubor a failne (first-run).
- `$env:UPDATE_SNAPSHOTS="true"` — tichý přepis expected souborů při refactoringu.
- Expected files jsou uloženy v `Snapshots/{Category}/{TestName}.expected.cs` a zkopírovány do bin při build.

Aktuální snapshot kategorie:
- **Class** (8 modifikátorů: C1-C8)
- **Struct** (4 varianty: S1-S4)
- **Enum** (4 varianty: E1-E4)
- **Property** (6 variant: P1-P5, P8; P6,P7 chybí)
- **Method** (5 E2E scénářů)
- **TypeModel** (18 variant)
- **Delegate** (4 varianty: D1-D4 — PROP-052)

## Support Matrix

Strojově čitelný YAML contract map v `Docs/Core/00-Support-Matrix.yaml` (73 položek).
Rozlišuje contract statusy: `public-supported`, `advanced`, `internal`, `experimental`.
Slouží jako rozhodovací vstup pro AI agenty. PROP-051.

---

## Test helper scaffold

### TestDocumentBuilder

```csharp
//context//
// Účel: Factory pro vytváření testovacích BusinessAuthoringDocument instancí s přednastavenými daty.
// Vrstva: Tests.
// Vstup: Fluent API pro konfiguraci testovacího dokumentu.
// Výstup: BusinessAuthoringDocument s požadovanými entitami a atributy.
// Závislosti: BusinessAuthoringDocument, BusinessEntityNode, BusinessAttributeNode.
// Nezávislosti: Nezdedí se z produkčního kódu — je to čistě test utility.
// Invarianty: Každý build musí vracet novou instanci. Žádné sdílení stavu mezi testy.
// Související typy: BusinessAuthoringDocument, PatchEngine (alternativní cesta vytvoření).
// Testy: Sám je test utility — netestuje se přímo.

public class TestDocumentBuilder
{
    public TestDocumentBuilder WithEntity(string name) { return this; }
    public TestDocumentBuilder WithAttribute(string entityName, string attrName, string type) { return this; }
    public TestDocumentBuilder WithRelation(string from, string to, string type) { return this; }
    public BusinessAuthoringDocument Build() { return new(); }
}
```

### TestCommandLogBuilder

```csharp
//context//
// Účel: Factory pro vytváření testovacích CommandLog sekvencí.
// Vrstva: Tests.
// Vstup: Fluent API pro přidávání commandů.
// Výstup: IReadOnlyList<CommandEnvelope> pro replay testy.
// Závislosti: CommandEnvelope.
// Nezávislosti: Nevyžaduje PatchEngine — commandy se vytváří přímo.
// Invarianty: Sekvence musí být validní pro replay (správné pořadí, konzistentní ID).
// Související typy: CommandEnvelope, ReplayEngine.
// Testy: Sám je test utility.

public class TestCommandLogBuilder
{
    public TestCommandLogBuilder AddEntity(string name) { return this; }
    public TestCommandLogBuilder AddAttribute(string entityId, string name, string type) { return this; }
    public IReadOnlyList<CommandEnvelope> Build() { return new List<CommandEnvelope>(); }
}
```

---

## Syntax validation generovaného kódu

Pro ověření, že generátor produkuje syntakticky korektní C#, se používá **Roslyn syntax parser** přímo v testech — bez plné kompilace.

### SyntaxValidator

```csharp
public static class SyntaxValidator
{
    /// <summary>Validuje C# syntaxi. Vrací true pokud je kód syntakticky korektní,
    /// jinak false + výpis chyb do diagnostics.</summary>
    public static bool IsValid(string sourceCode, out string diagnostics);
}
```

- **Soubor:** `Tests/MetaForge.Generators.Tests/SyntaxValidator.cs`
- **Použití:** `SyntaxValidator.IsValid(result.SourceCode, out var diag)`
- **Závislost:** `Microsoft.CodeAnalysis.CSharp` (pouze v test projektu)
- **Princip:** `CSharpSyntaxTree.ParseText()` parsuje syntaxi bez assembly referencí — detekuje chybějící závorky, špatné deklarace, atd.

### Příklad testu

```csharp
[Fact]
public void Generated_Code_IsValidCSharpSyntax()
{
    var cls = new ClassElement { Name = "Customer" };
    var result = _generator.Generate(cls);

    var isValid = SyntaxValidator.IsValid(result.SourceCode, out var diagnostics);
    isValid.Should().BeTrue($"syntax error:{Environment.NewLine}{diagnostics}");
}
```

### Co NENÍ validováno

| Typ validace | Proč ne |
|-------------|---------|
| Sémantická (neznámé typy, chybějící usingy) | Vyžaduje plnou kompilaci s referencemi |
| Logická (nekonečná rekurze) | Mimo rozsah generátoru |
| Stylistická (formátování) | Řeší formatter, ne generátor |

---

## Snapshot testování (PROP-032 — nové)

> Integrační testy Core → Generators používají **snapshot-based** přístup — vygenerovaný C# kód se porovnává se vzorovými `.expected.cs` soubory.

### SnapshotComparer

```csharp
// Soubor: Tests/MetaForge.Core.Integration.Tests/Snapshots/SnapshotComparer.cs
public static class SnapshotComparer
{
    /// <summary>Ověří shodu se snapshotem. First-run vytvoří .expected.cs.</summary>
    public static void Verify(string category, string testName, string generatedCode);
    
    /// <summary>Validuje syntaxi generovaného kódu přes Roslyn.</summary>
    public static void AssertValidSyntax(string generatedCode);
}
```

### Decision Matrix

Testovací scénáře jsou definovány v `Docs/Integration/01-Integration-Test-Matrix.md` — 102 řádků pokrývajících:
- Class modifikátory (abstract, sealed, static, partial, record)
- Enum varianty (underlying type, Flags)
- Struct varianty (readonly, record)
- Property modifikátory + TypeModel varianty
- Method modifikátory + AST body
- Statement hierarchie

Každý ✅ řádek = snapshot integrační test. Každý ❌ řádek = Core unit validation test.

### 3-vrstvá validace

| Vrstva | Nástroj | Účel |
|--------|---------|------|
| Snapshot | `SnapshotComparer.Verify()` | Detekce regresí v generovaném kódu |
| Syntax | `SnapshotComparer.AssertValidSyntax()` | Generovaný kód je platný C# |
| Content | `FluentAssertions.Should().Contain()` | Ověření konkrétních patternů |

### Core.Tests

| Test | Co ověřuje |
|------|-----------|
| `TypeModel_DefaultValues` | TypeModel s BaseType.String má IsNullable=false, IsCollection=false |
| `CatalogManager_RegisterAndResolve` | RegisterPreset + ResolveType vrací správný preset |
| `CatalogManager_ResolveUnknown_ReturnsNull` | Neznámý typ vrací null |
| `ForgeBlockRegistry_RegisterPackage` | ForgeBlock se registruje a je queryable |
| `ForgeBlockRegistry_DuplicateHandle_Throws` | Duplicitní handle vyhazuje výjimku |
| `ComputedExpression_Compose_BuildsCorrectTree` | Kompozice Expression uzlů vytvoří očekávaný strom |
| `ExpressionRendererRegistry_UnknownKind_ReturnsNull` | Neregistrovaný druh výrazu nevyhodí výjimku, vrací null |
| `RuleBasedConstraintInferencer_KnownPattern_InfersConstraints` | Název atributu odpovídající známému patternu vrátí očekávané constraints |
| `MethodBoundaryAnalyzer_Signature_InfersBoundary` | Analýza podpisu metody vrátí boundary rule |
| `StandardLibraryTranslatorRegistry_KnownOperation_ReturnsRequirements` | Známá sémantická operace vrátí StandardLibraryRequirements |
| `StandardLibraryTranslatorRegistry_UnknownOperation_ReturnsNull` | Neznámá operace vrátí null bez výjimky |
| `StrongType_InvalidUnderlyingType_FailsValidation` | StrongType s neplatným TypeModel selže validaci |

### BusinessModel.Tests

| Test | Co ověřuje |
|------|-----------|
| `PatchEngine_AddEntity_AddsToDocument` | AddEntityOp přidá entitu do dokumentu |
| `PatchEngine_AddEntity_CreatesLogEntry` | CommandLog se zvýší o 1 |
| `ReplayEngine_EmptyLog_EmptyDocument` | Replay prázdného logu = prázdný dokument |
| `ReplayEngine_NCommands_CorrectState` | Replay N commandů = expected state |
| `ReplayEngine_Deterministic` | Dva replay stejného logu = identický výstup |
| `CommandLogStore_AppendOnly` | Count nikdy neklesá, nelze deletovat |
| `DocumentValidator_EmptyEntityName_Fails` | Prázdný název entity nepropadne validací |

### Translator.Tests

| Test | Co ověřuje |
|------|-----------|
| `Facade_AddEntity_IntegratesWithPatchAndLog` | Celý write path přes facade |
| `Facade_GetProjection_ReturnsCurrentState` | Read path vrací aktuální stav |
| `Translator_KnownType_ReturnsCorrectTypeModel` | Email → TypeModel(String, ...) |
| `Translator_UnknownType_ReturnsObject` | Neznámý → TypeModel(Object) |
| `WriteBack_ApplyEnrichment_UpdatesCoreDetail` | Enrichment se propíše do atributu |
| `Facade_AddEntity_ThenProjection_Consistent` | Write + read = konzistentní |

### Generators.Tests

| Test | Co ověřuje |
|------|-----------|
| `CSharpGenerator_ClassElement_ValidOutput` | Generovaný C# obsahuje "public class" |
| `CSharpGenerator_WithProperties_GeneratesAll` | Všechny properties jsou v outputu |
| `CSharpGenerator_EmptyName_Throws` | Element bez Name vyhazuje výjimku |
| `CSharpGenerator_Namespace_Included` | Namespace se propaguje do using/namespace bloku |

---

## Testovací konvence

1. **Naming:** `{Třída}_{Metoda}_{Scénář}` nebo `{Třída}_{Scénář}_{Výsledek}`
2. **Arrange-Act-Assert** pattern vždy
3. **Jeden assert per test** (preferenčně, ne dogmaticky)
4. **TestDocumentBuilder** pro setup — ne ruční konstrukce
5. **xUnit** jako framework (konzistence s aktuálním repem)
6. **FluentAssertions** pro čitelnost
7. **Žádné mocking frameworky pokud nejsou nutné** — preferuj fakes a in-memory implementace

---

## Rozšířený plán: testovací pokrytí pro C#-first architekturu

> Následující sekce pokrývají mezery identifikované analýzou existujících testů (stav k 2026-07-04) proti nové C#-first architektuře.
> Zdroj: `03-Core-Abstractions.md` až `12-Host-Surfaces.md` + `27-ForgeBlock-External-Libraries.md`

---

### I. Nové testy v MetaForge.Core.Tests

#### Abstrakce — AppRoot, ProjectElement, AttributeElement

**Nový soubor:** `Core.Tests/Abstractions/AppRootTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `AppRoot_Default_NoProjects` | Nový AppRoot má prázdný seznam projektů |
| `AppRoot_AddProject_CountIncrements` | Přidání projektu zvýší Count |
| `AppRoot_MultipleProjects_AllAccessible` | Více projektů je iterovatelné |
| `AppRoot_SerializeRoundtrip_Equivalent` | JSON round-trip zachová strukturu |

**Nový soubor:** `Core.Tests/Abstractions/ProjectElementTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `ProjectElement_Default_NameEmpty` | Default Name = string.Empty |
| `ProjectElement_SetName_ReadsBack` | Nastavení jména se vrací správně |
| `ProjectElement_DefaultNamespace_Null` | Default Namespace = null |
| `ProjectElement_AddRootElement_CountIncrements` | RootElement se přidá a Count se zvýší |
| `ProjectElement_MixedElementTypes_KindsAreCorrect` | Class, Interface, Enum vedle sebe — Kind rozezná |
| `ProjectElement_Empty_NameValidationMessage` | Prázdný název = validační chyba |

**Nový soubor:** `Core.Tests/Abstractions/AttributeElementTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `AttributeElement_Default_NameEmpty` | Default Name = string.Empty |
| `AttributeElement_WithArguments_PreservesOrder` | Argumenty si zachovají pořadí |
| `AttributeElement_NullArguments_Whitelisted` | null argumenty jsou povolené |

**Rozšířit:** `Core.Tests/Abstractions/RootElementTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `RootElement_Usings_DefaultEmpty` | Default seznam usingů je prázdný |
| `RootElement_Attributes_DefaultEmpty` | Default seznam atributů je prázdný |
| `RootElement_AddAttribute_CountIncrements` | Přidání AttributeElement zvýší Count |
| `RootElement_Equality_ById` | Dva RootElementy se stejným Id jsou equal |

#### DataType — enum se 32 C# typy

**Nový soubor:** `Core.Tests/Abstractions/DataTypeTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `DataType_Enum_Has32Members` | Enum má přesně 32 hodnot |
| `DataType_AllValues_Unique` | Všechny hodnoty jsou unikátní |
| `DataType_Bool_IsZero` | Bool = 0 (výchozí) |
| `DataType_Int128_Exists` | Int128 je definovaný |
| `DataType_Half_Exists` | Half (System.Half) je definovaný |
| `DataType_NInt_NUInt_Exist` | Platform-dependent typy existují |
| `DataType_DateOnly_TimeOnly_Exist` | Časové typy existují |

#### TypeModel — sealed record s factory metodami

**Rozšířit:** `Core.Tests/TypeModels/TypeModelTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `TypeModel_IsVoid_OnlyWhenBaseTypeVoid` | IsVoid = true jen při BaseType.Void + žádné modifikátory |
| `TypeModel_MakeNullable_SetsIsNullable` | MakeNullable() nastaví IsNullable=true |
| `TypeModel_MakeCollection_SetsIsCollection` | MakeCollection() nastaví IsCollection=true |
| `TypeModel_WithCustomName_Preserves` | WithCustomName() zachová vlastní jméno |
| `TypeModel_WithGenericArg_AddsToList` | WithGenericArg() přidá do seznamu |
| `TypeModel_Immutability_MakeNullableCreatesNew` | MakeNullable() vrací nový record (with) |
| `TypeModel_Factory_Void_ReturnsCorrect` | TypeModel.Void má BaseType.Void |
| `TypeModel_Factory_String_ReturnsCorrect` | TypeModel.String má BaseType.String |
| `TypeModel_Factory_Int32_ReturnsCorrect` | TypeModel.Int32 má BaseType.Int32 |
| `TypeModel_Factory_Bool_ReturnsCorrect` | TypeModel.Bool má BaseType.Bool |
| `TypeModel_Factory_Of_Arbitrary` | TypeModel.Of(DataType.DateTime) vrací správný typ |
| `TypeModel_Chaining_MakeNullableMakeCollection` | Řetězení MakeNullable().MakeCollection() dává oba flagy |

#### AccessModifier a SemanticCollection

**Nový soubor:** `Core.Tests/Abstractions/AccessModifierTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `AccessModifier_Enum_Has6Members` | Public, Internal, Protected, Private, ProtectedInternal, PrivateProtected |
| `AccessModifier_Default_IsPublic` | Výchozí hodnota je Public |

**Nový soubor:** `Core.Tests/Abstractions/SemanticCollectionTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `SemanticCollection_Add_FiresChanged` | Add() spustí Changed event |
| `SemanticCollection_Remove_FiresChanged` | Remove() spustí Changed event |
| `SemanticCollection_Clear_FiresChanged` | Clear() spustí Changed event |
| `SemanticCollection_MultipleAdds_CountMatches` | Vícenásobné Add() — Count odpovídá |
| `SemanticCollection_NoSubscriber_NoException` | Bez subscriberu je Add() bez výjimky |

---

### II. Core elementy — Class, Interface, Enum, Struct, Property, Method, Parameter

> Zdroj: `04-Core-Elements.md`

**Nový soubor:** `Core.Tests/Elements/ClassElementTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `ClassElement_Kind_IsClass` | Kind = "class" |
| `ClassElement_Default_IsPublic_NotAbstract_NotSealed_NotStatic_NotPartial` | Výchozí flags |
| `ClassElement_BaseClassName_Null` | Výchozí BaseClassName = null |
| `ClassElement_ImplementedInterfaces_Empty` | Výchozí seznam interfaces je prázdný |
| `ClassElement_AddProperty_CountIncrements` | Přidání PropertyElement zvýší Count |
| `ClassElement_AddMethod_CountIncrements` | Přidání MethodElement zvýší Count |
| `ClassElement_SetIsAbstract_CannotBeSealed` | Abstract + Sealed = validační chyba |
| `ClassElement_SetIsStatic_CannotBeAbstract` | Static + Abstract = validační chyba |

**Nový soubor:** `Core.Tests/Elements/InterfaceElementTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `InterfaceElement_Kind_IsInterface` | Kind = "interface" |
| `InterfaceElement_Default_IsPublic` | Výchozí AccessModifier = Public |
| `InterfaceElement_AddProperty_CountIncrements` | Přidání property do interfacu |
| `InterfaceElement_AddMethod_CountIncrements` | Přidání metody do interfacu |
| `InterfaceElement_NoBaseClass` | Interface nemá BaseClassName |

**Nový soubor:** `Core.Tests/Elements/EnumElementTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `EnumElement_Kind_IsEnum` | Kind = "enum" |
| `EnumElement_Default_UnderlyingTypeInt32` | Výchozí UnderlyingType = Int32 |
| `EnumElement_IsFlags_False` | Výchozí IsFlags = false |
| `EnumElement_AddMember_CountIncrements` | Přidání EnumMemberElement |
| `EnumElement_SetUnderlyingType_Byte` | UnderlyingType lze změnit na Byte |
| `EnumElement_Member_AssignValue` | EnumMemberElement s explicitní hodnotou |
| `EnumElement_Member_AutoValue` | EnumMemberElement bez hodnoty |

**Nový soubor:** `Core.Tests/Elements/StructElementTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `StructElement_Kind_IsStruct` | Kind = "struct" |
| `StructElement_Default_NotReadOnly_NotRecord` | Výchozí flags |
| `StructElement_SetIsReadOnly_IsRecord_True` | ReadOnly record struct |
| `StructElement_AddProperty_CountIncrements` | Property v structu |
| `StructElement_AddMethod_CountIncrements` | Metoda v structu |

**Nový soubor:** `Core.Tests/Elements/PropertyElementTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `PropertyElement_Default_NameEmpty_TypeObject_Public_GetterSetter` | Výchozí hodnoty |
| `PropertyElement_SetIsInitOnly_HasSetterTrue` | Init-only: HasSetter = true, IsInitOnly = true |
| `PropertyElement_SetIsRequired_True` | Required flag |
| `PropertyElement_SetDefaultValue_String` | DefaultValue = "test" |
| `PropertyElement_DefaultValue_Null` | Výchozí DefaultValue = null |
| `PropertyElement_SetIsStatic_True` | Static property |
| `PropertyElement_Type_With_Nullable` | Typ s IsNullable=true |

**Nový soubor:** `Core.Tests/Elements/MethodElementTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `MethodElement_Default_ReturnTypeVoid_Public_NotStatic_NotAsync` | Výchozí hodnoty |
| `MethodElement_AddParameter_CountIncrements` | Přidání ParameterElement |
| `MethodElement_SetIsAsync_ReturnsTaskWrapped` | Async metoda s return typem |
| `MethodElement_SetIsAbstract_MustBeInAbstractClass` | Abstraktní metoda jen v abstraktní třídě |
| `MethodElement_SetIsVirtual_True` | Virtuální metoda |
| `MethodElement_SetIsOverride_True` | Override metoda |
| `MethodElement_SetBody_ExpressionBody` | Body = "=> ..." |
| `MethodElement_AddAttribute_CountIncrements` | Atribut na metodě |

**Nový soubor:** `Core.Tests/Elements/ParameterElementTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `ParameterElement_Default_NameEmpty_TypeObject_ModifierNone` | Výchozí hodnoty |
| `ParameterElement_SetModifier_Ref` | Ref parametr |
| `ParameterElement_SetModifier_Out` | Out parametr |
| `ParameterElement_SetModifier_In` | In parametr |
| `ParameterElement_SetModifier_Params` | Params parametr |
| `ParameterElement_SetDefaultValue` | Volitelný parametr s defaultem |
| `ParameterElement_DefaultValue_WithoutHasDefault` | DefaultValue ignorován pokud HasDefaultValue=false |

---

### III. Boundary Analysis — MethodBoundaryAnalyzer

> Zdroj: `05-Core-Behaviors.md` — 7 doménových analyzérů

**Nový soubor:** `Core.Tests/Inference/Boundary/MethodBoundaryAnalyzerTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `MethodBoundaryAnalyzer_Void_ReturnsEmptyInput` | Metoda bez parametrů — prázdný AnalysisResult |
| `MethodBoundaryAnalyzer_StringParam_LengthBoundary` | String parametr — navrhne StringLength boundary |
| `MethodBoundaryAnalyzer_IntParam_RangeBoundary` | Int parametr — navrhne NumberRange boundary |
| `MethodBoundaryAnalyzer_CollectionParam_ElementBoundary` | Kolekce — navrhne CollectionElement boundary |
| `MethodBoundaryAnalyzer_MultipleParams_AllAnalyzed` | Více parametrů — analýza pro každý |
| `MethodBoundaryAnalyzer_RefParam_Analyzed` | Ref parametr je analyzovaný |
| `MethodBoundaryAnalyzer_OutParam_Ignored` | Out parametr se ignoruje (výstup) |
| `MethodBoundaryAnalyzer_CustomTypeParam_NotNullBoundary` | Custom type — navrhne NotNull boundary |
| `MethodBoundaryAnalyzer_Pipeline_OrderPreserved` | Pořadí analyzérů je deterministické |

---

### IV. AI Layer — rozšířit Translator.Tests

> Zdroj: `09-AI-Layer.md` — IAiBackendAdapter, AiConstraintInferencer, AiTranslationService

**Nový soubor:** `Translator.Tests/AI/AiBackendAdapterTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `OllamaAdapter_CompleteAsync_ReturnsContent` | Ollama vrací content string |
| `OllamaAdapter_HttpError_ThrowsGracefully` | HTTP 500 vrací fallback, ne výjimku |
| `OllamaAdapter_MalformedJson_GracefulNull` | Špatný JSON → vrací null |
| `OpenAiAdapter_CompleteAsync_ReturnsContent` | OpenAI formát odpovědi se správně parsuje |
| `OpenAiAdapter_AuthHeader_Present` | Authorization: Bearer hlavička je odeslána |

**Nový soubor:** `Translator.Tests/AI/AiConstraintInferencerTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `AiConstraintInferencer_ValidJson_ReturnsConstraints` | AI vrátí validní JSON → constraints extrahovány |
| `AiConstraintInferencer_EmptyResponse_ReturnsEmpty` | AI vrátí prázdný string → prázdný seznam |
| `AiConstraintInferencer_InvalidJson_ReturnsEmpty` | AI vrátí nevalidní JSON → prázdný seznam (graceful) |
| `AiConstraintInferencer_Fallback_ToDeterministic` | Když AI selže, fallback na RuleBasedConstraintInferencer |

**Nový soubor:** `Translator.Tests/AI/AiTranslationServiceTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `AiTranslationService_TranslationIntent_ReturnsDto` | Intent "translate" → validní TranslationResult DTO |
| `AiTranslationService_MalformedResponse_ReturnsEmpty` | Špatná odpověď → prázdný výsledek |
| `AiTranslationService_DomainTerms_Preserved` | Doménové termíny se nezmění v překladu |
| `AiTranslationService_Prompt_ContainsSourceDocument` | Prompt obsahuje zdrojový dokument |

---

### V. Nový projekt: MetaForge.Generators.Tests

> **Nový projekt** — `Tests/MetaForge.Generators.Tests/MetaForge.Generators.Tests.csproj`
> Reference: `MetaForge.Generators`, `MetaForge.Core`
> Priorita: **Vysoká** (kritické pro C#-first codegen)

**Soubor:** `CSharpGeneratorTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `CSharpGenerator_ClassElement_GeneratesPublicClass` | Vygenerovaný kód obsahuje "public class" |
| `CSharpGenerator_ClassElement_IncludesNamespace` | Namespace je v generovaném kódu |
| `CSharpGenerator_InterfaceElement_GeneratesInterface` | "public interface" |
| `CSharpGenerator_EnumElement_GeneratesEnum` | "public enum" + členové |
| `CSharpGenerator_StructElement_GeneratesStruct` | "public struct" |
| `CSharpGenerator_RecordStruct_GeneratesRecordStruct` | "public readonly record struct" |
| `CSharpGenerator_ClassWithProperty_GeneratesProperty` | Property v generované třídě |
| `CSharpGenerator_Property_WithAccessModifier` | get; set; init; dle AccessModifier |
| `CSharpGenerator_Method_Void_GeneratesMethod` | public void Metoda() { } |
| `CSharpGenerator_Method_WithParameters_GeneratesAll` | Všechny parametry v signatuře |
| `CSharpGenerator_Method_Async_GeneratesTask` | async Task<T> |
| `CSharpGenerator_Method_Static_GeneratesStatic` | public static void |
| `CSharpGenerator_Attribute_CanonicalString` | Atribut se správnými argumenty |
| `CSharpGenerator_Using_Deduplication` | Duplicitní usingy jsou deduplikovány |
| `CSharpGenerator_AbstractClass_VirtualMethod` | Abstraktní třída s virtuální metodou |
| `CSharpGenerator_SealedClass_NoVirtual` | Sealed třída nemá virtual |
| `CSharpGenerator_PartialClass_GeneratesPartial` | Partial keyword |
| `CSharpGenerator_AllModifiers_ValidCombination` | Kombinace public static async Partial |
| `CSharpGenerator_Output_Compilable` | Generated code se zkompiluje (Roslyn in-memory) |
| `CSharpGenerator_EmptyName_ThrowsArgumentException` | Element bez jména → ArgumentException |

**Soubor:** `AppRootToProjectGeneratorTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `AppRoot_SingleProject_GeneratesCsproj` | .csproj output |
| `AppRoot_MultipleProjects_GeneratesAll` | Vícenásobný output |
| `AppRoot_ProjectWithNamespace_GeneratesCorrectNamespace` | Namespace z ProjectElement |
| `AppRoot_EmptyProject_GeneratesEmptyFolder` | Prázdný projekt → prázdný výstup |

**Soubor:** `TemplateManagerTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `TemplateManager_Render_SimpleModel` | Jednoduchý model → očekávaný string |
| `TemplateManager_Render_MissingVariable_NullOutput` | Chybějící proměnná → null/empty |
| `TemplateManager_RegisterTemplate_Overwrite` | Registrace existující šablony přepíše |
| `TemplateManager_UnknownTemplate_Throws` | Neznámá šablona → výjimka |

**Soubor:** `LanguageMappingTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `LanguageMapping_CSharp_FileExtensionCs` | C# → .cs |
| `LanguageMapping_CSharp_CommentPrefixDoubleSlash` | C# → // |
| `LanguageMapping_CSharp_SupportsPartialClasses` | C# má partial classes |
| `LanguageMapping_AllLanguages_UniqueExtensions` | Každý jazyk má unikátní příponu |

**Soubor:** `PackageManifestGeneratorTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `PackageManifest_ValidJson_Structure` | Vygenerovaný JSON má required fields |
| `PackageManifest_Capabilities_Listed` | Capabilities jsou v manifestu |
| `PackageManifest_Dependencies_Included` | NuGet dependencies v manifestu |

**Soubor:** `FullPipelineIntegrationTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `AppRoot_To_CompilableCode` | Celý flow: vytvoř AppRoot → přidej projekt → ClassElement → Property → Method → C# → Roslyn kompilace |
| `MultiProject_AllCompilable` | Více projektů v AppRoot → každý generuje kompilovatelný kód |
| `AppRoot_With_ForgeBlock_GeneratesImports` | ClassElement s Math capability → generuje `using System;` + `Math.Sqrt(...)` |

---

### VI. Nový projekt: MetaForge.Infrastructure.Tests

> **Nový projekt** — `Tests/MetaForge.Infrastructure.Tests/MetaForge.Infrastructure.Tests.csproj`
> Reference: `MetaForge.Infrastructure`, `MetaForge.BusinessModel`
> Priorita: **Střední**

**Soubor:** `JsonCommandLogRepositoryTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `Save_ValidLog_WritesFile` | CommandLog se uloží na disk |
| `Load_ExistingFile_ReturnsLog` | Načtení vrací identický log |
| `SaveThenLoad_RoundTrip_Equivalent` | Round-trip zachovává data |
| `Load_NonexistentFile_ReturnsEmpty` | Neexistující soubor → prázdný log |
| `Append_AddsToEnd` | Append přidá na konec existujícího logu |
| `Append_DoesNotOverwrite` | Append neztratí existující záznamy |
| `ConcurrentWrite_NoCorruption` | Paralelní zápis nezpůsobí poškození |

**Soubor:** `JsonDocumentRepositoryTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `Save_ValidDocument_WritesFile` | Document se uloží jako JSON |
| `Load_ExistingFile_ReturnsDocument` | Načtení vrací identický dokument |
| `SaveThenLoad_AllEntities_Preserved` | Všechny entity, atributy, relace po round-trip |
| `Load_NonexistentFile_ReturnsNull` | Neexistující soubor → null |
| `Serialization_VersionField_Present` | JSON obsahuje schema version |

**Soubor:** `InMemoryCommandLogRepositoryTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `InMemory_Save_LoadsBack` | Uložení do paměti → načtení zpět |
| `InMemory_Empty_ReturnsEmpty` | Prázdné úložiště → prázdný log |
| `InMemory_ThreadSafe` | Konkurenční zápis je bezpečný |
| `InMemory_Clear_Empties` | Clear() odstraní všechna data |

**Soubor:** `FileSystemProviderTests.cs`

| Test | Co ověřuje |
|------|-----------|
| `FileExists_ExistingFile_ReturnsTrue` | Existující soubor → true |
| `FileExists_NonexistentFile_ReturnsFalse` | Neexistující soubor → false |
| `CreateDirectory_Path_Creates` | Vytvoření adresáře |
| `GetFiles_ReturnsCorrectCount` | Počet souborů odpovídá |

---

### VII. Nové projekty: Host Surfaces

#### CLI — `Tests/MetaForge.Cli.Tests/`

> Reference: `MetaForge.Cli`, `MetaForge.BusinessModel`
> Priorita: **Nízká**

| Test | Co ověřuje |
|------|-----------|
| `AddEntityCommand_Parse_NameRequired` | Chybějící `--name` → chyba, exit code != 0 |
| `AddEntityCommand_Execute_AddsEntity` | Úspěšné přidání entity přes CLI |
| `ProjectionCommand_Execute_ReturnsFormattedText` | `projection` vrací formátovaný výstup |
| `ExportCommand_WithLanguage_GeneratesCode` | `export --language csharp` generuje C# |
| `ExportCommand_MissingLanguage_Error` | Chybějící `--language` → chyba |
| `CliOutputFormatter_Table_RendersCorrectly` | Tabulkový výstup |
| `CliOutputFormatter_JsonFlag_OutputsJson` | `--json` flag → JSON výstup |

#### MCP — `Tests/MetaForge.Mcp.Tests/`

> Reference: `MetaForge.Mcp`, `MetaForge.BusinessModel`
> Priorita: **Nízká**

| Test | Co ověřuje |
|------|-----------|
| `AddEntityTool_Execute_AddsEntity` | MCP tool přidá entitu |
| `AddEntityTool_Schema_ValidJsonSchema` | Tool schema je validní JSON Schema |
| `GetProjectionTool_ReturnsProjection` | Projekce přes MCP tool |
| `TranslateTool_MapsEntityCorrectly` | Translator přes MCP |
| `ExportTool_GeneratesCode` | Generování kódu přes MCP tool |
| `ToolDispatcher_UnknownTool_ReturnsError` | Neznámý tool → chybová odpověď |
| `ToolDispatcher_MissingRequiredParam_ReturnsError` | Chybějící povinný parametr |

#### WebApi — rozšířit `Tests/MetaForge.WebApi.Tests/`

| Test | Co ověřuje |
|------|-----------|
| `AuthoringController_AddEntity_Returns201` | POST vrací 201 Created |
| `AuthoringController_InvalidBody_Returns400` | POST s nevalidním body → 400 |
| `ProjectionController_GetProjection_Returns200` | GET projection → 200 + JSON |
| `ProjectionController_NonexistentDocument_Returns404` | Neexistující dokument → 404 |
| `ExportController_Export_Returns200` | GET export → 200 + kód |
| `ExportController_InvalidLanguage_Returns400` | Neznámý jazyk → 400 |
| `ErrorHandlingMiddleware_Exception_Returns500` | Neočekávaná výjimka → 500 + error JSON |
| `ErrorHandlingMiddleware_Validation_Returns400` | Validační chyba → 400 + detail |
| `RequestLoggingMiddleware_LogsRequest` | Každý request je logován |

---

### VIII. ForgeBlock Standard Library Wrappers — test projekty (14×)

> Zdroj: `27-ForgeBlock-External-Libraries.md` sekce 16
> Každý balíček potřebuje 4 testovací soubory dle šablony:

```
Src/ForgeBlocks/{Name}/Tests/MetaForge.ForgeBlocks.{Name}.Tests.csproj
├── {Name}ForgeBlockPackageTests.cs        // Descriptor, Register(), capability
├── {Name}StandardLibraryTranslatorTests.cs // Per-language mappings
├── Semantic{Name}Tests.cs                  // Canonical handle strings
└── {Name}IntegrationTests.cs               // End-to-end: registrace → codegen
```

**Referenční implementace:** `MetaForge.ForgeBlocks.Math.Tests`

| ForgeBlock | Testovaná operace | Očekávaný handle |
|-----------|-------------------|------------------|
| **Text** | Regex.Match, Regex.Replace, String.Format, Concat, Split, Join | `mf.text.regex-match(...)`, `mf.text.format(...)` |
| **Collections** | LINQ: Where, OrderBy, GroupBy, Distinct, Aggregate, First, Last | `mf.collection.filter(...)`, `mf.collection.sort(...)` |
| **Json** | JsonSerializer.Serialize, Deserialize, JsonNode | `mf.json.serialize(...)`, `mf.json.deserialize(...)` |
| **Xml** | XDocument.Parse, XPathSelectElements, Transform | `mf.xml.parse(...)`, `mf.xml.select(...)` |
| **Http** | HttpClient.GetAsync, PostAsync, PutAsync, DeleteAsync | `mf.http.get(...)`, `mf.http.post(...)` |
| **FileSystem** | File.ReadAllText, WriteAllText, Copy, Move, Delete | `mf.fs.read(...)`, `mf.fs.write(...)` |
| **Crypto** | SHA256.HashData, Aes.Encrypt, RSA.SignData | `mf.crypto.hash(...)`, `mf.crypto.encrypt(...)` |
| **Compression** | GZipStream, DeflateStream, ZipFile, BrotliStream | `mf.compress.gzip(...)`, `mf.compress.zip(...)` |
| **Encoding** | Convert.ToBase64String, Uri.EscapeDataString, WebUtility.HtmlEncode | `mf.encode.base64(...)`, `mf.encode.url(...)` |
| **DateTime** | DateTime.Now, Parse, ToString, AddDays, TimeZoneInfo | `mf.time.now(...)`, `mf.time.format(...)` |
| **Concurrency** | Task.Run, Parallel.ForEach, Channel, Lock | `mf.thread.parallel(...)`, `mf.thread.channel(...)` |
| **Numerics** | Vector.Add, Matrix.Multiply, Complex.Pow, BigInteger.Parse | `mf.num.vector(...)`, `mf.num.matrix(...)` |
| **Reflection** | Type.GetType, GetProperties, GetMethods, Activator.CreateInstance | `mf.reflect.type(...)`, `mf.reflect.property(...)` |
| **Environment** | Environment.OSVersion, GetEnvironmentVariable, Process.GetCurrentProcess | `mf.sys.os(...)`, `mf.sys.env(...)` |

---

### Shrnutí — co se musí vytvořit

| # | Projekt / Soubor | Typ | Priorita |
|---|-----------------|-----|----------|
| 1 | `Core.Tests/Abstractions/AppRootTests.cs` | Nový soubor | 🔴 Vysoká |
| 2 | `Core.Tests/Abstractions/ProjectElementTests.cs` | Nový soubor | 🔴 Vysoká |
| 3 | `Core.Tests/Abstractions/AttributeElementTests.cs` | Nový soubor | 🔴 Vysoká |
| 4 | `Core.Tests/Abstractions/DataTypeTests.cs` | Nový soubor | 🔴 Vysoká |
| 5 | `Core.Tests/Abstractions/AccessModifierTests.cs` | Nový soubor | 🔴 Vysoká |
| 6 | `Core.Tests/Abstractions/SemanticCollectionTests.cs` | Nový soubor | 🔴 Vysoká |
| 7 | Rozšířit `Core.Tests/Abstractions/RootElementTests.cs` | Rozšířit existující | 🔴 Vysoká |
| 8 | Rozšířit `Core.Tests/TypeModels/TypeModelTests.cs` | Rozšířit existující | 🔴 Vysoká |
| 9 | `Core.Tests/Elements/ClassElementTests.cs` | Nový soubor | 🔴 Vysoká |
| 10 | `Core.Tests/Elements/InterfaceElementTests.cs` | Nový soubor | 🔴 Vysoká |
| 11 | `Core.Tests/Elements/EnumElementTests.cs` | Nový soubor | 🔴 Vysoká |
| 12 | `Core.Tests/Elements/StructElementTests.cs` | Nový soubor | 🔴 Vysoká |
| 13 | `Core.Tests/Elements/PropertyElementTests.cs` | Nový soubor | 🔴 Vysoká |
| 14 | `Core.Tests/Elements/MethodElementTests.cs` | Nový soubor | 🔴 Vysoká |
| 15 | `Core.Tests/Elements/ParameterElementTests.cs` | Nový soubor | 🔴 Vysoká |
| 16 | `Core.Tests/Inference/Boundary/MethodBoundaryAnalyzerTests.cs` | Nový soubor | 🟡 Střední |
| 17 | `Translator.Tests/AI/AiBackendAdapterTests.cs` | Nový soubor | 🟡 Střední |
| 18 | `Translator.Tests/AI/AiConstraintInferencerTests.cs` | Nový soubor | 🟡 Střední |
| 19 | `Translator.Tests/AI/AiTranslationServiceTests.cs` | Nový soubor | 🟡 Střední |
| 20 | `MetaForge.Generators.Tests` (projekt + 6 tříd) | **Nový projekt** | 🔴 Vysoká |
| 21 | `MetaForge.Infrastructure.Tests` (projekt + 4 třídy) | **Nový projekt** | 🟡 Střední |
| 22 | `MetaForge.Cli.Tests` (projekt + 7 testů) | **Nový projekt** | 🟢 Nízká |
| 23 | `MetaForge.Mcp.Tests` (projekt + 7 testů) | **Nový projekt** | 🟢 Nízká |
| 24 | 14× ForgeBlock Standard Library test projektů | **Nové projekty** | 🔵 Průběžně |

> **Celkem:** 16 nových souborů v existujících projektech, 3 nové testovací projekty (Generators, Infrastructure, CLI/MCP), 14 ForgeBlock test projektů.

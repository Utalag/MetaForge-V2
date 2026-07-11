# PROP-047 Translator — Strong Type Mapping

Typ výsledku: Candidate Proposal
Zdroj podnětu: Architecture — follow-up k implementované infrastruktuře generátoru
Stav životního cyklu: Candidate
Rozhodovací owner:
Poslední revize: 2026-07-10

Priorita: High
Oblast: Translator
Owner:
Datum vytvoření: 2026-07-10
Aktualizováno: 2026-07-10

Navazuje na:
- Strong type infrastruktura v Core + Generator (Fáze 1, 2, 4 — hotovo 2026-07-10)

Blokuje:
- PROP-048 — Scénář 7 (EF/JSON konvertory)

Související soubory:
- `Src/MetaForge.Translator/Translation/DefaultBusinessTranslator.cs`
- `Src/MetaForge.Translator/Translation/IBusinessTranslator.cs`
- `Src/MetaForge.BusinessModel/Models/BusinessAuthoringDocument.cs`
- `Src/MetaForge.BusinessModel/Models/BusinessAttributeCoreDetail.cs`
- `Tests/MetaForge.Translator.Tests/`

## 1. Kontext

Strong type infrastruktura na úrovni Core (StructElement, ClassElement.InlineStrongTypes) a Generatoru (inline rendering, Scénář 6) je hotová. Chybí propojení mezi BusinessModelem a Core vrstvou — Translator, který by přečetl `CoreDetail.IsStrongType` a `CustomTypeDefinition` z business dokumentu a sestavil z nich Core elementy.

## 2. Problém dnes

- `DefaultBusinessTranslator.Translate(BusinessAttributeNode)` vrací `TypeModel` pouze pro primitivní typy (`string`, `int`, `decimal` atd.)
- `BusinessAttributeCoreDetail` má `IsStrongType` a `ValueObjectName` — ale Translator je nečte
- `CustomTypeDefinition` existuje v `BusinessAuthoringDocument` — ale není využit
- Chybí `TranslateDocument()` který by přeložil celý dokument najednou (entity → ClassElement, atributy → PropertyElement)
- Výsledkem: strong type pipeline končí u Core modelu — nejde ji otestovat end-to-end přes Translator

## 3. Cíl

- `DefaultBusinessTranslator.TranslateDocument()` přečte `CoreDetail.IsStrongType == true`
- Vyhledá `CustomTypeDefinition` podle `ValueObjectName`
- Vytvoří `StructElement.ReadOnlyRecord(valueObjectName)` s primary konstruktorem a `Value` property
- Nastaví `PropertyElement.Type = TypeModel.Of(DataType.Struct).WithCustomName("...")`
- Přidá struct do `ClassElement.InlineStrongTypes`
- Pokud `CoreDetail` chybí nebo `IsStrongType == null` → fallback na primitivum (graceful degradation)
- Translation source anotace: `RootElement.Metadata.Set("Generation.TranslationSource", "AI"/"Deterministic")`

## 4. Změny

### 4.1 `DefaultBusinessTranslator.cs`

**Nový helper:**
```csharp
private TypeModel MapBaseType(string baseType) => baseType.ToLowerInvariant() switch
{
    "string" or "text" => TypeModel.String,
    "int" or "integer" or "int32" => TypeModel.Int32,
    "long" or "int64" => TypeModel.Of(DataType.Int64),
    "decimal" or "money" => TypeModel.Decimal,
    "double" => TypeModel.Of(DataType.Double),
    "bool" => TypeModel.Bool,
    "datetime" => TypeModel.DateTime,
    "guid" => TypeModel.Guid,
    _ => TypeModel.Object,
};
```

**Upravený `TranslateDocument()` — pokud existuje:**
```csharp
public IReadOnlyList<RootElement> TranslateDocument(BusinessAuthoringDocument document)
{
    var result = new List<RootElement>();
    var translationSource = DetermineTranslationSource(document);
    
    foreach (var entity in document.Entities)
    {
        var classElement = new ClassElement { Name = entity.Name };
        
        // Translation source
        if (translationSource != null)
            classElement.Metadata.Set("Generation.TranslationSource", translationSource);
        
        foreach (var attr in entity.Attributes)
        {
            if (attr.CoreDetail?.IsStrongType == true && attr.CoreDetail?.ValueObjectName != null)
            {
                // Najít CustomTypeDefinition
                var ctd = document.CustomTypes
                    .FirstOrDefault(ct => ct.Name == attr.CoreDetail.ValueObjectName);
                if (ctd != null)
                {
                    // Vytvořit strong type struct
                    var baseType = MapBaseType(ctd.BaseType);
                    var st = StructElement.ReadOnlyRecord(ctd.Name)
                        .WithPrimaryConstructor(new ParameterElement { Name = "Value", Type = baseType })
                        .WithProperty(PropertyElement.GetOnly("Value", baseType));
                    
                    // Přidat validační pravidla z CustomTypeDefinition
                    foreach (var rule in ctd.ValidationRules)
                        st.Metadata.Set("Validation.Rule", rule);
                    
                    // Property s odkazem na strong type
                    classElement.InlineStrongTypes.Add(st);
                    classElement.Properties.Add(PropertyElement.GetSet(attr.Name,
                        TypeModel.Of(DataType.Struct).WithCustomName(ctd.Name)));
                    continue;
                }
            }
            
            // Fallback na primitivum
            classElement.Properties.Add(PropertyElement.GetSet(attr.Name, Translate(attr)));
        }
        
        result.Add(classElement);
    }
    
    return result;
}
```

**Nový helper `DetermineTranslationSource()`:**
```csharp
private string? DetermineTranslationSource(BusinessAuthoringDocument document)
{
    // Z dokumentu/metadat zjistit, zda šlo o AI překlad
    // Pokud existuje CommandProvenance s Mode == "ai-generated" → "AI"
    // Jinak → "Deterministic"
    return "Deterministic"; // Výchozí, dokud AI vrstva nezapíše metadata
}
```

### 4.2 Rozšíření `IBusinessTranslator` (pokud je potřeba)

Pokud `TranslateDocument()` není v interfacu, přidat:
```csharp
IReadOnlyList<RootElement> TranslateDocument(BusinessAuthoringDocument document);
```

## 5. Testy

### Scénář A: Strong type mapping
- Vytvořit `BusinessAuthoringDocument` s 1 entitou, 1 atributem s `CoreDetail.IsStrongType=true` a `CustomTypeDefinition`
- Zavolat `TranslateDocument()`
- Ověřit: `ClassElement.InlineStrongTypes` obsahuje 1 struct
- Ověřit: `PropertyElement.Type.CustomTypeName == "PhoneNumber"`

### Scénář B: Fallback na primitiva
- Vytvořit dokument bez `CoreDetail` (null)
- Zavolat `TranslateDocument()`
- Ověřit: `InlineStrongTypes` je prázdný
- Ověřit: `PropertyElement.Type == TypeModel.String`

### Scénář C: Translation source
- Ověřit, že element má `Metadata.Get("Generation.TranslationSource")`

## 6. Architektonické invarianty

- **Translator je deterministický** — `TranslateDocument()` **neinferuje** strong types, pouze čte `CoreDetail.IsStrongType`
- Kdo zapisuje `CoreDetail.IsStrongType`: **AI enrichment pipeline v téže Translator vrstvě** — `DefaultBusinessTranslator.TryEnrichAsync()` → `IAiTranslator` → `EnrichmentResult` → `WriteBackService.WriteCoreDetail()`. To vše v MetaForge.Translator.
- Bez AI = primitiva (graceful fallback) — `IsStrongType` zůstane null nebo false
- Translation source anotace je metadata, ne business logika
- Core elementy jsou immutable po vytvoření
- Translator nemá závislost na Generatoru

## 7. Otevřené otázky

- ~~Jak přesně AI vrstva zapisuje CoreDetail.IsStrongType=true?~~ ✅ AI enrichment pipeline v Translatoru (`TryEnrichAsync` → `WriteBackService.WriteCoreDetail`)
- Má `TranslateDocument` řešit i relace (BusinessRelationNode → implementované interfacy)?
- Jaký formát má `CommandProvenance.Model` pro identifikaci AI modelu?

## 8. Akceptační kritéria

- [ ] `dotnet test Tests/MetaForge.Translator.Tests/` — všechny testy procházejí
- [ ] End-to-end: BusinessModel → Translator → Core → Generator → .cs (kompilabilní)
- [ ] Fallback: bez CoreDetail → primitiva, ne pády
- [ ] Translation source metadata je korektně nastavena

## Odhad
1–2 dny implementace + testy

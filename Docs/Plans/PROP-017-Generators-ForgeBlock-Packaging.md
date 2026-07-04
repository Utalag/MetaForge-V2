# PROP-017: Generators — ForgeBlock packaging a katalog

> **Stav:** 📝 Navrženo
> **Datum:** 2026-07-04
> **Autor:** Copilot

## Cíl

Vybudovat v Generators vrstvě metadata/packaging subsystém pro ForgeBlock balíky, který umožní jejich publikaci do katalogu a generování doprovodných manifestů.

## Odůvodnění

ForgeBlock balíky (Math, String, Validation atd.) potřebují:
- Metadata o svých capabilities (ID, kategorie, tagy, podporované jazyky)
- Katalogové výpisy pro discovery v IDE / CLI
- Instalační deskriptory (odkaz na NuGet balíček)
- Language mappingy (rosetta stone) pro sémantické operace

Aktuální `CodeGenerator` generuje C# kód, ale neví o tom, jaké ForgeBlock balíčky jsou k dispozici a co nabízejí.

## Obsah

### 1. BlueprintBuilder

Staví metadata deskriptory pro ForgeBlock:

| Model | Popis |
|-------|-------|
| `ForgeBlockBlueprintCapabilityDescriptor` | ID, displayName, description, category, kind, tags, semantic handles, supported languages, dependencies |
| `ForgeBlockBlueprintCatalogEntryDescriptor` | EntryId, displayName, category, kind, tags, semantic handles, related capabilities |
| `ForgeBlockBlueprintDiscoveryItemDescriptor` | Id, tags, usage example, returns |
| `ForgeBlockBlueprintLanguageMapping` | Language → mapping funkcí a importů |

Builder poskytuje fluent API: `.WithCapability(...)`, `.WithCatalogEntry(...)`, `.WithDiscoveryItem(...)`.

### 2. CatalogPreviewBuilder

Generuje katalogové výpisy pro marketplace / CLI:

| Model | Popis |
|-------|-------|
| `ForgeBlockCatalogListingDescriptor` | PackageId, displayName, version, description, tags, category, capabilities, install info |
| `ForgeBlockCatalogInstallDescriptor` | Source kind, source name, packageId, version |
| `ForgeBlockCatalogPreviewIndex` | Source info + seznam listingů |
| `ForgeBlockCatalogPreviewOptions` | Source kind, visibility, version override |

### 3. BuiltInBootstrap

Bootstrappuje vestavěné ForgeBlock balíky při startu generátoru.

### 4. Integrace do CodeGeneratoru

`CodeGenerator` bude při generování (např. třídy s atributem `[Email]`):
1. Dotázat `ForgeBlockRegistry` na capability
2. Získat potřebné usingy a NuGet balíčky
3. Přidat je do `GeneratedCodeArtifact.RequiredPackages`

Příklad:
```
ClassElement s Property "Email" typu string
  → ForgeBlockRegistry vyhledá capability "validation-email"
  → Přidá using "FluentValidation"
  → Přidá CodePackageDependency("FluentValidation", "11.x")
```

## Závislosti

| Komponenta | Stav |
|------------|------|
| `ForgeBlockRegistry` (Core) | ✅ Hotovo |
| `IForgeBlockPackage`, `DiscoveryMetadata` (Core) | ✅ Hotovo |
| `ForgeBlockCapability`, `ForgeBlockPackageDescriptor` (Core) | ✅ Hotovo |
| `PackageManifestGenerator` (Generators) | ✅ Částečně — generuje .props, chybí propojení s registry |
| ForgeBlock balíky (Math, String, Validation) | ❌ Zatím neexistují |

## Výstup

| Soubor | Umístění |
|--------|----------|
| Blueprint modely + builder | `Src/MetaForge.Generators/ForgeBlockPackages/` |
| Catalog preview modely + builder | `Src/MetaForge.Generators/ForgeBlockPackages/` |
| BuiltIn bootstrap | `Src/MetaForge.Generators/ForgeBlockPackages/` |
| Rozšíření `CodeGenerator` | `Src/MetaForge.Generators/CodeGenerator.cs` |
| Rozšíření `GeneratedCodeArtifact` | přidat `RequiredPackages` property |
| Testy | `Tests/MetaForge.Generators.Tests/ForgeBlockPackages/` |

## Odhad

| Fáze | Dny |
|------|-----|
| Blueprint modely + builder | 1 den |
| Catalog preview modely + builder | 1 den |
| BuiltIn bootstrap | 0,5 dne |
| Integrace do CodeGeneratoru | 1 den |
| Testy | 0,5 dne |
| **Celkem** | **3-4 dny** |

## Spouštěč implementace

Až budou existovat první ForgeBlock balíky (Math, String, Validation) a bude potřeba je distribuovat nebo registrovat do katalogu.

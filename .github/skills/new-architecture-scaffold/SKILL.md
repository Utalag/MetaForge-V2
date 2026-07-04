---
name: new-architecture-scaffold
description: "Pouzij pri: vytvareni novych projektu a slozek dle cilove struktury Nove Architektury — MetaForge.slnx, projekty v Src/, Testy/, ForgeBlocks/."
---

# new-architecture-scaffold

Poskytnout referenci pro vytváření projektů a složek dle `02-Target-Repo-Structure.md` a `26-Scaffold-Projects-and-Folders.md`.

## Kdy použít

- Při zakládání nového projektu (solution, csproj)
- Při přidávání nového projektu do existujícího solution
- Při vytváření adresářové struktury pro novou vrstvu
- Při kontrole namespace konvencí

## Cílová struktura

```
MetaForge/
├── .github/
│   ├── agents/
│   ├── instructions/
│   └── skills/
├── Src/
│   ├── MetaForge.Core/
│   ├── MetaForge.BusinessModel/
│   ├── MetaForge.Translator/
│   ├── MetaForge.Infrastructure/
│   ├── MetaForge.Cli/
│   ├── MetaForge.Mcp/
│   ├── MetaForge.WebApi/
│   ├── MetaForge.Generators/
│   ├── MetaForge.Ai/
│   └── ForgeBlocks/
│       ├── Math/
│       ├── String/
│       └── Validation/
├── Tests/
│   ├── MetaForge.Core.Tests/
│   ├── MetaForge.BusinessModel.Tests/
│   ├── MetaForge.Translator.Tests/
│   ├── MetaForge.Generators.Tests/
│   └── MetaForge.WebApi.Tests/
├── Docs/
│   ├── Architecture/
│   └── Plans/
├── PROPOSALS.md
├── PROPOSALS_NEXT.md
├── Progress.md
├── Memories.md
├── README.md
└── MetaForge.slnx
```

## Vzor .csproj — Class Library

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>MetaForge.Core</RootNamespace>
  </PropertyGroup>
</Project>
```

## Vzor .csproj — Konzolová aplikace

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>MetaForge.Cli</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\MetaForge.Translator\MetaForge.Translator.csproj" />
  </ItemGroup>
</Project>
```

## Vzor .csproj — Test projekt

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\Src\MetaForge.Core\MetaForge.Core.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="FluentAssertions" Version="6.12.1" />
  </ItemGroup>
</Project>
```

## Namespace konvence

| Projekt | Namespace |
|---------|-----------|
| MetaForge.Core | `MetaForge.Core` |
| Core.Abstractions | `MetaForge.Core.Abstractions` |
| Core.DataTypes | `MetaForge.Core.DataTypes` |
| Core.Elements | `MetaForge.Core.Elements` |
| Core.Elements.Types | `MetaForge.Core.Elements.Types` |
| Core.Elements.Members | `MetaForge.Core.Elements.Members` |
| Core.Elements.Expressions | `MetaForge.Core.Elements.Expressions` |
| Core.Catalog | `MetaForge.Core.Catalog` |
| Core.ForgeBlockPackages | `MetaForge.Core.ForgeBlockPackages` |
| Core.ValueObjects | `MetaForge.Core.ValueObjects` |
| Core.Inference | `MetaForge.Core.Inference` |
| Core.StandardLibraries | `MetaForge.Core.StandardLibraries` |
| MetaForge.BusinessModel | `MetaForge.BusinessModel` |
| BusinessModel.Models | `MetaForge.BusinessModel.Models` |
| BusinessModel.CommandLog | `MetaForge.BusinessModel.CommandLog` |
| BusinessModel.Patches | `MetaForge.BusinessModel.Patches` |
| MetaForge.Translator | `MetaForge.Translator` |
| Translator.Translation | `MetaForge.Translator.Translation` |
| Translator.Host | `MetaForge.Translator.Host` |
| MetaForge.Generators | `MetaForge.Generators` |
| Generators.CSharp | `MetaForge.Generators.CSharp` |
| MetaForge.Ai | `MetaForge.Ai` |
| Ai.Abstractions | `MetaForge.Ai.Abstractions` |
| Ai.Adapters | `MetaForge.Ai.Adapters` |
| Ai.Inference | `MetaForge.Ai.Inference` |
| Ai.Translation | `MetaForge.Ai.Translation` |
| ForgeBlocks.{Name} | `MetaForge.ForgeBlocks.{Name}` |

## Solution (slnx) struktura

```xml
<Solution>
  <Folder Name="/Src/">
    <Project Path="Src/MetaForge.Core/MetaForge.Core.csproj" />
    <Project Path="Src/MetaForge.BusinessModel/MetaForge.BusinessModel.csproj" />
    <!-- ... -->
  </Folder>
  <Folder Name="/Src/ForgeBlocks/">
    <Project Path="Src/ForgeBlocks/Math/MetaForge.ForgeBlocks.Math.csproj" />
  </Folder>
  <Folder Name="/Tests/">
    <Project Path="Tests/MetaForge.Core.Tests/MetaForge.Core.Tests.csproj" />
  </Folder>
</Solution>
```

## Anti-patterny

- ❌ Vytváření projektů mimo definovanou strukturu
- ❌ Špatné namespace (např. `MetaForge.Core` místo `MetaForge.Core.Abstractions`)
- ❌ Chybějící test project k novému projektu
- ❌ Projekt bez odpovídajícího skillu v `.github/skills/`

## Výstupní checklist

- [ ] Solution odráží cílovou strukturu
- [ ] Všechny projekty mají správné namespace
- [ ] Každý Src projekt má odpovídající test project
- [ ] ForgeBlocky jsou ve vlastní složce `Src/ForgeBlocks/`
- [ ] Governance soubory (PROPOSALS.md atd.) existují

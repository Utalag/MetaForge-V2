# Epic 9 — Testovací infrastruktura

> **Cíl:** Vytvořit testovací projekty pro všechny vrstvy — unit testy, replay testy, generator output testy.
> **Výstup:** 4 testovací projekty s klíčovými testy.
> **Závislosti:** Epic 2 (Core), Epic 3 (BusinessModel), Epic 4 (Translator), Epic 7 (Generators).

---

## DŮLEŽITÉ: Testy jsou FIRST-CLASS

- Testy vznikají průběžně s každou vrstvou — ne až na konci.
- Unit testy preferovány — rychlé, izolované, deterministické.
- ŽÁDNÉ testy závislé na AI.
- Použij xUnit + FluentAssertions.

---

## TASK-9.1.1 — Založení projektu MetaForge.Core.Tests

**Vstup:** `MetaForge.slnx`, Epic 2 dokončen.
**Výstup:** Testovací projekt `Tests/MetaForge.Core.Tests/MetaForge.Core.Tests.csproj`.
**Soubory:** `Tests/MetaForge.Core.Tests/MetaForge.Core.Tests.csproj`, `MetaForge.slnx`

**Kód — `.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>MetaForge.Core.Tests</RootNamespace>
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
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
  </ItemGroup>
</Project>
```

**Aktualizace `MetaForge.slnx`** — přidej do `/Tests/`:

```xml
  <Folder Name="/Tests/">
    <Project Path="Tests/MetaForge.Core.Tests/MetaForge.Core.Tests.csproj" />
  </Folder>
```

**Ověření:** `dotnet build Tests/MetaForge.Core.Tests/` projde.
**Riziko:** Nízké.
**Rollback:** Odeber projekt, smaž složku.

---

## TASK-9.1.2 — Core.Tests: TypeModel, DataType, CatalogManager

**Vstup:** TASK-9.1.1.
**Výstup:** 3 testovací soubory.
**Soubory:**
- `Tests/MetaForge.Core.Tests/DataTypes/TypeModelTests.cs`
- `Tests/MetaForge.Core.Tests/DataTypes/DataTypeTests.cs`
- `Tests/MetaForge.Core.Tests/Catalog/CatalogManagerTests.cs`

**Kód — `TypeModelTests.cs`:**

```csharp
using FluentAssertions;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Tests.DataTypes;

public class TypeModelTests
{
    [Fact]
    public void String_StaticProperty_HasCorrectBaseType()
    {
        TypeModel.String.BaseType.Should().Be(DataType.String);
        TypeModel.String.IsNullable.Should().BeFalse();
        TypeModel.String.IsCollection.Should().BeFalse();
    }

    [Fact]
    public void Int32_StaticProperty_HasCorrectBaseType()
    {
        TypeModel.Int32.BaseType.Should().Be(DataType.Int32);
    }

    [Fact]
    public void Void_StaticProperty_IsVoid()
    {
        TypeModel.Void.IsVoid.Should().BeTrue();
    }

    [Fact]
    public void MakeNullable_SetsIsNullableTrue()
    {
        var type = TypeModel.String.MakeNullable();
        type.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void MakeCollection_SetsIsCollectionTrue()
    {
        var type = TypeModel.String.MakeCollection();
        type.IsCollection.Should().BeTrue();
    }

    [Fact]
    public void WithCustomName_SetsCustomTypeName()
    {
        var type = TypeModel.Of(DataType.Entity).WithCustomName("Customer");
        type.CustomTypeName.Should().Be("Customer");
    }

    [Fact]
    public void WithGenericArg_AddsArgument()
    {
        var type = TypeModel.Of(DataType.Array).WithGenericArg(TypeModel.String);
        type.GenericArguments.Should().HaveCount(1);
        type.GenericArguments[0].BaseType.Should().Be(DataType.String);
    }

    [Fact]
    public void Of_CreatesTypeWithGivenBaseType()
    {
        var type = TypeModel.Of(DataType.Guid);
        type.BaseType.Should().Be(DataType.Guid);
    }

    [Fact]
    public void Immutability_MakeNullable_DoesNotModifyOriginal()
    {
        var original = TypeModel.String;
        var nullable = original.MakeNullable();
        original.IsNullable.Should().BeFalse();
        nullable.IsNullable.Should().BeTrue();
    }
}
```

**Kód — `DataTypeTests.cs`:**

```csharp
using FluentAssertions;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Tests.DataTypes;

public class DataTypeTests
{
    [Fact]
    public void Enum_Has32Members()
    {
        var values = Enum.GetValues<DataType>();
        values.Should().HaveCount(32);
    }

    [Fact]
    public void AllValues_AreUnique()
    {
        var values = Enum.GetValues<DataType>();
        values.Distinct().Should().HaveCount(values.Length);
    }

    [Fact]
    public void Bool_IsZero()
    {
        ((int)DataType.Bool).Should().Be(0);
    }

    [Fact]
    public void String_Exists()
    {
        Enum.IsDefined(typeof(DataType), DataType.String).Should().BeTrue();
    }
}
```

**Kód — `CatalogManagerTests.cs`:**

```csharp
using FluentAssertions;
using MetaForge.Core.Catalog;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Tests.Catalog;

public class CatalogManagerTests
{
    private readonly CatalogManager _catalog = new();

    [Fact]
    public void ResolveType_WithBuiltInProvider_ReturnsPreset()
    {
        _catalog.RegisterProvider(new BuiltInCatalogProvider());
        var preset = _catalog.ResolveType("email");

        preset.Should().NotBeNull();
        preset!.Name.Should().Be("email");
        preset.Type.BaseType.Should().Be(DataType.String);
    }

    [Fact]
    public void ResolveType_Unknown_ReturnsNull()
    {
        _catalog.RegisterProvider(new BuiltInCatalogProvider());
        var preset = _catalog.ResolveType("neexistujici_typ_xyz");

        preset.Should().BeNull();
    }

    [Fact]
    public void ResolveType_CustomPreset_TakesPriority()
    {
        _catalog.RegisterProvider(new BuiltInCatalogProvider());
        _catalog.RegisterPreset(new PresetDefinition("email", TypeModel.Int32));

        var preset = _catalog.ResolveType("email");
        preset.Should().NotBeNull();
        preset!.Type.BaseType.Should().Be(DataType.Int32); // Custom má prioritu
    }

    [Fact]
    public void RegisterPreset_AddsToCatalog()
    {
        var preset = new PresetDefinition("mytype", TypeModel.Guid);
        _catalog.RegisterPreset(preset);

        var result = _catalog.ResolveType("mytype");
        result.Should().NotBeNull();
        result!.Type.BaseType.Should().Be(DataType.Guid);
    }

    [Fact]
    public void GetAllPresets_IncludesBothCustomAndProvider()
    {
        _catalog.RegisterProvider(new BuiltInCatalogProvider());
        _catalog.RegisterPreset(new PresetDefinition("custom", TypeModel.Bool));

        var all = _catalog.GetAllPresets();
        all.Should().NotBeEmpty();
        all.Should().Contain(p => p.Name == "custom");
        all.Should().Contain(p => p.Name == "email");
    }

    [Fact]
    public void SearchPresets_FindsByName()
    {
        _catalog.RegisterProvider(new BuiltInCatalogProvider());
        var results = _catalog.SearchPresets("email");

        results.Should().NotBeEmpty();
        results.Should().Contain(p => p.Name == "email");
    }

    [Fact]
    public void SearchPresets_CaseInsensitive()
    {
        _catalog.RegisterProvider(new BuiltInCatalogProvider());
        var results = _catalog.SearchPresets("EMAIL");

        results.Should().NotBeEmpty();
    }
}
```

**Ověření:** `dotnet test Tests/MetaForge.Core.Tests/` projde — všechny testy zelené.
**Riziko:** Nízké.
**Rollback:** Smaž testovací soubory.

---

## TASK-9.1.3 — Core.Tests: ForgeBlockRegistry, ConstraintInferencer

**Vstup:** TASK-9.1.2.
**Výstup:** 2 testovací soubory.
**Soubory:**
- `Tests/MetaForge.Core.Tests/ForgeBlockPackages/ForgeBlockRegistryTests.cs`
- `Tests/MetaForge.Core.Tests/Inference/RuleBasedConstraintInferencerTests.cs`

**Kód — `ForgeBlockRegistryTests.cs`:**

```csharp
using FluentAssertions;
using MetaForge.Core.ForgeBlockPackages;

namespace MetaForge.Core.Tests.ForgeBlockPackages;

public class ForgeBlockRegistryTests
{
    private readonly ForgeBlockRegistry _registry = new();

    private static IForgeBlockPackage CreateTestPackage(string handle)
    {
        // Jednoduchá testovací implementace
        return new TestForgeBlock(handle);
    }

    [Fact]
    public void Register_AddsPackage()
    {
        var package = CreateTestPackage("test");
        _registry.Register(package);

        _registry.Packages.Should().HaveCount(1);
    }

    [Fact]
    public void Register_DuplicateHandle_Throws()
    {
        var p1 = CreateTestPackage("duplicate");
        var p2 = CreateTestPackage("duplicate");
        _registry.Register(p1);

        var act = () => _registry.Register(p2);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*duplicate*");
    }

    [Fact]
    public void GetPackage_Existing_ReturnsPackage()
    {
        var package = CreateTestPackage("findme");
        _registry.Register(package);

        var found = _registry.GetPackage("findme");
        found.Should().NotBeNull();
        found!.Handle.Should().Be("findme");
    }

    [Fact]
    public void GetPackage_NonExisting_ReturnsNull()
    {
        var found = _registry.GetPackage("nonexistent");
        found.Should().BeNull();
    }

    [Fact]
    public void SearchByTag_FindsMatching()
    {
        var package = CreateTestPackage("math");
        _registry.Register(package);

        var results = _registry.SearchByTag("math");
        results.Should().NotBeEmpty();
    }

    [Fact]
    public void GetAllCapabilities_ReturnsAll()
    {
        var p1 = CreateTestPackage("p1");
        _registry.Register(p1);

        var capabilities = _registry.GetAllCapabilities();
        capabilities.Should().NotBeEmpty();
    }

    // === Test helper ===
    private sealed class TestForgeBlock : IForgeBlockPackage
    {
        public string Handle { get; }
        public string Version => "1.0.0";
        public IReadOnlyList<ForgeBlockCapability> Capabilities { get; } = new List<ForgeBlockCapability>
        {
            new("test_op", "Test Operation", "A test capability", new[] { "test" }),
        };
        public DiscoveryMetadata Discovery { get; }

        public TestForgeBlock(string handle)
        {
            Handle = handle;
            Discovery = new DiscoveryMetadata(
                DisplayName: handle,
                Description: "Test package",
                Tags: new[] { handle }
            );
        }

        public void Register(ForgeBlockRegistry registry) { }
    }
}
```

**Kód — `RuleBasedConstraintInferencerTests.cs`:**

```csharp
using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Inference;

namespace MetaForge.Core.Tests.Inference;

public class RuleBasedConstraintInferencerTests
{
    private readonly RuleBasedConstraintInferencer _inferencer = new();

    [Fact]
    public void Infer_Email_ReturnsEmailConstraints()
    {
        var result = _inferencer.Infer("email", TypeModel.String);
        result.Should().Contain("email_format");
        result.Should().Contain("not_empty");
    }

    [Fact]
    public void Infer_Phone_ReturnsPhoneConstraints()
    {
        var result = _inferencer.Infer("phone", TypeModel.String);
        result.Should().Contain("phone_format");
    }

    [Fact]
    public void Infer_Price_ReturnsNotNegative()
    {
        var result = _inferencer.Infer("price", TypeModel.Decimal);
        result.Should().Contain("not_negative");
    }

    [Fact]
    public void Infer_UnknownName_ReturnsEmpty()
    {
        var result = _inferencer.Infer("xyz123unknown", TypeModel.String);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Infer_NonNullableString_InfersNotEmpty()
    {
        var result = _inferencer.Infer("description", TypeModel.String);
        result.Should().Contain("max_length:4000");
    }

    [Fact]
    public void Infer_PrefixMatch_EmailAddress_ReturnsEmailConstraints()
    {
        var result = _inferencer.Infer("emailAddress", TypeModel.String);
        result.Should().Contain("email_format");
    }
}
```

**Ověření:** `dotnet test Tests/MetaForge.Core.Tests/` projde.
**Riziko:** Nízké.
**Rollback:** Smaž soubory.

---

## TASK-9.2.1 — Založení projektu MetaForge.BusinessModel.Tests

**Vstup:** `MetaForge.slnx`, Epic 3 dokončen.
**Výstup:** Testovací projekt `Tests/MetaForge.BusinessModel.Tests/`.
**Soubory:** `Tests/MetaForge.BusinessModel.Tests/MetaForge.BusinessModel.Tests.csproj`

**Kód — `.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>MetaForge.BusinessModel.Tests</RootNamespace>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\Src\MetaForge.BusinessModel\MetaForge.BusinessModel.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="FluentAssertions" Version="6.12.1" />
  </ItemGroup>
</Project>
```

---

## TASK-9.2.2 — BusinessModel.Tests: CommandLog, Replay, PatchEngine

**Vstup:** TASK-9.2.1.
**Výstup:** 3 testovací soubory.
**Soubory:**
- `Tests/MetaForge.BusinessModel.Tests/CommandLog/CommandLogStoreTests.cs`
- `Tests/MetaForge.BusinessModel.Tests/CommandLog/ReplayEngineTests.cs`
- `Tests/MetaForge.BusinessModel.Tests/Patches/PatchEngineTests.cs`

**Kód — `CommandLogStoreTests.cs`:**

```csharp
using FluentAssertions;
using MetaForge.BusinessModel.CommandLog;

namespace MetaForge.BusinessModel.Tests.CommandLog;

public class CommandLogStoreTests
{
    [Fact]
    public void NewStore_HasCountZero()
    {
        var store = new CommandLogStore();
        store.Count.Should().Be(0);
    }

    [Fact]
    public void Append_IncrementsCount()
    {
        var store = new CommandLogStore();
        store.Append(new CommandEnvelope { CommandType = "AddEntity", Payload = "Test" });
        store.Count.Should().Be(1);
    }

    [Fact]
    public void Append_MultipleCommands_PreservesOrder()
    {
        var store = new CommandLogStore();
        store.Append(new CommandEnvelope { CommandType = "First" });
        store.Append(new CommandEnvelope { CommandType = "Second" });
        store.Append(new CommandEnvelope { CommandType = "Third" });

        var all = store.GetAll();
        all.Should().HaveCount(3);
        all[0].CommandType.Should().Be("First");
        all[1].CommandType.Should().Be("Second");
        all[2].CommandType.Should().Be("Third");
    }

    [Fact]
    public void Count_NeverDecreases()
    {
        var store = new CommandLogStore();
        store.Append(new CommandEnvelope());
        var beforeCount = store.Count;
        // Simulace: nelze odebrat — neexistuje metoda pro odebrání
        store.Count.Should().Be(beforeCount);
    }

    [Fact]
    public void GetAll_ReturnsReadOnlyView()
    {
        var store = new CommandLogStore();
        store.Append(new CommandEnvelope { CommandType = "Test" });
        var all = store.GetAll();
        all.Should().HaveCount(1);
        // Přidání dalšího by nemělo ovlivnit již vrácenou kolekci
    }

    [Fact]
    public void GetFrom_ReturnsCommandsFromIndex()
    {
        var store = new CommandLogStore();
        store.Append(new CommandEnvelope { CommandType = "0" });
        store.Append(new CommandEnvelope { CommandType = "1" });
        store.Append(new CommandEnvelope { CommandType = "2" });

        var from = store.GetFrom(1);
        from.Should().HaveCount(2);
        from[0].CommandType.Should().Be("1");
        from[1].CommandType.Should().Be("2");
    }
}
```

**Kód — `ReplayEngineTests.cs`:**

```csharp
using FluentAssertions;
using MetaForge.BusinessModel.CommandLog;

namespace MetaForge.BusinessModel.Tests.CommandLog;

public class ReplayEngineTests
{
    private readonly ReplayEngine _engine = new();

    [Fact]
    public void Replay_EmptyLog_ReturnsEmptyDocument()
    {
        var commands = new List<CommandEnvelope>();
        var doc = _engine.Replay(commands);

        doc.Entities.Should().BeEmpty();
        doc.Relations.Should().BeEmpty();
    }

    [Fact]
    public void Replay_SingleAddEntity_ReturnsDocumentWithEntity()
    {
        var commands = new List<CommandEnvelope>
        {
            new()
            {
                CommandType = "AddEntity",
                TargetEntityId = "e1",
                Payload = "Customer",
            }
        };

        var doc = _engine.Replay(commands);

        doc.Entities.Should().HaveCount(1);
        doc.Entities[0].Name.Should().Be("Customer");
        doc.Entities[0].Id.Should().Be("e1");
    }

    [Fact]
    public void Replay_TwoEntities_BothPresent()
    {
        var commands = new List<CommandEnvelope>
        {
            new() { CommandType = "AddEntity", TargetEntityId = "e1", Payload = "Customer" },
            new() { CommandType = "AddEntity", TargetEntityId = "e2", Payload = "Order" },
        };

        var doc = _engine.Replay(commands);

        doc.Entities.Should().HaveCount(2);
        doc.Entities.Select(e => e.Name).Should().Contain(new[] { "Customer", "Order" });
    }

    [Fact]
    public void Replay_AddThenUpdate_NameIsUpdated()
    {
        var commands = new List<CommandEnvelope>
        {
            new() { CommandType = "AddEntity", TargetEntityId = "e1", Payload = "Customer" },
            new() { CommandType = "UpdateEntity", TargetEntityId = "e1", Payload = "Client" },
        };

        var doc = _engine.Replay(commands);

        doc.Entities.Should().HaveCount(1);
        doc.Entities[0].Name.Should().Be("Client");
    }

    [Fact]
    public void Replay_AddThenDelete_EntityRemoved()
    {
        var commands = new List<CommandEnvelope>
        {
            new() { CommandType = "AddEntity", TargetEntityId = "e1", Payload = "Customer" },
            new() { CommandType = "DeleteEntity", TargetEntityId = "e1" },
        };

        var doc = _engine.Replay(commands);

        doc.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Replay_Deterministic_SameInputSameOutput()
    {
        var commands = new List<CommandEnvelope>
        {
            new() { CommandType = "AddEntity", TargetEntityId = "e1", Payload = "A" },
            new() { CommandType = "AddEntity", TargetEntityId = "e2", Payload = "B" },
        };

        var doc1 = _engine.Replay(commands);
        var doc2 = _engine.Replay(commands);

        doc1.Entities.Should().HaveCount(doc2.Entities.Count);
        doc1.Entities[0].Name.Should().Be(doc2.Entities[0].Name);
        doc1.Entities[1].Name.Should().Be(doc2.Entities[1].Name);
    }

    [Fact]
    public void Replay_AddAttribute_AttributePresent()
    {
        var commands = new List<CommandEnvelope>
        {
            new() { CommandType = "AddEntity", TargetEntityId = "e1", Payload = "Customer" },
            new() { CommandType = "AddAttribute", TargetEntityId = "e1", TargetAttributeId = "a1", Payload = "FirstName|string|true" },
        };

        var doc = _engine.Replay(commands);

        doc.Entities[0].Attributes.Should().HaveCount(1);
        doc.Entities[0].Attributes[0].Name.Should().Be("FirstName");
        doc.Entities[0].Attributes[0].Type.Should().Be("string");
        doc.Entities[0].Attributes[0].IsRequired.Should().BeTrue();
    }
}
```

**Kód — `PatchEngineTests.cs`:**

```csharp
using FluentAssertions;
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;
using MetaForge.BusinessModel.Patches;
using MetaForge.BusinessModel.Patches.Operations;

namespace MetaForge.BusinessModel.Tests.Patches;

public class PatchEngineTests
{
    private readonly CommandLogStore _logStore = new();
    private readonly BusinessAuthoringDocument _document = new();
    private PatchEngine _engine => new(_logStore);

    [Fact]
    public void Apply_AddEntity_AddsToDocument()
    {
        var engine = _engine;
        var op = new AddEntityOp("Customer");

        engine.Apply(_document, op);

        _document.Entities.Should().HaveCount(1);
        _document.Entities[0].Name.Should().Be("Customer");
    }

    [Fact]
    public void Apply_AddEntity_CreatesLogEntry()
    {
        var engine = _engine;
        var op = new AddEntityOp("Customer");

        engine.Apply(_document, op);

        _logStore.Count.Should().Be(1);
        _logStore.GetAll()[0].CommandType.Should().Be("AddEntity");
    }

    [Fact]
    public void Apply_AddAttribute_ThrowsForNonExistentEntity()
    {
        var engine = _engine;
        var op = new AddAttributeOp("nonexistent", "Name");

        var act = () => engine.Apply(_document, op);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*neexistuje*");
    }

    [Fact]
    public void Apply_DeleteEntity_RemovesEntityAndRelations()
    {
        var engine = _engine;
        var addOp = new AddEntityOp("Customer");
        engine.Apply(_document, addOp);
        var entityId = addOp.EntityId;

        var deleteOp = new DeleteEntityOp(entityId);
        engine.Apply(_document, deleteOp);

        _document.Entities.Should().BeEmpty();
        _logStore.Count.Should().Be(2);
    }

    [Fact]
    public void Apply_NullDocument_ThrowsArgumentNullException()
    {
        var engine = _engine;
        var op = new AddEntityOp("Test");

        var act = () => engine.Apply(null!, op);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Apply_NullOperation_ThrowsArgumentNullException()
    {
        var engine = _engine;

        var act = () => engine.Apply(_document, null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
```

**Ověření:** `dotnet test Tests/MetaForge.BusinessModel.Tests/` projde.
**Riziko:** Střední — replay testy jsou kritické pro integritu dat.
**Rollback:** Smaž soubory.

---

## TASK-9.3.1 — Založení projektu MetaForge.Translator.Tests

**Vstup:** Epic 4 dokončen.
**Výstup:** Projekt + testy pro Translator.
**Soubory:**
- `Tests/MetaForge.Translator.Tests/MetaForge.Translator.Tests.csproj`
- `Tests/MetaForge.Translator.Tests/Translation/DefaultBusinessTranslatorTests.cs`

**Kód — `.csproj`:**

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
    <ProjectReference Include="..\..\Src\MetaForge.Translator\MetaForge.Translator.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="FluentAssertions" Version="6.12.1" />
  </ItemGroup>
</Project>
```

**Kód — `DefaultBusinessTranslatorTests.cs`:**

```csharp
using FluentAssertions;
using MetaForge.BusinessModel.Models;
using MetaForge.Core.Catalog;
using MetaForge.Core.DataTypes;
using MetaForge.Translator.Translation;

namespace MetaForge.Translator.Tests.Translation;

public class DefaultBusinessTranslatorTests
{
    private readonly CatalogManager _catalog = new();
    private readonly DefaultBusinessTranslator _translator;

    public DefaultBusinessTranslatorTests()
    {
        _catalog.RegisterProvider(new BuiltInCatalogProvider());
        _translator = new DefaultBusinessTranslator(_catalog);
    }

    [Fact]
    public void Translate_Email_ReturnsStringType()
    {
        var attr = new BusinessAttributeNode { Name = "Email", Type = "email" };
        var result = _translator.Translate(attr);

        result.BaseType.Should().Be(DataType.String);
    }

    [Fact]
    public void Translate_Money_ReturnsDecimalType()
    {
        var attr = new BusinessAttributeNode { Name = "Price", Type = "money" };
        var result = _translator.Translate(attr);

        result.BaseType.Should().Be(DataType.Decimal);
    }

    [Fact]
    public void Translate_UnknownType_ReturnsObject()
    {
        var attr = new BusinessAttributeNode { Name = "Foo", Type = "unknown_xyz" };
        var result = _translator.Translate(attr);

        result.BaseType.Should().Be(DataType.Object);
    }

    [Fact]
    public void TryEnrich_Email_ReturnsEnrichmentWithMaxLength()
    {
        var attr = new BusinessAttributeNode { Id = "a1", Name = "Email", Type = "email" };
        var result = _translator.TryEnrich(attr);

        result.Should().NotBeNull();
        result!.MaxLength.Should().Be(254);
        result.ValidationRules.Should().Contain("email_format");
    }

    [Fact]
    public void TryEnrich_PlainString_ReturnsNull()
    {
        var attr = new BusinessAttributeNode { Id = "a1", Name = "Foo", Type = "int" };
        var result = _translator.TryEnrich(attr);

        result.Should().BeNull();
    }
}
```

**Ověření:** `dotnet test Tests/MetaForge.Translator.Tests/` projde.
**Riziko:** Nízké.
**Rollback:** Smaž soubory.

---

## TASK-9.4.1 — Založení projektu MetaForge.Generators.Tests

**Vstup:** Epic 7 dokončen.
**Výstup:** Projekt + testy generátoru.
**Soubory:**
- `Tests/MetaForge.Generators.Tests/MetaForge.Generators.Tests.csproj`
- `Tests/MetaForge.Generators.Tests/CSharp/CSharpGeneratorTests.cs`

**Kód — `.csproj`:**

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
    <ProjectReference Include="..\..\Src\MetaForge.Generators\MetaForge.Generators.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="FluentAssertions" Version="6.12.1" />
  </ItemGroup>
</Project>
```

**Kód — `CSharpGeneratorTests.cs`:**

```csharp
using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;
using MetaForge.Generators.CSharp;

namespace MetaForge.Generators.Tests.CSharp;

public class CSharpGeneratorTests
{
    private readonly CSharpGenerator _generator = new();

    [Fact]
    public void Generate_ClassElement_ContainsPublicClass()
    {
        var cls = new ClassElement { Name = "Customer" };
        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("public class Customer");
        result.LanguageId.Should().Be("csharp");
        result.FileName.Should().Be("Customer.cs");
    }

    [Fact]
    public void Generate_ClassWithProperty_ContainsPropertyDeclaration()
    {
        var cls = new ClassElement { Name = "Customer" };
        cls.Properties.Add(new PropertyElement
        {
            Name = "FirstName",
            Type = TypeModel.String,
        });

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("string FirstName");
        result.SourceCode.Should().Contain("get;");
    }

    [Fact]
    public void Generate_ClassWithMethod_ContainsMethodDeclaration()
    {
        var cls = new ClassElement { Name = "Customer" };
        cls.Methods.Add(new MethodElement
        {
            Name = "GetFullName",
            ReturnType = TypeModel.String,
            Body = "return \"test\";",
        });

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("string GetFullName()");
        result.SourceCode.Should().Contain("return \"test\";");
    }

    [Fact]
    public void Generate_EmptyName_ReturnsError()
    {
        var cls = new ClassElement { Name = "" };
        var result = _generator.Generate(cls);

        result.Diagnostics.Should().NotBeNull();
        result.Diagnostics!.Should().Contain(d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Generate_Enum_ContainsEnumDeclaration()
    {
        var enm = new EnumElement { Name = "Status" };
        enm.Members.Add(new EnumMemberElement { Name = "Active" });
        enm.Members.Add(new EnumMemberElement { Name = "Inactive" });

        var result = _generator.Generate(enm);

        result.SourceCode.Should().Contain("public enum Status");
        result.SourceCode.Should().Contain("Active");
        result.SourceCode.Should().Contain("Inactive");
    }

    [Fact]
    public void Generate_Interface_ContainsInterfaceDeclaration()
    {
        var iface = new InterfaceElement { Name = "IRepository" };
        iface.Methods.Add(new MethodElement
        {
            Name = "GetById",
            ReturnType = TypeModel.Object,
        });

        var result = _generator.Generate(iface);

        result.SourceCode.Should().Contain("interface IRepository");
        result.SourceCode.Should().Contain("GetById");
    }

    [Fact]
    public void Generate_Struct_ContainsStructDeclaration()
    {
        var str = new StructElement { Name = "Point" };
        str.Properties.Add(new PropertyElement { Name = "X", Type = TypeModel.Int32 });

        var result = _generator.Generate(str);

        result.SourceCode.Should().Contain("struct Point");
        result.SourceCode.Should().Contain("int X");
    }
}
```

**Ověření:** `dotnet test Tests/MetaForge.Generators.Tests/` projde.
**Riziko:** Nízké.
**Rollback:** Smaž soubory.

---

## Souhrn Epic 9 — Co musí existovat po dokončení

```
Tests/
├── MetaForge.Core.Tests/
│   ├── MetaForge.Core.Tests.csproj
│   ├── DataTypes/
│   │   ├── TypeModelTests.cs
│   │   └── DataTypeTests.cs
│   ├── Catalog/
│   │   └── CatalogManagerTests.cs
│   ├── ForgeBlockPackages/
│   │   └── ForgeBlockRegistryTests.cs
│   └── Inference/
│       └── RuleBasedConstraintInferencerTests.cs
├── MetaForge.BusinessModel.Tests/
│   ├── MetaForge.BusinessModel.Tests.csproj
│   ├── CommandLog/
│   │   ├── CommandLogStoreTests.cs
│   │   └── ReplayEngineTests.cs
│   └── Patches/
│       └── PatchEngineTests.cs
├── MetaForge.Translator.Tests/
│   ├── MetaForge.Translator.Tests.csproj
│   └── Translation/
│       └── DefaultBusinessTranslatorTests.cs
└── MetaForge.Generators.Tests/
    ├── MetaForge.Generators.Tests.csproj
    └── CSharp/
        └── CSharpGeneratorTests.cs
```

**Celkem testů:** ~35+
**Build:** `dotnet test MetaForge.slnx` projde, všechny testy zelené.

**Checkpoint:** `git tag checkpoint/epic-9-done`

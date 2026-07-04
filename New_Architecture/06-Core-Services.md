# Core — Services

> CatalogManager, ForgeBlockRegistry, Discovery, Vogen/StrongType

---

## CatalogManager

```csharp
public class CatalogManager
{
    public void RegisterPreset(PresetDefinition preset) { }
    public PresetDefinition? ResolveType(string typeName) { }
    public IReadOnlyList<PresetDefinition> SearchPresets(string query) { }
    public IReadOnlyList<PresetDefinition> GetAllPresets() { }
}
```

## IForgeBlockPackage

```csharp
public interface IForgeBlockPackage
{
    string Handle { get; }
    string Version { get; }
    IReadOnlyList<ForgeBlockCapability> Capabilities { get; }
    DiscoveryMetadata Discovery { get; }
    void Register(ForgeBlockRegistry registry);
}
```

## IForgeBlockCapabilityPackage

```csharp
public interface IForgeBlockCapabilityPackage : IForgeBlockPackage
{
    ForgeBlockPackageDescriptor Descriptor { get; }
    IReadOnlyList<ForgeBlockCatalogEntryDescriptor> CatalogEntries { get; }
}
```

## StrongType

```csharp
public record StrongType(
    string Name,
    TypeModel Underlying,
    IReadOnlyList<ValueObjectValidationRule>? ValidationRules = null,
    ConversionOptions? Conversion = null
);
```

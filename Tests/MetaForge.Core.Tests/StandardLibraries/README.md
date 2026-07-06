# StandardLibraries — Test Coverage

| Test Method | Popis |
|-------------|-------|
| `StandardLibraryTranslatorRegistryTests.Register_AddsTranslator` | Translator se zaregistruje |
| `StandardLibraryTranslatorRegistryTests.Register_OverwriteSameKey_UpdatesTranslator` | Přepsání stejného klíče aktualizuje translator |
| `StandardLibraryTranslatorRegistryTests.Resolve_Existing_ReturnsTranslator` | Existující operationId vrátí translator |
| `StandardLibraryTranslatorRegistryTests.Resolve_Unknown_ReturnsNull` | Neznámé operationId → null |
| `StandardLibraryTranslatorRegistryTests.GetAll_ReturnsAllRegistered` | Vrátí všechny registrované |
| `StandardLibraryTranslatorRegistryTests.GetAll_Empty_ReturnsEmpty` | Prázdný seznam pokud nic není registrováno |
| `StandardLibraryRequirementResolverTests.Resolve_ExistingOperation_ReturnsRequirements` | Existující operace vrátí požadavky |
| `StandardLibraryRequirementResolverTests.Resolve_UnknownOperation_ReturnsNull` | Neznámá operace → null |
| `StandardLibraryRequirementResolverTests.GetRequiredNamespaces_EmptyInput_ReturnsEmpty` | Prázdný vstup → prázdný seznam |
| `StandardLibraryRequirementResolverTests.GetRequiredNamespaces_MultipleOperations_AggregatesUnique` | Více operací → unikátní namespaces |
| `StandardLibraryRequirementResolverTests.GetRequiredNamespaces_DuplicateNamespaces_Deduplicates` | Duplicitní namespaces se deduplikují |
| `StandardLibraryRequirementResolverTests.GetRequiredNamespaces_OperationWithNullNamespaces_Skipped` | Když RequiredNamespaces je null, přeskočí se |

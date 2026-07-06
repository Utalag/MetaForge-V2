# Catalog — Test Coverage

## BuiltInCatalogProviderTests (nové)

| Test Method | Popis |
|-------------|-------|
| `GetAllPresets_ReturnsAll20Presets` | Vrátí všech 20 předdefinovaných presetů |
| `ProviderName_Always_ReturnsBuiltIn` | ProviderName je "BuiltIn" |
| `ResolveType_String_ReturnsString` | "string" → TypeModel.String |
| `ResolveType_Int_ReturnsInt32` | "int" → TypeModel.Int32 |
| `ResolveType_Decimal_ReturnsDecimal` | "decimal" → TypeModel.Decimal |
| `ResolveType_Bool_ReturnsBool` | "bool" → TypeModel.Bool |
| `ResolveType_Email_ReturnsString` | "email" → TypeModel.String |
| `ResolveType_Phone_ReturnsString` | "phone" → TypeModel.String |
| `ResolveType_Guid_ReturnsGuid` | "guid" → TypeModel.Guid |
| `ResolveType_DateTime_ReturnsDateTime` | "datetime" → TypeModel.DateTime |
| `ResolveType_Url_ReturnsUri` | "url" → DataType.Uri |
| `ResolveType_Money_ReturnsDecimal` | "money" → TypeModel.Decimal |
| `ResolveType_Price_ReturnsDecimal` | "price" → TypeModel.Decimal |
| `ResolveType_TextAlias_ReturnsString` | "text" → TypeModel.String (alias) |
| `ResolveType_BoolAlias_ReturnsBool` | "boolean" → TypeModel.Bool (alias) |
| `ResolveType_UuidAlias_ReturnsGuid` | "uuid" → TypeModel.Guid (alias) |
| `ResolveType_UriAlias_ReturnsUri` | "uri" → DataType.Uri (alias) |
| `ResolveType_Long_ReturnsInt64` | "long" → DataType.Int64 |
| `ResolveType_Double_ReturnsDouble` | "double" → DataType.Double |
| `ResolveType_Float_ReturnsSingle` | "float" → DataType.Single |
| `ResolveType_Date_ReturnsDateOnly` | "date" → DataType.DateOnly |
| `ResolveType_Time_ReturnsTimeOnly` | "time" → DataType.TimeOnly |
| `ResolveType_Unknown_ReturnsNull` | Neznámý název → null |
| `ResolveType_Email_HasContactValidationTags` | Email má tagy "contact" a "validation" |
| `ResolveType_Phone_HasContactTag` | Phone má tag "contact" |
| `ResolveType_Money_HasFinanceTag` | Money má tag "finance" |
| `ResolveType_Price_HasFinanceTag` | Price má tag "finance" |

## CatalogManagerTests (rozšíření)

| Test Method | Popis |
|-------------|-------|
| `RegisterStrongType_AddsToCatalog` | StrongType se zaregistruje |
| `ResolveStrongType_Existing_ReturnsType` | Existující StrongType se vrátí |
| `ResolveStrongType_Unknown_ReturnsNull` | Neznámý StrongType → null |
| `GetAllStrongTypes_ReturnsAllRegistered` | Vrátí všechny registrované StrongType |
| `GetAllStrongTypes_Empty_ReturnsEmpty` | Prázdný seznam pokud nic není registrováno |
| `SearchPresets_ByTag_FindsMatching` | Hledání podle tagu najde preset |
| `SearchPresets_CaseInsensitiveTag` | Hledání tagu je case-insensitive |
| `GetAllPresets_CustomOverridesProvider_DoesNotDuplicate` | Custom preset přepíše provider, DistinctBy zajistí unikátnost |

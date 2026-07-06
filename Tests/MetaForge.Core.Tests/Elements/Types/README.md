# Elements — Types Test Coverage

| Test Method | Popis |
|-------------|-------|
| `ClassElementTests.Kind_Always_ReturnsClass` | Kind vrací "class" |
| `ClassElementTests.TotalCoin_NoPropertiesNoMethods_ReturnsCoin` | TotalCoin = Coin při prázdných Properties/Methods |
| `ClassElementTests.TotalCoin_WithProperties_IncludesSum` | TotalCoin zahrnuje Coin všech Properties |
| `ClassElementTests.TotalCoin_WithMethods_IncludesTotalCoinOfMethods` | TotalCoin zahrnuje TotalCoin všech Methods |
| `ClassElementTests.TotalCoin_WithPropertiesAndMethods_AggregatesBoth` | TotalCoin = Coin + sum(Properties.Coin) + sum(Methods.TotalCoin) |
| `ClassElementTests.Flags_Defaults_AreFalse` | IsAbstract, IsSealed, IsStatic, IsPartial, IsRecord jsou false |
| `ClassElementTests.AccessModifier_Default_IsPublic` | Výchozí AccessModifier je Public |
| `ClassElementTests.BaseClassName_Default_IsNull` | BaseClassName je null |
| `ClassElementTests.ImplementedInterfaces_Default_IsEmpty` | ImplementedInterfaces je prázdný |
| `InterfaceElementTests.Kind_Always_ReturnsInterface` | Kind vrací "interface" |
| `InterfaceElementTests.TotalCoin_WithPropertiesAndMethods_Aggregates` | TotalCoin agreguje Properties a Methods |
| `InterfaceElementTests.AccessModifier_Default_IsPublic` | Výchozí AccessModifier je Public |
| `EnumElementTests.Kind_Always_ReturnsEnum` | Kind vrací "enum" |
| `EnumElementTests.UnderlyingType_Default_IsInt32` | UnderlyingType je Int32 |
| `EnumElementTests.IsFlags_Default_IsFalse` | IsFlags je false |
| `EnumElementTests.TotalCoin_NoMembers_ReturnsCoin` | TotalCoin = Coin při prázdných Members |
| `EnumElementTests.TotalCoin_WithMembers_IncludesSum` | TotalCoin zahrnuje Coin všech Members |
| `EnumElementTests.AccessModifier_Default_IsPublic` | Výchozí AccessModifier je Public |
| `EnumMemberElementTests.Name_Default_IsEmptyString` | Výchozí Name je prázdný string |
| `EnumMemberElementTests.Value_Default_IsNull` | Value je null |
| `EnumMemberElementTests.Coin_Default_IsOne` | Výchozí Coin je 1 |
| `EnumMemberElementTests.Attributes_Default_IsEmpty` | Attributes je prázdný seznam |
| `StructElementTests.Kind_Always_ReturnsStruct` | Kind vrací "struct" |
| `StructElementTests.TotalCoin_WithPropertiesAndMethods_Aggregates` | TotalCoin agreguje Properties a Methods |
| `StructElementTests.IsReadOnly_Default_IsFalse` | IsReadOnly je false |
| `StructElementTests.IsRecord_Default_IsFalse` | IsRecord je false |

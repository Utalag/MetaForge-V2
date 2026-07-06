# Abstractions — Test Coverage

| Test Method | Popis |
|-------------|-------|
| `AppRootTests.AppRoot_Default_HasEmptyProjects` | Ověří, že nový AppRoot má prázdný seznam projektů |
| `AppRootTests.TotalCoin_EmptyProjects_ReturnsZero` | TotalCoin = 0 při prázdném seznamu projektů |
| `AppRootTests.TotalCoin_SingleProjectNoRootElements_ReturnsZero` | Projekt bez RootElementů → TotalCoin = 0 |
| `AppRootTests.TotalCoin_SingleProjectWithClass_ReturnsClassTotalCoin` | Jeden projekt + jedna třída → TotalCoin = Coin třídy |
| `AppRootTests.TotalCoin_MultipleProjects_AggregatesAll` | Více projektů → TotalCoin je součet napříč projekty |
| `AppRootTests.TotalCoin_ProjectWithMultipleRootElements_AggregatesAll` | Projekt s více RootElementy → TotalCoin je součet všech |
| `RootElementTests.Id_Default_IsNotEmptyGuid` | Id je vygenerovaný Guid (není prázdný) |
| `RootElementTests.Kind_ClassElement_ReturnsClass` | ClassElement.Kind vrací "class" |
| `RootElementTests.Kind_EnumElement_ReturnsEnum` | EnumElement.Kind vrací "enum" |
| `RootElementTests.Kind_InterfaceElement_ReturnsInterface` | InterfaceElement.Kind vrací "interface" |
| `RootElementTests.Kind_StructElement_ReturnsStruct` | StructElement.Kind vrací "struct" |
| `RootElementTests.TotalCoin_Default_ReturnsCoin` | Bázový TotalCoin = Coin (neupravený potomkem) |
| `RootElementTests.Usings_Default_IsEmpty` | Usings je prázdný seznam |
| `RootElementTests.Attributes_Default_IsEmpty` | Attributes je prázdný seznam |
| `ProjectElementTests.Name_Default_IsEmptyString` | Výchozí Name je prázdný string |
| `ProjectElementTests.DefaultNamespace_Default_IsNull` | Výchozí DefaultNamespace je null |
| `ProjectElementTests.RootElements_Default_IsEmpty` | RootElements je prázdný seznam |
| `AttributeElementTests.Name_Default_IsEmptyString` | Výchozí Name je prázdný string |
| `AttributeElementTests.Arguments_Default_IsEmpty` | Arguments je prázdný seznam |
| `AttributeElementTests.Arguments_CanContainNull` | Arguments může obsahovat null hodnoty |
| `SemanticCollectionTests.Add_RaisesChangedEvent` | Add vyvolá Changed událost |
| `SemanticCollectionTests.Remove_RaisesChangedEvent` | Remove vyvolá Changed událost |
| `SemanticCollectionTests.Clear_RaisesChangedEvent` | Clear vyvolá Changed událost |
| `SemanticCollectionTests.ChangedEvent_NotRaisedBeforeMutation` | Changed není vyvoláno bez mutace |
| `SemanticCollectionTests.MultipleAdds_RaisesChangedEachTime` | Každý Add vyvolá Changed zvlášť |
| `AccessModifierTests.Enum_HasAllExpectedMembers` | Enum obsahuje Public, Internal, Protected, Private, ProtectedInternal, PrivateProtected |

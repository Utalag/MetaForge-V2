# Core Examples — C# → Core → Popis

> Reálné příklady mapování C# kódu na Core reprezentaci.

## Příklad 1: Jednoduchá třída

### C#
```csharp
public class Customer
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```

### Core reprezentace
```
ClassElement
├── Kind: "class"
├── Name: "Customer"
├── AccessModifier: Public
├── Properties:
│   ├── PropertyElement { Name: "Name", Type: string, HasGetter: true, HasSetter: true }
│   └── PropertyElement { Name: "Age", Type: int, HasGetter: true, HasSetter: true }
```

### Core kód
```csharp
var customer = new ClassElement
{
    Name = "Customer",
    Properties =
    {
        new PropertyElement { Name = "Name", Type = TypeModel.String },
        new PropertyElement { Name = "Age", Type = TypeModel.Int32 }
    }
};
```

---

## Příklad 2: Abstraktní bázová třída s generiky

### C#
```csharp
/// <summary>Base repository for all entities.</summary>
public abstract class Repository<T> where T : class
{
    public abstract Task<T?> GetByIdAsync(Guid id);
    public abstract Task SaveAsync(T entity);
}
```

### Core reprezentace
```
ClassElement
├── Kind: "class"
├── Name: "Repository"
├── XmlSummary: "Base repository for all entities."
├── IsAbstract: true
├── TypeParameters: ["T"]
├── TypeConstraints: [GenericConstraint { TypeParameterName: "T", Constraints: [Class] }]
├── Methods:
│   ├── MethodElement { Name: "GetByIdAsync", IsAbstract: true, IsAsync: true, ReturnType: Task<T?>, Parameters: [id: Guid] }
│   └── MethodElement { Name: "SaveAsync", IsAbstract: true, IsAsync: true, ReturnType: Task, Parameters: [entity: T] }
```

### Core kód
```csharp
var repo = new ClassElement
{
    Name = "Repository",
    XmlSummary = "Base repository for all entities.",
    IsAbstract = true,
    TypeParameters = { "T" },
    TypeConstraints = { GenericConstraint.Class("T") },
    Methods =
    {
        new MethodElement
        {
            Name = "GetByIdAsync",
            IsAbstract = true,
            IsAsync = true,
            ReturnType = TypeModel.Of(DataType.Entity)
                .WithCustomName("Task")
                .WithGenericArg(TypeModel.Of(DataType.Entity).WithCustomName("T").MakeNullable()),
            Parameters = { new ParameterElement { Name = "id", Type = TypeModel.Guid } }
        },
        new MethodElement
        {
            Name = "SaveAsync",
            IsAbstract = true,
            IsAsync = true,
            ReturnType = TypeModel.Of(DataType.Entity).WithCustomName("Task"),
            Parameters = { new ParameterElement { Name = "entity", Type = TypeModel.Of(DataType.Entity).WithCustomName("T") } }
        }
    }
};
```

---

## Příklad 3: Enum s Flags

### C#
```csharp
[Flags]
public enum Permissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Execute = 4
}
```

### Core reprezentace
```
EnumElement
├── Kind: "enum"
├── Name: "Permissions"
├── IsFlags: true
├── UnderlyingType: Int32
├── Members:
│   ├── EnumMemberElement { Name: "None", Value: 0 }
│   ├── EnumMemberElement { Name: "Read", Value: 1 }
│   ├── EnumMemberElement { Name: "Write", Value: 2 }
│   └── EnumMemberElement { Name: "Execute", Value: 4 }
```

### Core kód
```csharp
var permissions = EnumElement.Flags("Permissions");
permissions.Members.AddRange(new[]
{
    new EnumMemberElement { Name = "None", Value = 0 },
    new EnumMemberElement { Name = "Read", Value = 1 },
    new EnumMemberElement { Name = "Write", Value = 2 },
    new EnumMemberElement { Name = "Execute", Value = 4 }
});
```

---

## Příklad 4: Record s primary constructor

### C#
```csharp
public sealed record Address(string Street, string City, string ZipCode);
```

### Core reprezentace
```
ClassElement
├── Kind: "class"
├── Name: "Address"
├── IsSealed: true
├── IsRecord: true
├── PrimaryConstructorParameters:
│   ├── ParameterElement { Name: "Street", Type: string }
│   ├── ParameterElement { Name: "City", Type: string }
│   └── ParameterElement { Name: "ZipCode", Type: string }
```

### Core kód
```csharp
var address = ClassElement.PrimaryRecord("Address",
    new ParameterElement { Name = "Street", Type = TypeModel.String },
    new ParameterElement { Name = "City", Type = TypeModel.String },
    new ParameterElement { Name = "ZipCode", Type = TypeModel.String }
);
address.IsSealed = true;
```

---

## Příklad 5: ForEach statement s AST

### C#
```csharp
foreach (var item in items)
{
    Console.WriteLine(item);
}
```

### Core AST
```csharp
var forEach = new ForEachStatement
{
    VariableName = "item",
    Collection = new MemberAccessExpression { MemberPath = "items" },
    Body = new BlockStatement
    {
        Statements =
        {
            new ExpressionStatement
            {
                Expr = new MethodCallExpression
                {
                    MethodName = "Console.WriteLine",
                    Arguments = { new MemberAccessExpression { MemberPath = "item" } }
                }
            }
        }
    }
};
```

---

## Příklad 6: Switch expression

### C#
```csharp
var result = status switch
{
    "active" => "OK",
    "inactive" => "Disabled",
    _ => "Unknown"
};
```

### Core AST
```csharp
var switchExpr = new SwitchExpression
{
    Selector = new MemberAccessExpression { MemberPath = "status" },
    Arms =
    {
        new SwitchArm
        {
            Pattern = new ConstantExpression { Value = "active" },
            Value = new ConstantExpression { Value = "OK" }
        },
        new SwitchArm
        {
            Pattern = new ConstantExpression { Value = "inactive" },
            Value = new ConstantExpression { Value = "Disabled" }
        },
        new SwitchArm
        {
            Pattern = new ConstantExpression { Value = "_" },
            Value = new ConstantExpression { Value = "Unknown" }
        }
    }
};
```

---

## Příklad 7: StrongType (Value Object)

### C# (koncept)
```csharp
// Uživatel definuje: "Chci typ Email, který je string a musí obsahovat @"
```

### Core reprezentace
```csharp
var emailType = new StrongType(
    Name: "Email",
    Underlying: TypeModel.String,
    ValidationRules: new[]
    {
        new ValueObjectValidationRule("NotEmpty", null, "Email cannot be empty"),
        new ValueObjectValidationRule("Regex", @"^[^@\s]+@[^@\s]+\.[^@\s]+$", "Invalid email format")
    },
    Conversion: new ConversionOptions(
        GenerateImplicitConversion: true,
        GenerateExplicitConversion: true,
        GenerateToString: true,
        GenerateEquals: true,
        GenerateGetHashCode: true
    )
);
```

---

## Příklad 8: AppRoot — celý projekt

### Koncept
```
AppRoot
└── ProjectElement { Name: "MyApp", DefaultNamespace: "MyApp" }
    ├── ClassElement { Name: "Customer" }
    │   ├── Property: Name (string)
    │   └── Property: Age (int)
    ├── EnumElement { Name: "OrderStatus" }
    │   └── Members: Pending, Shipped, Delivered
    └── InterfaceElement { Name: "IRepository<T>" }
        └── Method: GetById(id: Guid) → T
```

### Core kód
```csharp
var app = new AppRoot();
var project = new ProjectElement
{
    Name = "MyApp",
    DefaultNamespace = "MyApp"
};

project.RootElements.Add(new ClassElement
{
    Name = "Customer",
    Properties =
    {
        new PropertyElement { Name = "Name", Type = TypeModel.String },
        new PropertyElement { Name = "Age", Type = TypeModel.Int32 }
    }
});

project.RootElements.Add(new EnumElement
{
    Name = "OrderStatus",
    Members =
    {
        new EnumMemberElement { Name = "Pending" },
        new EnumMemberElement { Name = "Shipped" },
        new EnumMemberElement { Name = "Delivered" }
    }
});

app.Projects.Add(project);
```

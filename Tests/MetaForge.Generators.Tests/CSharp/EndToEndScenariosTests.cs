using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Statements;
using MetaForge.Core.Elements.Types;
using MetaForge.Core.ValueObjects;
using MetaForge.Generators;

namespace MetaForge.Generators.Tests;

/// <summary>
/// End-to-end testy: Core → Generator → validace C#.
/// Stress-test platformy odhalující mezery v generátoru.
/// 13 scénářů pokrývajících všechny renderery (PROP-043 + PROP-045).
/// </summary>
public class EndToEndScenariosTests
{
    private readonly CodeGenerator _generator = new();

    /// <summary>
    /// Scénář 1: Kvadratická rovnice — statická metoda, if/else, parametry, return hodnoty
    /// </summary>
    [Fact]
    public void Scenario1_QuadraticEquation_GeneratesCompilableCode()
    {
        var cls = new ClassElement
        {
            Name = "QuadraticSolver",
            IsStatic = true
        };

        // static (double?, double?) Solve(double a, double b, double c)
        var solveMethod = new MethodElement
        {
            Name = "Solve",
            IsStatic = true,
            ReturnType = TypeModel.Of(DataType.Entity)
                .WithCustomName("ValueTuple")
                .WithGenericArg(TypeModel.Of(DataType.Double).MakeNullable())
                .WithGenericArg(TypeModel.Of(DataType.Double).MakeNullable()),
            Body = new BlockStatement(
                new AssignmentStatement
                {
                    Variable = "var discriminant",
                    Value = new BinaryExpression(
                        new BinaryExpression(
                            new MemberAccessExpression("b"),
                            BinaryOperator.Multiply,
                            new MemberAccessExpression("b")
                        ),
                        BinaryOperator.Subtract,
                        new BinaryExpression(
                            new BinaryExpression(
                                new ConstantExpression(4),
                                BinaryOperator.Multiply,
                                new MemberAccessExpression("a")
                            ),
                            BinaryOperator.Multiply,
                            new MemberAccessExpression("c")
                        )
                    )
                },
                new IfStatement
                {
                    Condition = new BinaryExpression(
                        new MemberAccessExpression("discriminant"),
                        BinaryOperator.LessThan,
                        new ConstantExpression(0)
                    ),
                    TrueBranch = new ReturnStatement
                    {
                        Value = new ConstantExpression(null)
                    }
                },
                new ReturnStatement
                {
                    Value = new BinaryExpression(
                        new UnaryExpression(
                            UnaryOperator.Negate,
                            new MemberAccessExpression("b")
                        ),
                        BinaryOperator.Add,
                        new MethodCallExpression(
                            "Math.Sqrt",
                            new Expression[] { new MemberAccessExpression("discriminant") }
                        )
                    )
                }
            )
        };
        solveMethod.Parameters.Add(new ParameterElement { Name = "a", Type = TypeModel.Of(DataType.Double) });
        solveMethod.Parameters.Add(new ParameterElement { Name = "b", Type = TypeModel.Of(DataType.Double) });
        solveMethod.Parameters.Add(new ParameterElement { Name = "c", Type = TypeModel.Of(DataType.Double) });

        cls.Methods.Add(solveMethod);

        var result = _generator.Generate(cls);
        result.SourceCode.Should().NotBeNullOrEmpty();
        AssertGeneratedCodeIsValid(result);
        AssertGeneratedCodeMatches(result,
            """
            public static class QuadraticSolver
            {
                public static ValueTuple<double?, double?> Solve(double a, double b, double c)
                {
                    var discriminant = ((b * b) - ((4 * a) * c));
                    if ((discriminant < 0))
                        return null;
                    return (-b + Math.Sqrt(discriminant));
                }
            }
            """);
    }

    /// <summary>
    /// Scénář 2: Autoservis — entity: Customer, Car, RepairOrder s properties a konstruktorem
    /// </summary>
    [Fact]
    public void Scenario2_AutoRepairShop_GeneratesCompilableCode()
    {
        // === Customer entity ===
        var customer = new ClassElement
        {
            Name = "Customer",
            Properties =
            {
                PropertyElement.GetSet("Id", TypeModel.Int32),
                PropertyElement.GetSet("Name", TypeModel.String),
                PropertyElement.GetSet("Phone", TypeModel.String),
                PropertyElement.GetSet("Email", TypeModel.String),
            },
            Constructors =
            {
                ConstructorElement.Basic("Customer",
                    new ParameterElement { Name = "name", Type = TypeModel.String },
                    new ParameterElement { Name = "phone", Type = TypeModel.String })
                    .WithBody(new BlockStatement(
                        new AssignmentStatement
                        {
                            Variable = "Name",
                            Value = new MemberAccessExpression("name")
                        },
                        new AssignmentStatement
                        {
                            Variable = "Phone",
                            Value = new MemberAccessExpression("phone")
                        }
                    ))
            }
        };

        // === Car entity ===
        var car = new ClassElement
        {
            Name = "Car",
            Properties =
            {
                PropertyElement.GetSet("Id", TypeModel.Int32),
                PropertyElement.GetSet("LicensePlate", TypeModel.String),
                PropertyElement.GetSet("Brand", TypeModel.String),
                PropertyElement.GetSet("Model", TypeModel.String),
                PropertyElement.GetSet("Year", TypeModel.Int32),
                PropertyElement.GetSet("OwnerId", TypeModel.Int32),
            }
        };

        // === RepairOrder ===
        var repairOrder = new ClassElement
        {
            Name = "RepairOrder",
            Properties =
            {
                PropertyElement.GetSet("Id", TypeModel.Int32),
                PropertyElement.GetSet("CarId", TypeModel.Int32),
                PropertyElement.GetSet("Description", TypeModel.String),
                PropertyElement.GetSet("Cost", TypeModel.Of(DataType.Decimal)),
                PropertyElement.GetSet("CreatedAt", TypeModel.Of(DataType.DateTime)),
                PropertyElement.GetSet("IsCompleted", TypeModel.Bool),
            },
            Methods =
            {
                new MethodElement
                {
                    Name = "Complete",
                    IsStatic = false,
                    ReturnType = TypeModel.Void,
                    Body = new BlockStatement(
                        new AssignmentStatement
                        {
                            Variable = "IsCompleted",
                            Value = new ConstantExpression(true)
                        }
                    )
                }
            }
        };

        // === RepairStatus enum ===
        var status = new EnumElement { Name = "RepairStatus" };
        status.Members.Add(new EnumMemberElement { Name = "Pending" });
        status.Members.Add(new EnumMemberElement { Name = "InProgress" });
        status.Members.Add(new EnumMemberElement { Name = "Completed" });
        status.Members.Add(new EnumMemberElement { Name = "Cancelled" });

        var expectedCustomer =
            """
            public class Customer
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public string Phone { get; set; }
                public string Email { get; set; }
                public Customer(string name, string phone)
                {
                    Name = name;
                    Phone = phone;
                }
            }
            """;
        var expectedCar =
            """
            public class Car
            {
                public int Id { get; set; }
                public string LicensePlate { get; set; }
                public string Brand { get; set; }
                public string Model { get; set; }
                public int Year { get; set; }
                public int OwnerId { get; set; }
            }
            """;
        var expectedRepairOrder =
            """
            public class RepairOrder
            {
                public int Id { get; set; }
                public int CarId { get; set; }
                public string Description { get; set; }
                public decimal Cost { get; set; }
                public DateTime CreatedAt { get; set; }
                public bool IsCompleted { get; set; }
                public void Complete()
                {
                    IsCompleted = true;
                }
            }
            """;
        var expectedStatus =
            """
            public enum RepairStatus
            {
                Pending,
                InProgress,
                Completed,
                Cancelled
            }
            """;

        var elements = new (RootElement Element, string Expected)[]
        {
            (customer, expectedCustomer),
            (car, expectedCar),
            (repairOrder, expectedRepairOrder),
            (status, expectedStatus)
        };

        foreach (var (element, expected) in elements)
        {
            var result = _generator.Generate(element);
            AssertGeneratedCodeIsValid(result);
            AssertGeneratedCodeMatches(result, expected);
        }
    }

    /// <summary>
    /// Scénář 3: Objednávkový systém — Order, OrderItem, decimal, DateTime, enum
    /// </summary>
    [Fact]
    public void Scenario3_OrderSystem_GeneratesCompilableCode()
    {
        // === OrderStatus enum ===
        var orderStatus = new EnumElement { Name = "OrderStatus" };
        orderStatus.Members.Add(new EnumMemberElement { Name = "Draft", Value = 0 });
        orderStatus.Members.Add(new EnumMemberElement { Name = "Confirmed", Value = 1 });
        orderStatus.Members.Add(new EnumMemberElement { Name = "Shipped", Value = 2 });
        orderStatus.Members.Add(new EnumMemberElement { Name = "Delivered", Value = 3 });

        // === OrderItem ===
        var orderItem = new ClassElement
        {
            Name = "OrderItem",
            Properties =
            {
                PropertyElement.GetSet("ProductName", TypeModel.String),
                PropertyElement.GetSet("Quantity", TypeModel.Int32),
                PropertyElement.GetSet("UnitPrice", TypeModel.Of(DataType.Decimal)),
            },
            Methods =
            {
                new MethodElement
                {
                    Name = "GetTotal",
                    ReturnType = TypeModel.Of(DataType.Decimal),
                    ExpressionBody = new BinaryExpression(
                        new MemberAccessExpression("Quantity"),
                        BinaryOperator.Multiply,
                        new MemberAccessExpression("UnitPrice")
                    )
                }
            }
        };

        // === Order ===
        var order = new ClassElement
        {
            Name = "Order",
            Properties =
            {
                PropertyElement.GetSet("Id", TypeModel.Int32),
                PropertyElement.GetSet("CustomerName", TypeModel.String),
                PropertyElement.GetSet("OrderDate", TypeModel.Of(DataType.DateTime)),
                PropertyElement.GetSet("Status", new TypeModel
                {
                    CustomTypeName = "OrderStatus",
                    BaseType = DataType.Entity
                }),
                PropertyElement.GetSet("TotalPrice", TypeModel.Of(DataType.Decimal)),
            },
            Methods =
            {
                new MethodElement
                {
                    Name = "CalculateTotal",
                    ReturnType = TypeModel.Of(DataType.Decimal),
                    Body = new BlockStatement(
                        new ReturnStatement
                        {
                            Value = new ConstantExpression(0) // placeholder
                        }
                    )
                }
            }
        };

        var expectedOrderStatus =
            """
            public enum OrderStatus
            {
                Draft = 0,
                Confirmed = 1,
                Shipped = 2,
                Delivered = 3
            }
            """;
        var expectedOrderItem =
            """
            public class OrderItem
            {
                public string ProductName { get; set; }
                public int Quantity { get; set; }
                public decimal UnitPrice { get; set; }
                public decimal GetTotal() => (Quantity * UnitPrice);
            }
            """;
        var expectedOrder =
            """
            public class Order
            {
                public int Id { get; set; }
                public string CustomerName { get; set; }
                public DateTime OrderDate { get; set; }
                public OrderStatus Status { get; set; }
                public decimal TotalPrice { get; set; }
                public decimal CalculateTotal()
                {
                    return 0;
                }
            }
            """;

        var elements = new (RootElement Element, string Expected)[]
        {
            (orderStatus, expectedOrderStatus),
            (orderItem, expectedOrderItem),
            (order, expectedOrder)
        };

        foreach (var (element, expected) in elements)
        {
            var result = _generator.Generate(element);
            AssertGeneratedCodeIsValid(result);
            AssertGeneratedCodeMatches(result, expected);
        }
    }

    /// <summary>
    /// Scénář 4: User management — IUserRepository, UserService s DI, User entity
    /// </summary>
    [Fact]
    public void Scenario4_UserManagement_GeneratesCompilableCode()
    {
        // === User entity ===
        var user = new ClassElement
        {
            Name = "User",
            Properties =
            {
                PropertyElement.GetSet("Id", TypeModel.Guid),
                PropertyElement.GetSet("Username", TypeModel.String),
                PropertyElement.GetSet("Email", TypeModel.String),
                PropertyElement.GetSet("CreatedAt", TypeModel.Of(DataType.DateTime)),
                PropertyElement.GetSet("IsActive", TypeModel.Bool),
            }
        };

        // === IUserRepository ===
        var userRepo = new InterfaceElement
        {
            Name = "IUserRepository",
            Methods =
            {
                new MethodElement
                {
                    Name = "GetByIdAsync",
                    IsAsync = true,
                    ReturnType = TypeModel.Of(DataType.Entity).WithCustomName("User").MakeNullable(),
                    Parameters =
                    {
                        new ParameterElement { Name = "id", Type = TypeModel.Guid }
                    }
                },
                new MethodElement
                {
                    Name = "AddAsync",
                    IsAsync = true,
                    ReturnType = TypeModel.Void,
                    Parameters =
                    {
                        new ParameterElement { Name = "user", Type = TypeModel.Of(DataType.Entity).WithCustomName("User") }
                    }
                }
            }
        };

        // === UserService ===
        var userService = new ClassElement
        {
            Name = "UserService",
            Fields =
            {
                FieldElement.ReadOnly("_repository",
                    TypeModel.Of(DataType.Entity).WithCustomName("IUserRepository"))
            },
            Constructors =
            {
                ConstructorElement.Basic("UserService",
                    new ParameterElement
                    {
                        Name = "repository",
                        Type = TypeModel.Of(DataType.Entity).WithCustomName("IUserRepository")
                    })
                    .WithBody(new BlockStatement(
                        new AssignmentStatement
                        {
                            Variable = "_repository",
                            Value = new MemberAccessExpression("repository")
                        }
                    ))
            },
            Methods =
            {
                new MethodElement
                {
                    Name = "GetUserByIdAsync",
                    IsAsync = true,
                    ReturnType = TypeModel.Of(DataType.Entity).WithCustomName("User").MakeNullable(),
                    Parameters =
                    {
                        new ParameterElement { Name = "id", Type = TypeModel.Guid }
                    },
                    Body = new BlockStatement(
                        new ReturnStatement
                        {
                            Value = new MethodCallExpression(
                                "_repository.GetByIdAsync",
                                new Expression[]
                                {
                                    new MemberAccessExpression("id")
                                }
                            )
                        }
                    )
                }
            }
        };

        var expectedUser =
            """
            public class User
            {
                public Guid Id { get; set; }
                public string Username { get; set; }
                public string Email { get; set; }
                public DateTime CreatedAt { get; set; }
                public bool IsActive { get; set; }
            }
            """;
        var expectedUserRepo =
            """
            public interface IUserRepository
            {
                public async Task<User?> GetByIdAsync(Guid id);
                public async Task AddAsync(User user);
            }
            """;
        var expectedUserService =
            """
            public class UserService
            {
                private readonly IUserRepository _repository;
                public UserService(IUserRepository repository)
                {
                    _repository = repository;
                }
                public async Task<User?> GetUserByIdAsync(Guid id)
                {
                    return _repository.GetByIdAsync(id);
                }
            }
            """;

        var elements = new (RootElement Element, string Expected)[]
        {
            (user, expectedUser),
            (userRepo, expectedUserRepo),
            (userService, expectedUserService)
        };

        foreach (var (element, expected) in elements)
        {
            var result = _generator.Generate(element);
            AssertGeneratedCodeIsValid(result);
            AssertGeneratedCodeMatches(result, expected);
        }
    }

    /// <summary>
    /// Scénář 5: Kalkulačka — expression-bodied methods, různé expression typy
    /// </summary>
    [Fact]
    public void Scenario5_Calculator_GeneratesCompilableCode()
    {
        var calc = new ClassElement
        {
            Name = "Calculator",
            IsStatic = true,
            Methods =
            {
                CreateCalcMethod("Add", BinaryOperator.Add, "a", "b"),
                CreateCalcMethod("Subtract", BinaryOperator.Subtract, "a", "b"),
                CreateCalcMethod("Multiply", BinaryOperator.Multiply, "a", "b"),
                CreateCalcMethod("Divide", BinaryOperator.Divide, "a", "b"),
            }
        };

        // Add IsEven with modulo
        calc.Methods.Add(new MethodElement
        {
            Name = "IsEven",
            IsStatic = true,
            ReturnType = TypeModel.Bool,
            ExpressionBody = new BinaryExpression(
                new MemberAccessExpression("n"),
                BinaryOperator.Equal,
                new ConstantExpression(0)
            )
        });
        var isEven = calc.Methods.Last();
        isEven.Parameters.Add(new ParameterElement { Name = "n", Type = TypeModel.Int32 });

        var result = _generator.Generate(calc);
        AssertGeneratedCodeIsValid(result);
        AssertGeneratedCodeMatches(result,
            """
            public static class Calculator
            {
                public static int Add(int a, int b) => (a + b);
                public static int Subtract(int a, int b) => (a - b);
                public static int Multiply(int a, int b) => (a * b);
                public static int Divide(int a, int b) => (a / b);
                public static bool IsEven(int n) => (n == 0);
            }
            """);
    }

    /// <summary>
    /// Scénář 6: Autoservis se strong types — value objects jako Vogen-annotated typy.
    /// Demonstruje pattern: AI rozhodne o strong type → Translator vytvoří ValueObjectElement → Generator vyrenderuje Vogen [ValueObject].
    /// </summary>
    [Fact]
    public void Scenario6_AutoRepairShopWithStrongTypes_GeneratesCompilableCode()
    {
        var st = (string name, DataType baseType) =>
        {
            var vo = new ValueObjectElement
            {
                Name = name,
                IsReadOnly = true,
                Conversions = VogenConversions.None,
            };
            return vo;
        };

        // === CustomerId, PersonName, PhoneNumber, EmailAddress ===
        var customerId = st("CustomerId", DataType.Int32);
        var personName = st("PersonName", DataType.String);
        var phoneNumber = st("PhoneNumber", DataType.String);
        var emailAddress = st("EmailAddress", DataType.String);

        var customer = new ClassElement
        {
            Name = "Customer",
            InlineStrongTypes = { customerId, personName, phoneNumber, emailAddress },
            Properties =
            {
                PropertyElement.GetSet("Id", TypeModel.Of(DataType.Struct).WithCustomName("CustomerId")),
                PropertyElement.GetSet("Name", TypeModel.Of(DataType.Struct).WithCustomName("PersonName")),
                PropertyElement.GetSet("Phone", TypeModel.Of(DataType.Struct).WithCustomName("PhoneNumber")),
                PropertyElement.GetSet("Email", TypeModel.Of(DataType.Struct).WithCustomName("EmailAddress")),
            },
            Constructors =
            {
                ConstructorElement.Basic("Customer",
                    new ParameterElement { Name = "name", Type = TypeModel.Of(DataType.Struct).WithCustomName("PersonName") },
                    new ParameterElement { Name = "phone", Type = TypeModel.Of(DataType.Struct).WithCustomName("PhoneNumber") })
                    .WithBody(new BlockStatement(
                        new AssignmentStatement
                        {
                            Variable = "Name",
                            Value = new MemberAccessExpression("name")
                        },
                        new AssignmentStatement
                        {
                            Variable = "Phone",
                            Value = new MemberAccessExpression("phone")
                        }
                    ))
            }
        };

        var result = _generator.Generate(customer);
        AssertGeneratedCodeIsValid(result);
        AssertGeneratedCodeMatches(result,
            """
            [global::Vogen.ValueObject]
            public readonly partial struct CustomerId
            {
            }

            [global::Vogen.ValueObject]
            public readonly partial struct PersonName
            {
            }

            [global::Vogen.ValueObject]
            public readonly partial struct PhoneNumber
            {
            }

            [global::Vogen.ValueObject]
            public readonly partial struct EmailAddress
            {
            }

            public class Customer
            {
                public CustomerId Id { get; set; }
                public PersonName Name { get; set; }
                public PhoneNumber Phone { get; set; }
                public EmailAddress Email { get; set; }
                public Customer(PersonName name, PhoneNumber phone)
                {
                    Name = name;
                    Phone = phone;
                }
            }
            """);
    }

    private static MethodElement CreateCalcMethod(string name, BinaryOperator op, string paramA, string paramB)
    {
        var method = new MethodElement
        {
            Name = name,
            IsStatic = true,
            ReturnType = TypeModel.Int32,
            ExpressionBody = new BinaryExpression(
                new MemberAccessExpression(paramA),
                op,
                new MemberAccessExpression(paramB)
            )
        };
        method.Parameters.Add(new ParameterElement { Name = paramA, Type = TypeModel.Int32 });
        method.Parameters.Add(new ParameterElement { Name = paramB, Type = TypeModel.Int32 });
        return method;
    }

    // === Scénáře 7-8: Generator E2E Completeness (PROP-045) ===

    [Fact]
    public void Scenario7_AsyncPipeline_GeneratesCompilableCode()
    {
        var cls = new ClassElement { Name = "DataProcessor" };
        var fetchMethod = new MethodElement
        {
            Name = "FetchAsync",
            IsAsync = true,
            // Genrátor automaticky obalí návratový typ do Task<>, takže zde stačí string
            ReturnType = TypeModel.String,
            Body = new BlockStatement(
                new AssignmentStatement
                {
                    Variable = "var data",
                    Value = new AwaitExpression(new MemberAccessExpression("service.GetDataAsync"))
                },
                new ReturnStatement { Value = new MemberAccessExpression("data") }
            )
        };
        fetchMethod.Parameters.Add(new ParameterElement { Name = "service", Type = TypeModel.Of(DataType.Entity).WithCustomName("IDataService") });
        cls.Methods.Add(fetchMethod);

        var result = _generator.Generate(cls);
        AssertGeneratedCodeIsValid(result);
        AssertGeneratedCodeMatches(result,
            """
            public class DataProcessor
            {
                public async Task<string> FetchAsync(IDataService service)
                {
                    var data = await service.GetDataAsync;
                    return data;
                }
            }
            """);
    }

    [Fact]
    public void Scenario8_CollectionAndErrorHandling_GeneratesCompilableCode()
    {
        var cls = new ClassElement { Name = "CollectionProcessor" };
        var processMethod = new MethodElement
        {
            Name = "Process",
            ReturnType = TypeModel.Int32,
            Body = new BlockStatement(
                new AssignmentStatement { Variable = "var sum", Value = new ConstantExpression(0) },
                new ForEachStatement
                {
                    VariableName = "item",
                    Collection = new MemberAccessExpression("items"),
                    Body = new BlockStatement(
                        new AssignmentStatement
                        {
                            Variable = "sum",
                            Value = new BinaryExpression(
                                new MemberAccessExpression("sum"),
                                BinaryOperator.Add,
                                new MemberAccessExpression("item"))
                        })
                },
                new ReturnStatement { Value = new MemberAccessExpression("sum") })
        };
        processMethod.Parameters.Add(new ParameterElement { Name = "items", Type = TypeModel.Of(DataType.Entity).WithCustomName("List").WithGenericArg(TypeModel.Int32) });
        cls.Methods.Add(processMethod);

        var result = _generator.Generate(cls);
        AssertGeneratedCodeIsValid(result);
    }

    [Fact]
    public void Scenario9_ErrorHandling_GeneratesCompilableCode()
    {
        var cls = new ClassElement { Name = "SafeExecutor" };
        var executeMethod = new MethodElement
        {
            Name = "Execute",
            ReturnType = TypeModel.String,
            Body = new BlockStatement(
                new TryCatchStatement(
                    new BlockStatement(
                        new ReturnStatement { Value = new ConstantExpression("ok") }),
                    new CatchClause("Exception", "ex",
                        new BlockStatement(
                            new ReturnStatement
                            {
                                Value = new BinaryExpression(
                                    new ConstantExpression("Error: "),
                                    BinaryOperator.Add,
                                    new MemberAccessExpression("ex.Message"))
                            })))
                {
                    FinallyBody = new BlockStatement(
                        new AssignmentStatement
                        {
                            Variable = "var cleanup",
                            Value = new ConstantExpression("done")
                        })
                })
        };
        cls.Methods.Add(executeMethod);

        var result = _generator.Generate(cls);
        AssertGeneratedCodeIsValid(result);
    }

    [Fact]
    public void Scenario10_Conditional_GeneratesCompilableCode()
    {
        var cls = new ClassElement { Name = "TypeDemo" };
        var convertMethod = new MethodElement
        {
            Name = "GetValue",
            ReturnType = TypeModel.Int32,
            Body = new BlockStatement(
                new ReturnStatement
                {
                    Value = new ConditionalExpression(
                        new BinaryExpression(
                            new MemberAccessExpression("count"),
                            BinaryOperator.GreaterThan,
                            new ConstantExpression(0)),
                        new MemberAccessExpression("count"),
                        new ConstantExpression(-1))
                })
        };
        convertMethod.Parameters.Add(new ParameterElement { Name = "count", Type = TypeModel.Int32 });
        cls.Methods.Add(convertMethod);

        var result = _generator.Generate(cls);
        AssertGeneratedCodeIsValid(result);
    }

    [Fact]
    public void Scenario11_LambdaAndNullCoalescing_GeneratesCompilableCode()
    {
        var cls = new ClassElement { Name = "QueryBuilder" };
        var queryMethod = new MethodElement
        {
            Name = "Find",
            ReturnType = TypeModel.String,
            Body = new BlockStatement(
                new AssignmentStatement
                {
                    Variable = "var result",
                    Value = new MethodCallExpression("items.Where",
                        new Expression[]
                        {
                            new LambdaExpression(
                                new[] { "x" },
                                new BinaryExpression(
                                    new MemberAccessExpression("x.Length"),
                                    BinaryOperator.GreaterThan,
                                    new ConstantExpression(3)))
                        })
                },
                new ReturnStatement
                {
                    Value = new NullCoalescingExpression(
                        new MemberAccessExpression("result"),
                        new ConstantExpression("fallback"))
                })
        };
        queryMethod.Parameters.Add(new ParameterElement { Name = "items", Type = TypeModel.Of(DataType.Entity).WithCustomName("List").WithGenericArg(TypeModel.String) });
        cls.Methods.Add(queryMethod);

        var result = _generator.Generate(cls);
        AssertGeneratedCodeIsValid(result);
    }

    [Fact]
    public void Scenario12_SwitchAndDefault_GeneratesCompilableCode()
    {
        var cls = new ClassElement { Name = "SwitchDemo" };
        var processMethod = new MethodElement
        {
            Name = "Dispatch",
            ReturnType = TypeModel.String,
            Body = new BlockStatement(
                new SwitchStatement(
                    new MemberAccessExpression("code"),
                    new SwitchCase
                    {
                        Pattern = new ConstantExpression(1),
                        Body = new ReturnStatement { Value = new ConstantExpression("one") }
                    },
                    new SwitchCase
                    {
                        Pattern = new ConstantExpression(2),
                        Body = new ReturnStatement { Value = new ConstantExpression("two") }
                    })
                {
                    DefaultCase = new BlockStatement(
                        new ReturnStatement { Value = new ConstantExpression("other") })
                })
        };
        processMethod.Parameters.Add(new ParameterElement { Name = "code", Type = TypeModel.Int32 });
        cls.Methods.Add(processMethod);

        var result = _generator.Generate(cls);
        AssertGeneratedCodeIsValid(result);
    }

    [Fact]
    public void Scenario13_WhileAndLocalFunction_GeneratesCompilableCode()
    {
        var cls = new ClassElement { Name = "LoopDemo" };
        var processMethod = new MethodElement
        {
            Name = "SumTo",
            ReturnType = TypeModel.Int32,
            Body = new BlockStatement(
                new AssignmentStatement { Variable = "var total", Value = new ConstantExpression(0) },
                new AssignmentStatement { Variable = "var n", Value = new MemberAccessExpression("limit") },
                new WhileStatement
                {
                    Condition = new BinaryExpression(
                        new MemberAccessExpression("n"),
                        BinaryOperator.GreaterThan,
                        new ConstantExpression(0)),
                    Body = new BlockStatement(
                        new AssignmentStatement
                        {
                            Variable = "total",
                            Value = new BinaryExpression(
                                new MemberAccessExpression("total"),
                                BinaryOperator.Add,
                                new MemberAccessExpression("n"))
                        },
                        new AssignmentStatement
                        {
                            Variable = "n",
                            Value = new BinaryExpression(
                                new MemberAccessExpression("n"),
                                BinaryOperator.Subtract,
                                new ConstantExpression(1))
                        })
                },
                new ReturnStatement { Value = new MemberAccessExpression("total") })
        };
        processMethod.Parameters.Add(new ParameterElement { Name = "limit", Type = TypeModel.Int32 });
        cls.Methods.Add(processMethod);

        var result = _generator.Generate(cls);
        AssertGeneratedCodeIsValid(result);
    }

    private static void AssertGeneratedCodeIsValid(GeneratedCodeArtifact artifact)
    {
        artifact.SourceCode.Should().NotBeNullOrWhiteSpace();
        var isValid = SyntaxValidator.IsValid(artifact.SourceCode, out var diagnostics);
        if (!isValid)
        {
            // Vypiš kód pro debugging
            var codePreview = artifact.SourceCode.Length > 500
                ? artifact.SourceCode[..500] + "..."
                : artifact.SourceCode;
        }
        isValid.Should().BeTrue(
            $"Generated code for {artifact.FileName} should be valid C#:{Environment.NewLine}{diagnostics}{Environment.NewLine}Code:{Environment.NewLine}{artifact.SourceCode[..Math.Min(500, artifact.SourceCode.Length)]}");
    }

    /// <summary>
    /// Porovná vygenerovaný kód s očekávaným výstupem.
    /// Normalizuje whitespace (ignoruje mezery na koncích řádků a prázdné řádky).
    /// </summary>
    private static void AssertGeneratedCodeMatches(GeneratedCodeArtifact artifact, string expected)
    {
        var actual = NormalizeCode(artifact.SourceCode);
        var expectedNormalized = NormalizeCode(expected);

        actual.Should().Be(expectedNormalized,
            $"Generated code should match expected output.{Environment.NewLine}" +
            $"Expected:{Environment.NewLine}{expected}{Environment.NewLine}" +
            $"Actual:{Environment.NewLine}{artifact.SourceCode}");
    }

    private static string NormalizeCode(string code)
    {
        return string.Join("\n", code
            .Replace("\r\n", "\n")
            .Split('\n')
            .Select(line => line.TrimEnd())
            .Where(line => !string.IsNullOrWhiteSpace(line)));
    }
}

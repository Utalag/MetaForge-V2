# Elements — Expressions Test Coverage

| Test Method | Popis |
|-------------|-------|
| `ConstantExpressionTests.Constructor_NullValue_InferObject` | null → TypeModel.Object |
| `ConstantExpressionTests.Constructor_StringValue_InferString` | string → TypeModel.String |
| `ConstantExpressionTests.Constructor_IntValue_InferInt32` | int → TypeModel.Int32 |
| `ConstantExpressionTests.Constructor_LongValue_InferInt32` | long → TypeModel.Int32 |
| `ConstantExpressionTests.Constructor_ShortValue_InferInt32` | short → TypeModel.Int32 |
| `ConstantExpressionTests.Constructor_ByteValue_InferInt32` | byte → TypeModel.Int32 |
| `ConstantExpressionTests.Constructor_DecimalValue_InferDecimal` | decimal → TypeModel.Decimal |
| `ConstantExpressionTests.Constructor_DoubleValue_InferDecimal` | double → TypeModel.Decimal |
| `ConstantExpressionTests.Constructor_FloatValue_InferDecimal` | float → TypeModel.Decimal |
| `ConstantExpressionTests.Constructor_BoolValue_InferBool` | bool → TypeModel.Bool |
| `ConstantExpressionTests.Constructor_DateTimeValue_InferDateTime` | DateTime → TypeModel.DateTime |
| `ConstantExpressionTests.Constructor_DateTimeOffsetValue_InferDateTime` | DateTimeOffset → TypeModel.DateTime |
| `ConstantExpressionTests.Constructor_GuidValue_InferGuid` | Guid → TypeModel.Guid |
| `ConstantExpressionTests.Constructor_ExplicitResultType_OverridesInferred` | Explicitní resultType má přednost před inferencí |
| `ConstantExpressionTests.Constructor_UInt_FallsBackToObject` | uint není v pattern match → TypeModel.Object |
| `ConstantExpressionTests.Constructor_BigInteger_FallsBackToObject` | BigInteger není v pattern match → TypeModel.Object |
| `ConstantExpressionTests.Constructor_Char_FallsBackToObject` | char není v pattern match → TypeModel.Object |
| `BinaryExpressionTests.Constructor_EqualOperator_ResultIsBool` | Equal → TypeModel.Bool |
| `BinaryExpressionTests.Constructor_NotEqualOperator_ResultIsBool` | NotEqual → TypeModel.Bool |
| `BinaryExpressionTests.Constructor_GreaterThanOperator_ResultIsBool` | GreaterThan → TypeModel.Bool |
| `BinaryExpressionTests.Constructor_LessThanOperator_ResultIsBool` | LessThan → TypeModel.Bool |
| `BinaryExpressionTests.Constructor_ArithmeticOperatorBothInt32_ResultIsInt32` | Add/Subtract/Multiply/Divide/Modulo s oběma Int32 → Int32 |
| `BinaryExpressionTests.Constructor_ArithmeticOperatorLeftDecimal_ResultIsDecimal` | Add s Decimal + Int32 → Decimal (type promotion) |
| `BinaryExpressionTests.Constructor_ArithmeticOperatorRightDecimal_ResultIsDecimal` | Add s Int32 + Decimal → Decimal (type promotion) |
| `BinaryExpressionTests.Constructor_ConcatOperator_ResultIsString` | Concat → TypeModel.String |
| `BinaryExpressionTests.Constructor_AndOperator_ResultIsBool` | And → TypeModel.Bool |
| `BinaryExpressionTests.Constructor_OrOperator_ResultIsBool` | Or → TypeModel.Bool |
| `BinaryExpressionTests.Constructor_NullCoalesce_FallsBackToLeftResultType` | NullCoalesce není v switchi → fallback na left.ResultType |
| `BinaryExpressionTests.Constructor_ExplicitResultType_OverridesInferred` | Explicitní resultType má přednost |
| `UnaryExpressionTests.Constructor_NotOperator_ResultIsBool` | Not → TypeModel.Bool |
| `UnaryExpressionTests.Constructor_NegateOperator_ResultIsOperandType` | Negate → operand.ResultType |
| `UnaryExpressionTests.Constructor_BitwiseNot_ResultIsOperandType` | BitwiseNot → operand.ResultType |
| `UnaryExpressionTests.Constructor_Increment_ResultIsOperandType` | Increment → operand.ResultType |
| `UnaryExpressionTests.Constructor_Decrement_ResultIsOperandType` | Decrement → operand.ResultType |
| `UnaryExpressionTests.Constructor_ExplicitResultType_OverridesInferred` | Explicitní resultType má přednost |
| `ExpressionBasicTests.ExpressionKind_HasExpectedMembers` | Enum obsahuje Constant, MemberAccess, Binary, Unary, MethodCall, Lambda, New, Conditional, Computed |
| `ExpressionBasicTests.MethodCall_Constructor_SetsProperties` | MethodName a Arguments se nastaví |
| `ExpressionBasicTests.MethodCall_NoArguments_EmptyList` | Prázdné argumenty |
| `ExpressionBasicTests.MemberAccess_Constructor_SetsMemberPath` | MemberPath se nastaví |
| `ExpressionBasicTests.MemberAccess_DefaultResultType_IsObject` | Výchozí resultType je Object |
| `ExpressionBasicTests.Conditional_ResultType_EqualsWhenTrue` | ResultType = whenTrue.ResultType |
| `ExpressionBasicTests.ComputedExpression_DefaultOperation_IsIdentity` | Výchozí Operation je identity |
| `ExpressionBasicTests.ComputedExpression_Operands_IsEmpty` | Operands je prázdný |
| `BinaryOperatorTests.Enum_HasAllExpectedMembers` | Enum obsahuje Add, Subtract, Multiply, Divide, Modulo, Equal, NotEqual, GreaterThan, LessThan, GreaterThanOrEqual, LessThanOrEqual, And, Or, Concat, NullCoalesce |
| `UnaryOperatorTests.Enum_HasAllExpectedMembers` | Enum obsahuje Not, Negate, BitwiseNot, Increment, Decrement |

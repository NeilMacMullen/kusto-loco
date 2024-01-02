# Adding a User-Defined Function

*Note - this will hopefully get easier but for the moment here are the steps*

In BuiltInScalarFunctions add a declaration similar to 
```CSharp
 functions.Add(DateTimeToIso, new ScalarFunctionInfo(new ScalarOverloadInfo(new DateTimeToIsoImpl(),
     ScalarTypes.String,
     ScalarTypes.DateTime)));
```
at the end of BuiltInScalarFunctions static constructor

You will also need to provide a FunctionSymbol in the AdditionalFunctionSymbols region in the same file.

In the ScalarFuntions folder add an implenetation class.  This needs to provide both InvokeScalar and InvokeColumnar methods.  The InvokeColumnar method generally needs to...
- cast the input arguments to the appropriate type of column(s)
- create an output column of the same length as the input columns(s)
- iterate over the input rows and populate the output column
- return the output as a ColumnarResult

*Important* data in columns is nullable so you need to ensure that the function does somethign appropriate if one or more argumments are null. Note though that strings should not be null - instead string.Empty is used 

```CSharp
 internal class DateTimeToIsoImpl : IScalarFunctionImpl
 {
     public ScalarResult InvokeScalar(ScalarResult[] arguments)
     {
         Debug.Assert(arguments.Length == 1);
         var input = (DateTime?)arguments[0].Value;

         return new ScalarResult(ScalarTypes.String, Impl(input));
     }

     public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
     {
         Debug.Assert(arguments.Length == 1);
         var inputCol = (TypedBaseColumn<DateTime?>)arguments[0].Column;

         var data = new string?[inputCol.RowCount];
         for (var i = 0; i < inputCol.RowCount; i++)
         {
             data[i] = Impl(inputCol[i]);
         }

         return new ColumnarResult(ColumnFactory.Create(data));
     }

     private static string? Impl(DateTime? input)
         => input?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty;
 }
```
Ideally, you can add a test in SimpleFunctionTests then add some [documentation](additionalFunctions.md) for your new function.


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

Ideally, you can add a test in SimpleFunctionTests then add some [documentation](additionalFunctions.md) for your new function.


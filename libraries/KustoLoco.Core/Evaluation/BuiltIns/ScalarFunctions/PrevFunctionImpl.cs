using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoGeneric(Types = "all")]
internal abstract class BasePrevNext<T>(bool isPrev) : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
        => throw new InvalidOperationException("prev/next not supported for scalars");


    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        return NextPrevSlider<T>.InvokeColumnar(arguments, isPrev);
    }
}

[KustoGeneric(Types = "all")]
internal abstract class BasePrev<T> : BasePrevNext<T>
{
    internal BasePrev() :base(true){}
}

[KustoGeneric(Types = "all")]
internal abstract class BaseNext<T> : BasePrevNext<T>
{
    internal BaseNext() : base(false) { }
}



internal class PrevFunctionIntImpl : BasePrevOfint;
internal class PrevFunctionLongImpl : BasePrevOflong;
internal class PrevFunctionRealImpl : BasePrevOfdouble;
internal class PrevFunctionDecimalImpl : BasePrevOfdecimal;
internal class PrevFunctionStringImpl : BasePrevOfstring;
internal class PrevFunctionGuidImpl : BasePrevOfGuid;
internal class PrevFunctionTimespanImpl : BasePrevOfTimeSpan;
internal class PrevFunctionDateTimeImpl : BasePrevOfDateTime;




internal class NextFunctionIntImpl : BaseNextOfint;
internal class NextFunctionLongImpl : BaseNextOflong;
internal class NextFunctionRealImpl : BaseNextOfdouble;
internal class NextFunctionDecimalImpl : BaseNextOfdecimal;
internal class NextFunctionStringImpl : BaseNextOfstring;
internal class NextFunctionGuidImpl : BaseNextOfGuid;
internal class NextFunctionTimespanImpl : BaseNextOfTimeSpan;
internal class NextFunctionDateTimeImpl : BaseNextOfDateTime;

using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal abstract class BasePrevNext<T>(bool isPrev) : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
        => throw new InvalidOperationException("prev/next not supported for scalars");


    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        return NextPrevSlider.InvokeColumnar<T?>(arguments, isPrev);
    }
}

internal abstract class BasePrev<T> : BasePrevNext<T>
{
    internal BasePrev() :base(true){}
}


internal abstract class BaseNext<T> : BasePrevNext<T>
{
    internal BaseNext() : base(false) { }
}



internal class PrevFunctionIntImpl : BasePrev<int?>;
internal class PrevFunctionLongImpl : BasePrev<long?>;
internal class PrevFunctionRealImpl : BasePrev<double?>;
internal class PrevFunctionDecimalImpl : BasePrev<decimal?>;
internal class PrevFunctionStringImpl : BasePrev<string?>;
internal class PrevFunctionGuidImpl : BasePrev<Guid?>;
internal class PrevFunctionTimespanImpl : BasePrev<TimeSpan?>;
internal class PrevFunctionDateTimeImpl : BasePrev<DateTime?>;




internal class NextFunctionIntImpl : BaseNext<int?>;
internal class NextFunctionLongImpl : BaseNext<long?>;
internal class NextFunctionRealImpl : BaseNext<double?>;
internal class NextFunctionDecimalImpl : BaseNext<decimal?>;
internal class NextFunctionStringImpl : BaseNext<string?>;
internal class NextFunctionGuidImpl : BaseNext<Guid?>;
internal class NextFunctionTimespanImpl : BaseNext<TimeSpan?>;
internal class NextFunctionDateTimeImpl : BaseNext<DateTime?>;

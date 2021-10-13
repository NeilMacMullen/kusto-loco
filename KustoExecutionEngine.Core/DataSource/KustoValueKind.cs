using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KustoExecutionEngine.Core.DataSource
{
    public enum KustoValueKind
    {
        Bool,

        Int,

        Long,

        Real,

        Decimal,

        String,

        DateTime,

        TimeSpan,

        Guid,

        Type,

        Dynamic
    }
}

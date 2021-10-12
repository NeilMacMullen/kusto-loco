using Kusto.Language.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KustoExecutionEngine.Core
{
    internal class EmptyTabularSource : ITabularSource
    {
        public IRow GetNextRow()
        {
            throw new NotImplementedException();
        }
    }
}

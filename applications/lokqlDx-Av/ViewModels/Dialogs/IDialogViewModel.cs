using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokqlDx.ViewModels.Dialogs;
internal interface IDialogViewModel
{
    Task Result { get; }
}

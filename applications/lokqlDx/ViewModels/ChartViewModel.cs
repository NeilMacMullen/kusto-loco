using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LokqlDx.ViewModels 
{
    internal partial class ChartViewModel : ObservableObject
    {
        [RelayCommand]
        public void ToggleStyling()
        {
            // Implementation for toggling styles
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokqlDx.ViewModels;
public partial class CopilotChatViewModel : ObservableObject
{
    [ObservableProperty] double _fontSize = 20;
    internal void SetUiPreferences(UIPreferences uiPreferences)
    {
        FontSize = uiPreferences.FontSize;
    }
}

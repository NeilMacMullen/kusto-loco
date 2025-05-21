using Avalonia.Controls;
using lokqlDx;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ScottPlot.TickGenerators.Financial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokqlDx.Services;
public class DialogService
{
    public Task ShowMessageBox(string message)
    {
        var mb = MessageBoxManager.GetMessageBoxStandard("lokqlDx", message, ButtonEnum.Ok, Icon.None, WindowStartupLocation.CenterScreen);

        return mb.ShowAsync();
    }

    public Task ShowMessageBox(string title, string message)
    {
        var mb = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, Icon.None, WindowStartupLocation.CenterScreen);

        return mb.ShowAsync();
    }
    public async Task<YesNoCancel> ShowConfirmCancelBox(string title, string message)
    {
        var mb = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.YesNoCancel, Icon.None, WindowStartupLocation.CenterScreen);

        ButtonResult result = await mb.ShowAsync();

        return result switch
        {
            ButtonResult.Ok or ButtonResult.Yes => YesNoCancel.Yes,
            ButtonResult.No => YesNoCancel.No,
            _ => YesNoCancel.Cancel,
        };
    }
    public Task ShowAppPreferences() => throw new NotImplementedException();
}

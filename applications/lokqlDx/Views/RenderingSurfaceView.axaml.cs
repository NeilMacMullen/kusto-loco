using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LokqlDx.ViewModels;
using Vanara.PInvoke;

namespace LokqlDx.Views;

public partial class RenderingSurfaceView : UserControl, IDisposable
{
    public RenderingSurfaceView()
    {
        InitializeComponent();
    }

    public void Dispose()
    {
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }



    
}

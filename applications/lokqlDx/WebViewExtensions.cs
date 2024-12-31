using System.Drawing;
using System.IO;
using Microsoft.Web.WebView2.Core;

namespace lokqlDx;

public static class WebViewExtensions
{
    private static readonly IntPtr HWND_MESSAGE = new(-3);

    /// <summary>
    ///     Captures the image currently displayed in the webview.
    /// </summary>
    private static async Task<byte[]> CaptureImage(CoreWebView2 webview)
    {
        var stream = new MemoryStream();
        await webview.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);

        return stream.GetBuffer();
    }

    public static async Task<byte[]> RenderToImage(string html, double pixelWidth, double pixelHeight)
    {
        var environment = await CoreWebView2Environment.CreateAsync();
        var browserController = await environment.CreateCoreWebView2ControllerAsync(HWND_MESSAGE);
        var bounds = new Rectangle(0, 0, (int)pixelWidth, (int)pixelHeight);
        browserController.Bounds = bounds;
        await NavigateToStringAsync(browserController.CoreWebView2, html);
        var image = await CaptureImage(browserController.CoreWebView2);
        browserController.Close();
        return image;
    }

    /// <summary>
    ///     Navigates to a string in the specified webView and waits for the navigation to complete.
    /// </summary>
    public static async Task NavigateToStringAsync(CoreWebView2 webView, string htmlContent, bool retry = false)
    {
        try
        {
            var tcs = new TaskCompletionSource<bool>();

            void NavigationCompletedHandler(object sender, CoreWebView2NavigationCompletedEventArgs e)
            {
                webView.NavigationCompleted -= NavigationCompletedHandler!;
                tcs.SetResult(true);
            }

            webView.NavigationCompleted += NavigationCompletedHandler!;
            webView.NavigateToString(htmlContent);

            await tcs.Task;
        }
        catch
        {
            //sometimes we can't render content, for example if it's way too large, if so attempt to provide a warning
            if (!retry)
                await NavigateToStringAsync(webView, "<html><body><font color=\"red\">Unable to render content</font></body></html>", true);
        }
    }
}

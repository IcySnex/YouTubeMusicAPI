using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using System.Windows;
using System.Windows.Media.Imaging;

namespace YouTubeMusicAPI.Sample.WPF.ViewModels;

internal class AuthenticateViewModel
{
    readonly ILogger logger;

    readonly WebView2 webView;
    readonly Window window;
    readonly TaskCompletionSource taskCompletionSource = new();

    string? continueUrl = null;

    public AuthenticateViewModel(
        ILogger logger)
    {
        this.logger = logger;

        webView = new();
        window = new()
        {
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Title = "YouTubeMusicAPI Sample: WPF - Sign In",
            Width = 450,
            Height = 800,
            ResizeMode = ResizeMode.NoResize,
            Icon = BitmapFrame.Create(new Uri("pack://application:,,,/icon.ico", UriKind.Absolute)),
            Content = webView
        };
        window.Closed += (s, e) => taskCompletionSource.SetResult();
    }


    void OnNavigationStarting(
        object? _,
        CoreWebView2NavigationStartingEventArgs e)
    {
        logger.LogInformation("[AuthenticateViewModel-OnNavigationStarting] WebView navigating to page: {uri}", e.Uri);

        // Check if login done
        if (continueUrl is not null && continueUrl.Equals(e.Uri, StringComparison.InvariantCultureIgnoreCase))
            window.Dispatcher.Invoke(window.Close);

        // Get continue url
        NameValueCollection query = HttpUtility.ParseQueryString(e.Uri);

        if (query["continue"] is string c)
        {
            continueUrl = HttpUtility.UrlDecode(c);
            logger.LogInformation("[AuthenticateViewModel-OnNavigationStarting] Receieved continue url: {uri}", continueUrl);
        }
    }


    public async Task<IReadOnlyList<Cookie>> PromptAsync()
    {
        // Prepare WebView
        logger.LogInformation("[AuthenticateViewModel-PromptAsync] Preparing WebView...");

        _ = window.Dispatcher.BeginInvoke(window.ShowDialog);
        await webView.EnsureCoreWebView2Async();

        webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
        webView.CoreWebView2.Settings.IsZoomControlEnabled = false;
        webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
        webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

        webView.CoreWebView2.NavigationStarting += OnNavigationStarting;

        await webView.CoreWebView2.Profile.ClearBrowsingDataAsync();

        webView.CoreWebView2.Navigate("https://music.youtube.com/signin");

        // Wait until window closed
        await taskCompletionSource.Task;

        // Extract cookies
        logger.LogInformation("[AuthenticateViewModel-PromptAsync] Extracting cookies...");

        List<CoreWebView2Cookie> cookies = await webView.CoreWebView2.CookieManager.GetCookiesAsync("https://music.youtube.com");
        webView.Dispose();

        List<Cookie> result = [];
        foreach (CoreWebView2Cookie cookie in cookies)
        {
            result.Add(new()
            {
                Name = cookie.Name,
                Value = cookie.Value,
                Domain = cookie.Domain,
                Path = cookie.Path,
                Expires = cookie.Expires,
                Secure = cookie.IsSecure,
                HttpOnly = cookie.IsHttpOnly
            });
        }

        return result;
    }
}
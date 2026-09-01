using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace TreasureChamber.Desktop;

public partial class MainWindow : Window
{
    private readonly string _url;

    /// <summary>WebView2 用户数据目录（exe 旁，独立命名避免与程序更新混淆）。</summary>
    private static string UserDataFolder => Path.Combine(AppContext.BaseDirectory, "WebView2Data");

    public MainWindow(string url)
    {
        InitializeComponent();
        _url = url;
        Loaded += async (_, _) => await NavigateAsync();
    }

    private async Task NavigateAsync()
    {
        try
        {
            await EnsureCoreWebView2Async();
        }
        catch (WebView2RuntimeNotFoundException)
        {
            MessageBox.Show(
                "未检测到 WebView2 运行时（Windows 10/11 一般随 Edge 自带）。\n" +
                "请安装 Microsoft Edge WebView2 Runtime 后重试：\n" +
                "https://developer.microsoft.com/microsoft-edge/webview2/",
                "珍宝展厅", MessageBoxButton.OK, MessageBoxImage.Warning);
            System.Windows.Application.Current.Shutdown(-1);
            return;
        }
        catch (Exception first)
        {
            // E_UNEXPECTED(0x8000FFFF) 常见于用户数据目录损坏（如上次异常断电/强杀）。
            // 自愈：清空数据目录后重试一次
            try
            {
                if (Directory.Exists(UserDataFolder)) Directory.Delete(UserDataFolder, true);
                await EnsureCoreWebView2Async();
            }
            catch (Exception second)
            {
                MessageBox.Show(
                    $"加载界面失败（已尝试自动修复）：\n{second.Message}\n\n" +
                    $"首次错误：{first.Message}\n\n" +
                    $"用户数据目录：{UserDataFolder}",
                    "珍宝展厅", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown(-1);
                return;
            }
        }

        WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
        WebView.CoreWebView2.Settings.IsZoomControlEnabled = true;
        WebView.CoreWebView2.Settings.IsStatusBarEnabled = true;
        WebView.Source = new Uri(_url);
    }

    private async Task EnsureCoreWebView2Async()
    {
        var env = await CoreWebView2Environment.CreateAsync(null, UserDataFolder);
        await WebView.EnsureCoreWebView2Async(env);
    }
}

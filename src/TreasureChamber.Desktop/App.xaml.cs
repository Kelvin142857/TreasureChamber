using System.Threading;
using System.Windows;
using Microsoft.AspNetCore.Builder;
using TreasureChamber.WebApp;

namespace TreasureChamber.Desktop;

// 注意：基类必须全限定——TreasureChamber.Application 命名空间会遮蔽 System.Windows.Application
public partial class App : System.Windows.Application
{
    private const string MutexName = "TreasureChamber.Desktop.SingleInstance";
    private Mutex? _mutex;
    private WebApplication? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        // 单实例：避免与已运行实例（或同库的 Web 版）并发写 SQLite
        _mutex = new Mutex(true, MutexName, out var createdNew);
        if (!createdNew)
        {
            MessageBox.Show("珍宝展厅已在运行中，请勿重复启动。", "珍宝展厅",
                MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        base.OnStartup(e);

        try
        {
            // 进程内自托管后端：127.0.0.1 随机端口，数据目录在 exe 旁
            _host = WebAppHost.Create(desktopMode: true, contentRoot: AppContext.BaseDirectory);
            _host.StartAsync().GetAwaiter().GetResult();
            var url = WebAppHost.GetUrl(_host);

            var window = new MainWindow(url) { Title = "珍宝展厅 · 产品管理系统" };
            MainWindow = window;
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"启动失败：{ex.Message}", "珍宝展厅",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // 在后台线程停止后端：UI 线程同步等待可能与 WebView2 关闭流程死锁，
        // 最多等 3 秒，超时直接退出（进程结束时操作系统回收监听 socket）
        var host = _host;
        if (host != null)
        {
            Task.Run(async () =>
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    await host.StopAsync(cts.Token);
                    await host.DisposeAsync();
                }
                catch
                {
                    // 退出清理失败可忽略
                }
            }).Wait(3000);
        }
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}

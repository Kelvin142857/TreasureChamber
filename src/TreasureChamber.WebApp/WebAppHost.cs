using System.Text;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using TreasureChamber.Application.Services;
using TreasureChamber.Data;
using TreasureChamber.Data.Repo;

namespace TreasureChamber.WebApp;

/// <summary>
/// 共享 Web 宿主：Web 模式（0.0.0.0:5000 局域网）与桌面模式（127.0.0.1 随机端口）共用同一构架。
/// </summary>
public static class WebAppHost
{
    public static WebApplication Create(bool desktopMode = false, string? contentRoot = null)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = [],
            ContentRootPath = contentRoot
        });

        // Web 模式：绑定所有网卡 5000 端口（手机扫码访问）；
        // 桌面模式：仅本机随机端口（由 WebView2 承载）
        builder.WebHost.UseUrls(desktopMode ? "http://127.0.0.1:0" : "http://0.0.0.0:5000");

        var dataDir = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDir);

        builder.Services.AddDbContext<AppDbContext>(o =>
            o.UseSqlite($"Data Source={Path.Combine(dataDir, "treasure.db")}"));

        // 纯本地工具：不写 Windows 事件日志（沙箱/精简系统下会启动失败），仅控制台日志
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(o => o.SingleLine = true);

        // 数据保护密钥持久化到 App_Data，重启后防伪令牌/会话仍有效
        builder.Services.AddDataProtection()
            .SetApplicationName("TreasureChamber")
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(dataDir, "keys")));

        builder.Services.AddControllersWithViews();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(o => o.IdleTimeout = TimeSpan.FromHours(4));

        // 防伪令牌：内部 cookie 存 CookieToken（默认名 HttpOnly），另注入可读的
        // XSRF-TOKEN cookie 存放 RequestToken，前端 axios 自动回传 X-XSRF-TOKEN 头
        builder.Services.AddAntiforgery(o =>
        {
            o.HeaderName = "X-XSRF-TOKEN";
        });

        // Repos
        builder.Services.AddScoped<CategoryRepo>();
        builder.Services.AddScoped<SeriesRepo>();
        builder.Services.AddScoped<ProductRepo>();
        builder.Services.AddScoped<IntentOrderRepo>();
        builder.Services.AddScoped<SettingRepo>();
        // Services
        builder.Services.AddScoped<ProductImportService>();
        builder.Services.AddScoped<QrService>();
        builder.Services.AddScoped<IntentOrderService>();
        builder.Services.AddScoped<IntentOrderExportService>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            DbInitializer.Initialize(scope.ServiceProvider.GetRequiredService<AppDbContext>());
        }

        app.UseStaticFiles();
        app.UseSession();

        // 仅 GET 响应注入可读的 XSRF-TOKEN cookie（值为 RequestToken，SPA 读取后经
        // X-XSRF-TOKEN 头回传）；内部 CookieToken 由 GetAndStoreTokens 自动写入
        app.Use(async (context, next) =>
        {
            if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
            {
                var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
                var tokens = antiforgery.GetAndStoreTokens(context);
                context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
                    new CookieOptions { HttpOnly = false, SameSite = SameSiteMode.Lax });
            }
            await next(context);
        });

        app.UseRouting();
        app.MapControllers();
        // SPA 前端路由回退
        app.MapFallbackToFile("index.html");

        return app;
    }

    /// <summary>读取实际监听地址（桌面模式为 127.0.0.1:随机端口）。</summary>
    public static string GetUrl(WebApplication app)
    {
        var feature = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
        return feature?.Addresses.FirstOrDefault() ?? "";
    }
}

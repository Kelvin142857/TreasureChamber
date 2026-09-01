using System.Net;
using System.Net.Sockets;
using TreasureChamber.WebApp;

// Web 模式入口（局域网部署：0.0.0.0:5000）。
// 启动前先探测端口，被占用时给出友好提示（避免 CLR 崩溃弹窗）。
const int DefaultPort = 5000;

try
{
    using var probe = new TcpListener(IPAddress.Any, DefaultPort);
    probe.Start();
    probe.Stop();
}
catch (SocketException)
{
    Console.Error.WriteLine("错误：端口 " + DefaultPort + " 已被其他程序占用。");
    Console.Error.WriteLine("可能是 Docker 容器中的珍宝展厅正在运行（http://localhost:5000）。");
    Console.Error.WriteLine("请先停止它（docker stop treasurechamber），或换端口启动：");
    Console.Error.WriteLine("  ASPNETCORE_URLS=http://127.0.0.1:5001 dotnet run --project src\\TreasureChamber.WebApp");
    Environment.Exit(1);
}

try
{
    var app = WebAppHost.Create();
    app.Run();
}
catch (Exception ex)
{
    Console.Error.WriteLine("启动失败：" + ex.Message);
    Console.Error.WriteLine("若为端口冲突，请按上方提示处理。");
    Environment.Exit(1);
}

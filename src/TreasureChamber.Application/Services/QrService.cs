using System.IO.Compression;
using QRCoder;

namespace TreasureChamber.Application.Services;

/// <summary>二维码生成与批量导出（纯本地，QRCoder）。</summary>
public class QrService
{
    /// <summary>生成二维码 PNG 字节。</summary>
    public byte[] CreatePng(string content, int pixelsPerModule = 12)
    {
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        using var qr = new PngByteQRCode(data);
        return qr.GetGraphic(pixelsPerModule, [0, 0, 0], [255, 255, 255]);
    }

    /// <summary>批量导出 ZIP：每个产品一个 PNG，文件名 {型号}.png。</summary>
    public byte[] CreateZip(IEnumerable<(int ProductId, string Model, string Name, string Url)> products)
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var p in products)
            {
                var entry = archive.CreateEntry($"{SanitizeFileName(p.Model)}.png");
                using var entryStream = entry.Open();
                var png = CreatePng(p.Url);
                entryStream.Write(png, 0, png.Length);
            }
        }
        return ms.ToArray();
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
    }
}

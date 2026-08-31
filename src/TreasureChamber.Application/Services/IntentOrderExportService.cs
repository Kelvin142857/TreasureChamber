using MiniExcelLibs;
using TreasureChamber.Core.Entities;
using TreasureChamber.Core.Enums;

namespace TreasureChamber.Application.Services;

/// <summary>意向单导出 Excel。</summary>
public class IntentOrderExportService
{
    public byte[] Export(IEnumerable<IntentOrder> orders)
    {
        var rows = orders.SelectMany(o =>
        {
            var status = IntentOrderStatusLabels.All[o.Status];
            if (o.Items.Count == 0)
            {
                return new[]
                {
                    new
                    {
                        意向单号 = o.OrderNo,
                        客户姓名 = o.CustomerName,
                        联系电话 = o.CustomerPhone,
                        公司 = o.CustomerCompany ?? "",
                        产品型号 = "",
                        产品名称 = "",
                        数量 = (int?)null,
                        备注 = "",
                        下单时间 = o.CreatedAt,
                        状态 = status
                    }
                };
            }
            return o.Items.Select(i => new
            {
                意向单号 = o.OrderNo,
                客户姓名 = o.CustomerName,
                联系电话 = o.CustomerPhone,
                公司 = o.CustomerCompany ?? "",
                产品型号 = i.ProductModel,
                产品名称 = i.ProductName,
                数量 = (int?)i.Quantity,
                备注 = i.Remark ?? "",
                下单时间 = o.CreatedAt,
                状态 = status
            });
        });

        using var ms = new MemoryStream();
        MiniExcel.SaveAs(ms, rows, sheetName: "意向单");
        return ms.ToArray();
    }
}

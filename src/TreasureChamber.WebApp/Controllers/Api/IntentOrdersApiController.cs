using Microsoft.AspNetCore.Mvc;
using TreasureChamber.Application.Dtos;
using TreasureChamber.Application.Services;
using TreasureChamber.Core.Enums;
using TreasureChamber.Data.Repo;
using TreasureChamber.WebApp.Models;

namespace TreasureChamber.WebApp.Controllers.Api;

[ApiController]
[Route("api/intent-orders")]
public class IntentOrdersApiController(
    IntentOrderRepo repo,
    IntentOrderService service,
    IntentOrderExportService exportService) : ControllerBase
{
    // GET /api/intent-orders?status=&keyword=&page=
    [HttpGet]
    public async Task<ActionResult<PagedDto<IntentOrderDto>>> List(int? status, string? keyword, int page = 1)
    {
        var result = await repo.QueryAsync(status is int s ? (IntentOrderStatus)s : null, keyword, page);
        return Ok(new PagedDto<IntentOrderDto>(
            result.Items.Select(ApiMapper.ToIntentOrder).ToList(),
            result.Total, result.Page, result.PageSize, result.TotalPages));
    }

    // GET /api/intent-orders/export —— 导出 Excel
    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var orders = await repo.GetAllForExportAsync();
        var bytes = exportService.Export(orders);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"意向单_{DateTime.Now:yyyyMMddHHmm}.xlsx");
    }

    // POST /api/intent-orders —— 创建
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<IntentOrderDto>> Create(IntentOrderCreateDto dto)
    {
        var draft = new IntentOrderDraft
        {
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            CustomerCompany = dto.CustomerCompany,
            Note = dto.Note,
            Items = (dto.Items ?? new List<IntentOrderItemDto>())
                .Select(i => new IntentOrderItemDraft { ProductId = i.ProductId, Model = i.Model ?? "", Quantity = i.Quantity, Remark = i.Remark })
                .ToList()
        };
        var (order, error) = await service.CreateAsync(draft);
        if (error != null) return BadRequest(error);
        return Ok(ApiMapper.ToIntentOrder((await repo.GetDetailAsync(order!.Id))!));
    }

    // GET /api/intent-orders/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<IntentOrderDto>> Detail(int id)
    {
        var order = await repo.GetDetailAsync(id);
        if (order == null) return NotFound();
        return Ok(ApiMapper.ToIntentOrder(order));
    }

    // PUT /api/intent-orders/{id}/status?status=1
    [HttpPut("{id:int}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, int status)
    {
        if (!Enum.IsDefined(typeof(IntentOrderStatus), status)) return BadRequest("无效状态");
        await repo.UpdateStatusAsync(id, (IntentOrderStatus)status);
        return NoContent();
    }
}

using Microsoft.AspNetCore.Mvc;
using TreasureChamber.Core.Models;
using TreasureChamber.Data.Repo;
using TreasureChamber.WebApp.Models;

namespace TreasureChamber.WebApp.Controllers.Api;

[ApiController]
[Route("api/catalog")]
public class CatalogApiController(ProductRepo products, CategoryRepo categories, SeriesRepo series) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CatalogDto>> Get()
    {
        var seriesList = await series.WithCountsAsync();
        var tree = (await categories.GetAllAsync()).BuildTree(await categories.ProductCountsAsync());
        var recent = (await products.QueryAsync(new ProductQuery { PageSize = 8 })).Items;
        return Ok(new CatalogDto(
            seriesList.Select(s => new SeriesDto(s.Id, s.Name, s.Count)).ToList(),
            ApiMapper.ToTree(tree),
            recent.Select(ApiMapper.ToCard).ToList(),
            await products.CountAsync()));
    }
}

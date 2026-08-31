using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;
using TreasureChamber.Data.Repo;
using TreasureChamber.WebApp.Models;

namespace TreasureChamber.WebApp.Controllers.Api;

[ApiController]
[Route("api/settings")]
public class SettingsApiController(SettingRepo settings) : ControllerBase
{
    // GET /api/settings
    [HttpGet]
    public async Task<ActionResult<SettingsDto>> Get()
    {
        var requestBase = $"{Request.Scheme}://{Request.Host}";
        return Ok(new SettingsDto(
            await settings.GetAsync(SettingRepo.QrBaseUrl) ?? "",
            requestBase,
            GetLanIps()));
    }

    // PUT /api/settings —— { baseUrl: "http://192.168.1.100:5000" }
    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(SettingsUpdateDto dto)
    {
        var value = string.IsNullOrWhiteSpace(dto.BaseUrl) ? "" : dto.BaseUrl.Trim().TrimEnd('/');
        await settings.SetAsync(SettingRepo.QrBaseUrl, value);
        return NoContent();
    }

    private static List<string> GetLanIps()
    {
        try
        {
            return Dns.GetHostAddresses(Dns.GetHostName())
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.ToString())
                .ToList();
        }
        catch
        {
            return new List<string>();
        }
    }
}

using Microsoft.EntityFrameworkCore;

namespace TreasureChamber.Data.Repo;

public class SettingRepo(AppDbContext db) : BaseRepo(db)
{
    public const string QrBaseUrl = "QrBaseUrl";

    public async Task<string?> GetAsync(string key)
    {
        var setting = await Db.Settings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
    }

    public async Task SetAsync(string key, string value)
    {
        var setting = await Db.Settings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null)
        {
            Db.Settings.Add(new Core.Entities.SystemSetting { Key = key, Value = value });
        }
        else
        {
            setting.Value = value;
        }
        await Db.SaveChangesAsync();
    }
}

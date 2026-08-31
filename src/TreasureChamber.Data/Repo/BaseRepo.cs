using TreasureChamber.Core.Entities;

namespace TreasureChamber.Data.Repo;

public abstract class BaseRepo(AppDbContext db)
{
    protected readonly AppDbContext Db = db;
}

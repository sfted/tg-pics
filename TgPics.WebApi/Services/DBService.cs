using Microsoft.EntityFrameworkCore;
using TgPics.Core.Entities;

namespace TgPics.WebApi.Services;

public class DBService : DbContext
{
    public static string ConnectionString { get; set; }

    public DbSet<Post> Posts { get; set; }
    public DbSet<Picture> Pictures { get; set; }

    public DBService()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(
            ConnectionString,
            new MySqlServerVersion(new Version(8, 0, 11))
        );
    }
}

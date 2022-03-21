using Microsoft.EntityFrameworkCore;
using TgPics.Core.Entities;

namespace TgPics.WebApi.Services;

public class DatabaseService : DbContext
{
    public DatabaseService(DbContextOptions<DatabaseService> options)
        : base(options)
    {
        //Database.EnsureDeleted();
        Database.EnsureCreated();
    }

    public DbSet<Post> Posts { get; set; }
    public DbSet<MediaFile> Uploads { get; set; }
}

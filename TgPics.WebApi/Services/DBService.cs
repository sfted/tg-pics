using Microsoft.EntityFrameworkCore;
using TgPics.Core.Entities;

namespace TgPics.WebApi.Services;

public class DBService : DbContext
{
    public DbSet<Post> Posts { get; set; }
    public DbSet<Picture> Pictures { get; set; }

    public DBService()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=database;Trusted_Connection=True;");
    }
}

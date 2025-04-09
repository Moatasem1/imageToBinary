using ImagesToBinary.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;


namespace ImagesToBinary.Data;

class AppDB : DbContext
{
    public AppDB() { }

    public AppDB(DbContextOptions<AppDB> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            var connectionString = config.GetSection("DefaultConnection").Get<string>();
            optionsBuilder.UseSqlServer(connectionString);
        }
    } 
}

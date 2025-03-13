using ChatBot.Dal.Entites;
using Dal.Configurations;
using Microsoft.EntityFrameworkCore;
using MyFirstEBot.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstEBot;

public class MainContext : DbContext
{
    public DbSet<BotUser> Users { get; set; }
    public DbSet<UserInfo> UserInfos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=KOMILJON\\MSSQLSERVER01; Database=TgBot; User Id=sa; Password=1; TrustServerCertificate=True");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserInfoConfig());
    }


}

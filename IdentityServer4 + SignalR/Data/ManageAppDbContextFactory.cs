using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace IdentityServer4SignalR.Data
{
    public class ManageAppDbContextFactory : IDesignTimeDbContextFactory<ManageAppDbContext>
    {
        public ManageAppDbContext CreateDbContext(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json")
                .Build();
            var builder = new DbContextOptionsBuilder<ManageAppDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            /*object p = */
            builder.UseSqlServer(connectionString);
            return new ManageAppDbContext(builder.Options);
        }
    }
}
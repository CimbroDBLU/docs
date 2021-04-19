
using dblu.Docs.Models;
using dblu.Portale.Core.Infrastructure.Identity.Classes;
using dblu.Portale.Plugin.Docs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace dblu.Portale.Plugin.Docs.Data
{
    public class FileContext : DbContext
    {
        public FileContext(DbContextOptions<FileContext> options) :base(options)
        {

        }


        public DbSet<Allegati> Allegati { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Allegati>().HasKey(m => m.Id);
            base.OnModelCreating(builder);
        }
    }
}
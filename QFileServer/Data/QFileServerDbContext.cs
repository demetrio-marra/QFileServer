using Microsoft.EntityFrameworkCore;
using QFileServer.Data.Entities;

namespace QFileServer.Data
{
    public class QFileServerDbContext: DbContext
    {
        public QFileServerDbContext(DbContextOptions<QFileServerDbContext> options) : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(Console.WriteLine);
        }

        public DbSet<QFileServerEntity> FilesRepo { get; set; } = null!;
    }
}

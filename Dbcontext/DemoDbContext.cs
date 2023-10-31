using DocuBot_Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DocuBot_Api.Dbcontext
{
    public class DemoDbContext : DbContext
    {


        public DemoDbContext() { }

        public DemoDbContext(DbContextOptions<DemoDbContext> options)
              : base(options)
        {
        }

        public DbSet<usp_docuBOTPageProcess> usp_DocuBOTPageProcesses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<usp_docuBOTPageProcess>(entity =>
            {
                entity.HasNoKey();
               
            });
        }
    }
}

using DocuBot_Api.Models;
using DocuBot_Api.Models.RatingEngine_Models;
using DocuBot_Api.Models.User;
using Microsoft.EntityFrameworkCore;

namespace DocuBot_Api.Context
{
    public class DocubotDbContext : DbContext
    {
        public DocubotDbContext()
        {
        }

        public DocubotDbContext(DbContextOptions<DocubotDbContext> options)
            : base(options)
        {
        }


          public DbSet<usp_docuBOTPageProcess> usp_DocuBOTPageProcesses { get; set; }         
          public DbSet<LoanDetails> LoanDetailsDemo { get; set; }
          public DbSet<LoanDoc> LoanDocs { get; set; }
         public DbSet<LoanSchedule> loanschedule { get; set; }

        public virtual DbSet<UserInfo> UserInfos { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=98.70.8.54;Database=DEMODOCUBOT;User ID=hermesforms;password=integra123$%^;TrustServerCertificate=True");
                //optionsBuilder.UseSqlServer("Server=10.10.20.51;Database=DEMODOCUBOT1;User ID=sa;password=int123$%^;TrustServerCertificate=True");
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<LoanDetails>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");
                //entity.Property(e => e.Appid).HasColumnName("appid");
                entity.Property(e => e.Applno)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("applno");
                entity.Property(e => e.Approvaldate).HasColumnType("date").HasColumnName("approvaldate");
                entity.Property(e => e.Assetval).HasColumnName("assetval");
                entity.Property(e => e.Bounced).HasColumnName("bounced");
                entity.Property(e => e.Income).HasColumnName("income");
                entity.Property(e => e.Ccbal).HasColumnName("ccbal");
                entity.Property(e => e.Rating).HasColumnName("rating");
                entity.Property(e => e.Dependents).HasColumnName("dependents");
                entity.Property(e => e.Expenses).HasColumnName("expenses");
                entity.Property(e => e.Emistartdate).HasColumnName("emistartdate");
                entity.Property(e => e.RatingCalc).HasColumnName("RatingCalc");
                // Other properties...

                // Ensure that all columns from your SQL Server table are covered in your entity definition.
                // If there are additional properties in the C# entity that don't exist in the SQL table, you may want to remove them.

                entity.ToTable("LoanDetails"); // Make sure to specify the correct table name.
            });

            modelBuilder.Entity<LoanDoc>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DocId).HasColumnName("DocID");

                entity.Property(e => e.LoanApplNo)
                                .IsRequired()
                                .HasMaxLength(20)
                                .IsUnicode(false);
            });

            modelBuilder.Entity<UserInfo>(entity =>
            {
                entity.HasNoKey();
                entity.ToTable("UserInfo");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.DisplayName).HasMaxLength(60).IsUnicode(false);
                entity.Property(e => e.UserName).HasMaxLength(30).IsUnicode(false);
                entity.Property(e => e.Email).HasMaxLength(50).IsUnicode(false);
                entity.Property(e => e.Password).HasMaxLength(20).IsUnicode(false);
                entity.Property(e => e.CreatedDate).IsUnicode(false);
            });
        }
    }
}

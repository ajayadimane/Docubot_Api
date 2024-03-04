using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DocuBot_Api.Rating_Models;

public partial class RatingContext : DbContext
{
    public RatingContext()
    {
    }

    public RatingContext(DbContextOptions<RatingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Critlookup> Critlookups { get; set; }

    public virtual DbSet<ExtractTransactionDatum> ExtractTransactionData { get; set; }

    public virtual DbSet<ExtractionKeyValue> ExtractionKeyValues { get; set; }

    public virtual DbSet<Intrate> Intrates { get; set; }

    public virtual DbSet<Lbcriterion> Lbcriteria { get; set; }

    public virtual DbSet<Loadedfile> Loadedfiles { get; set; }

    public virtual DbSet<Loandetail> Loandetails { get; set; }

    public virtual DbSet<Loantype> Loantypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=10.10.20.51;Database=rating;Username=postgres;Password=postgres");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Critlookup>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("critlookup");

            entity.Property(e => e.Critcat)
                .HasMaxLength(35)
                .HasColumnName("critcat");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Points)
                .HasPrecision(10, 2)
                .HasColumnName("points");
            entity.Property(e => e.Rangehi).HasColumnName("rangehi");
            entity.Property(e => e.Rangelo).HasColumnName("rangelo");
            entity.Property(e => e.Weight)
                .HasPrecision(10, 2)
                .HasColumnName("weight");
        });

        modelBuilder.Entity<ExtractTransactionDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_etd");

            entity.ToTable("extract_transaction_data");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnType("character varying");
            entity.Property(e => e.Balance).HasColumnType("character varying");
            entity.Property(e => e.ChequeNumber).HasColumnType("character varying");
            entity.Property(e => e.Credit).HasColumnType("character varying");
            entity.Property(e => e.Debit).HasColumnType("character varying");
            entity.Property(e => e.Description).HasColumnType("character varying");
            entity.Property(e => e.Docid).HasColumnName("docid");
            entity.Property(e => e.InitBr)
                .HasColumnType("character varying")
                .HasColumnName("Init.Br");
            entity.Property(e => e.SerialNo).HasColumnType("character varying");
            entity.Property(e => e.TransactionId).HasColumnType("character varying");
            entity.Property(e => e.TxnDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("Txn_Date");
            entity.Property(e => e.ValueDate)
                .HasColumnType("character varying")
                .HasColumnName("Value_Date");

            entity.HasOne(d => d.Doc).WithMany(p => p.ExtractTransactionData)
                .HasForeignKey(d => d.Docid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Etd_fkey");
        });

        modelBuilder.Entity<ExtractionKeyValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ekv");

            entity.ToTable("extraction_key_value");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Accountdescription)
                .HasMaxLength(150)
                .HasColumnName("accountdescription");
            entity.Property(e => e.Accountholder)
                .HasMaxLength(150)
                .HasColumnName("accountholder");
            entity.Property(e => e.Accountno)
                .HasMaxLength(25)
                .HasColumnName("accountno");
            entity.Property(e => e.Accountstatus).HasColumnName("accountstatus");
            entity.Property(e => e.Accounttype).HasColumnName("accounttype");
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .HasColumnName("address");
            entity.Property(e => e.Address1).HasColumnName("address1");
            entity.Property(e => e.Address2).HasColumnName("address2");
            entity.Property(e => e.Address3).HasColumnName("address3");
            entity.Property(e => e.Balanceamount)
                .HasMaxLength(50)
                .HasColumnName("balanceamount");
            entity.Property(e => e.Balanceason)
                .HasMaxLength(50)
                .HasColumnName("balanceason");
            entity.Property(e => e.Bankaddress).HasColumnName("bankaddress");
            entity.Property(e => e.Bankaddress1).HasColumnName("bankaddress1");
            entity.Property(e => e.Bankaddress2).HasColumnName("bankaddress2");
            entity.Property(e => e.Bankaddress3).HasColumnName("bankaddress3");
            entity.Property(e => e.Bankname)
                .HasMaxLength(150)
                .HasColumnName("bankname");
            entity.Property(e => e.Branch)
                .HasMaxLength(50)
                .HasColumnName("branch");
            entity.Property(e => e.Branchcode).HasColumnName("branchcode");
            entity.Property(e => e.Cif).HasColumnName("cif");
            entity.Property(e => e.Cifno)
                .HasMaxLength(50)
                .HasColumnName("cifno");
            entity.Property(e => e.City).HasColumnName("city");
            entity.Property(e => e.Currency).HasColumnName("currency");
            entity.Property(e => e.Date)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date");
            entity.Property(e => e.Docid).HasColumnName("docid");
            entity.Property(e => e.Drawingpower).HasColumnName("drawingpower");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Ifsc)
                .HasMaxLength(50)
                .HasColumnName("ifsc");
            entity.Property(e => e.Interestrate).HasColumnName("interestrate");
            entity.Property(e => e.Jointholder).HasColumnName("jointholder");
            entity.Property(e => e.Micrcode)
                .HasMaxLength(50)
                .HasColumnName("micrcode");
            entity.Property(e => e.Mobileno).HasColumnName("mobileno");
            entity.Property(e => e.Modbalance).HasColumnName("modbalance");
            entity.Property(e => e.Nomination).HasColumnName("nomination");
            entity.Property(e => e.Nominationregistered)
                .HasMaxLength(10)
                .HasColumnName("nominationregistered");
            entity.Property(e => e.Opendate).HasColumnName("opendate");
            entity.Property(e => e.Pan).HasColumnName("pan");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Pincode).HasColumnName("pincode");
            entity.Property(e => e.Productcode).HasColumnName("productcode");
            entity.Property(e => e.Scheme).HasColumnName("scheme");
            entity.Property(e => e.State).HasColumnName("state");
            entity.Property(e => e.Statementdate)
                .HasColumnType("character varying")
                .HasColumnName("statementdate");
            entity.Property(e => e.Statementperiod)
                .HasMaxLength(150)
                .HasColumnName("statementperiod");
            entity.Property(e => e.Statementperiodfrom).HasColumnName("statementperiodfrom");
            entity.Property(e => e.Statementperiodto).HasColumnName("statementperiodto");

            entity.HasOne(d => d.Doc).WithMany(p => p.ExtractionKeyValues)
                .HasForeignKey(d => d.Docid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Ekv_fkey");
        });

        modelBuilder.Entity<Intrate>(entity =>
        {
            entity.HasKey(e => e.Rating).HasName("intrate_pkey");

            entity.ToTable("intrate");

            entity.Property(e => e.Rating)
                .ValueGeneratedNever()
                .HasColumnName("rating");
            entity.Property(e => e.Cl)
                .HasPrecision(7, 2)
                .HasColumnName("cl");
            entity.Property(e => e.El)
                .HasPrecision(7, 2)
                .HasColumnName("el");
            entity.Property(e => e.Hl)
                .HasPrecision(7, 2)
                .HasColumnName("hl");
            entity.Property(e => e.Pl)
                .HasPrecision(7, 2)
                .HasColumnName("pl");
            entity.Property(e => e.Twl)
                .HasPrecision(7, 2)
                .HasColumnName("twl");
        });

        modelBuilder.Entity<Lbcriterion>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("lbcriteria");

            entity.Property(e => e.CritCat).HasMaxLength(35);
            entity.Property(e => e.CritRegex).HasMaxLength(100);
            entity.Property(e => e.CritSource).HasMaxLength(25);
            entity.Property(e => e.Cycle)
                .HasMaxLength(5)
                .HasColumnName("cycle");
            entity.Property(e => e.Descripn).HasMaxLength(35);
            entity.Property(e => e.Formula)
                .HasMaxLength(100)
                .HasColumnName("formula");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Multiplier)
                .HasPrecision(5, 2)
                .HasColumnName("MULTIPLIER");
        });

        modelBuilder.Entity<Loadedfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_lf");

            entity.ToTable("loadedfiles");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Applno)
                .HasMaxLength(30)
                .HasColumnName("applno");
            entity.Property(e => e.Docname)
                .HasMaxLength(1000)
                .HasColumnName("docname");
        });

        modelBuilder.Entity<Loandetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ld");

            entity.ToTable("loandetails");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Applno)
                .HasMaxLength(30)
                .HasColumnName("applno");
            entity.Property(e => e.Approvaldate).HasColumnName("approvaldate");
            entity.Property(e => e.Assetval).HasColumnName("assetval");
            entity.Property(e => e.Bounced).HasColumnName("bounced");
            entity.Property(e => e.Ccbal).HasColumnName("ccbal");
            entity.Property(e => e.Cibil).HasColumnName("cibil");
            entity.Property(e => e.Custtype)
                .HasMaxLength(20)
                .HasColumnName("custtype");
            entity.Property(e => e.Delayed).HasColumnName("delayed");
            entity.Property(e => e.Dependents).HasColumnName("dependents");
            entity.Property(e => e.Disbdate).HasColumnName("disbdate");
            entity.Property(e => e.Disposableinc).HasColumnName("disposableinc");
            entity.Property(e => e.Emi).HasColumnName("emi");
            entity.Property(e => e.Emistartdate).HasColumnName("emistartdate");
            entity.Property(e => e.Expenses).HasColumnName("expenses");
            entity.Property(e => e.Income).HasColumnName("income");
            entity.Property(e => e.Interestrate).HasColumnName("interestrate");
            entity.Property(e => e.Loanamt).HasColumnName("loanamt");
            entity.Property(e => e.Loantype)
                .HasMaxLength(20)
                .HasColumnName("loantype");
            entity.Property(e => e.Lvr)
                .HasPrecision(7, 2)
                .HasColumnName("lvr");
            entity.Property(e => e.Othemi).HasColumnName("othemi");
            entity.Property(e => e.Owncontrib).HasColumnName("owncontrib");
            entity.Property(e => e.Permth).HasColumnName("permth");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Ratingcalc)
                .HasColumnType("character varying")
                .HasColumnName("ratingcalc");
            entity.Property(e => e.Rir)
                .HasPrecision(7, 2)
                .HasColumnName("rir");
            entity.Property(e => e.Sanctionedamt).HasColumnName("sanctionedamt");
            entity.Property(e => e.Taxpaid).HasColumnName("taxpaid");
            entity.Property(e => e.Tenor).HasColumnName("tenor");
        });

        modelBuilder.Entity<Loantype>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("loantype");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Loantype1)
                .HasMaxLength(5)
                .HasColumnName("loantype");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

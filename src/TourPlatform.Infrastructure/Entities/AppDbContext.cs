using Microsoft.EntityFrameworkCore;
using TourPlatform.Domain.Entities;

namespace TourPlatform.Infrastructure.Entities;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Pricingrecord> Pricingrecords { get; set; }

    public virtual DbSet<Route> Routes { get; set; }

    public virtual DbSet<Season> Seasons { get; set; }

    public virtual DbSet<Touroperator> Touroperators { get; set; }

    public virtual DbSet<Uploadhistory> Uploadhistories { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=TourOperator_Platform;Username=postgres;Password=Keissi");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("logs_pkey");

            entity.ToTable("logs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Exception).HasColumnName("exception");
            entity.Property(e => e.Level)
                .HasMaxLength(20)
                .HasColumnName("level");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Messagetemplate).HasColumnName("messagetemplate");
            entity.Property(e => e.Properties)
                .HasColumnType("jsonb")
                .HasColumnName("properties");
            entity.Property(e => e.Timestamp)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("timestamp");
        });

        modelBuilder.Entity<Pricingrecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pricingrecords_pkey");

            entity.ToTable("pricingrecords");

            entity.HasIndex(e => new { e.Routeid, e.Recorddate }, "idx_pricing_route_date");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Businessprice)
                .HasPrecision(10, 2)
                .HasColumnName("businessprice");
            entity.Property(e => e.Businessseats).HasColumnName("businessseats");
            entity.Property(e => e.Economyprice)
                .HasPrecision(10, 2)
                .HasColumnName("economyprice");
            entity.Property(e => e.Economyseats).HasColumnName("economyseats");
            entity.Property(e => e.Recorddate).HasColumnName("recorddate");
            entity.Property(e => e.Routeid).HasColumnName("routeid");
            entity.Property(e => e.Seasonid).HasColumnName("seasonid");
            entity.Property(e => e.Uploadedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("uploadedat");
            entity.Property(e => e.Uploadedby).HasColumnName("uploadedby");

            entity.HasOne(d => d.Route).WithMany(p => p.Pricingrecords)
                .HasForeignKey(d => d.Routeid)
                .HasConstraintName("fk_pricing_route");

            entity.HasOne(d => d.Season).WithMany(p => p.Pricingrecords)
                .HasForeignKey(d => d.Seasonid)
                .HasConstraintName("fk_pricing_season");

            entity.HasOne(d => d.UploadedbyNavigation).WithMany(p => p.Pricingrecords)
                .HasForeignKey(d => d.Uploadedby)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_pricing_user");
        });

        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("routes_pkey");

            entity.ToTable("routes");

            entity.HasIndex(e => e.Touroperatorid, "idx_routes_touroperator");

            entity.HasIndex(e => new { e.Touroperatorid, e.Routecode }, "uq_route_code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Destination)
                .HasMaxLength(50)
                .HasColumnName("destination");
            entity.Property(e => e.Origin)
                .HasMaxLength(50)
                .HasColumnName("origin");
            entity.Property(e => e.Routecode)
                .HasMaxLength(50)
                .HasColumnName("routecode");
            entity.Property(e => e.Touroperatorid).HasColumnName("touroperatorid");

            entity.HasOne(d => d.Touroperator).WithMany(p => p.Routes)
                .HasForeignKey(d => d.Touroperatorid)
                .HasConstraintName("fk_route_touroperator");
        });

        modelBuilder.Entity<Season>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("seasons_pkey");

            entity.ToTable("seasons");

            entity.HasIndex(e => e.Seasoncode, "seasons_seasoncode_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Enddate).HasColumnName("enddate");
            entity.Property(e => e.Seasoncode)
                .HasMaxLength(50)
                .HasColumnName("seasoncode");
            entity.Property(e => e.Startdate).HasColumnName("startdate");
        });

        modelBuilder.Entity<Touroperator>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("touroperators_pkey");

            entity.ToTable("touroperators");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Contactemail)
                .HasMaxLength(100)
                .HasColumnName("contactemail");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Uploadhistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("uploadhistory_pkey");

            entity.ToTable("uploadhistory");

            entity.HasIndex(e => e.Touroperatorid, "idx_upload_touroperator");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Filename)
                .HasMaxLength(200)
                .HasColumnName("filename");
            entity.Property(e => e.Logpath)
                .HasMaxLength(300)
                .HasColumnName("logpath");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.Totalrows).HasColumnName("totalrows");
            entity.Property(e => e.Touroperatorid).HasColumnName("touroperatorid");
            entity.Property(e => e.Uploadedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("uploadedat");

            entity.HasOne(d => d.Touroperator).WithMany(p => p.Uploadhistories)
                .HasForeignKey(d => d.Touroperatorid)
                .HasConstraintName("fk_upload_touroperator");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Passwordhash).HasColumnName("passwordhash");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
            entity.Property(e => e.Touroperatorid).HasColumnName("touroperatorid");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Touroperator).WithMany(p => p.Users)
                .HasForeignKey(d => d.Touroperatorid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_user_touroperator");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

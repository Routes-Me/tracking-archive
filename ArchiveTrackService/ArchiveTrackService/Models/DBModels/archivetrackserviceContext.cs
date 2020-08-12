using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ArchiveTrackService.Models.DBModels
{
    public partial class archivetrackserviceContext : DbContext
    {
        public archivetrackserviceContext()
        {
        }

        public archivetrackserviceContext(DbContextOptions<archivetrackserviceContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Archivecoordinates> Archivecoordinates { get; set; }
        public virtual DbSet<Archivevehicles> Archivevehicles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySql("server=localhost;port=3306;user=nirmal;password=NirmalTheOne@123;database=archivetrackservice", x => x.ServerVersion("8.0.20-mysql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Archivecoordinates>(entity =>
            {
                entity.ToTable("archivecoordinates");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Latitude)
                    .HasColumnName("latitude")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Longitude)
                    .HasColumnName("longitude")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TimeStamp)
                    .HasColumnName("timeStamp")
                    .HasColumnType("datetime");

                entity.Property(e => e.VehicleNumber).HasColumnName("vehicleNumber");
            });

            modelBuilder.Entity<Archivevehicles>(entity =>
            {
                entity.ToTable("archivevehicles");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DeviceId)
                    .HasColumnName("deviceId")
                    .HasColumnType("varchar(100)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_bin");

                entity.Property(e => e.InstitutionId).HasColumnName("institutionId");

                entity.Property(e => e.IsLive).HasColumnName("isLive");

                entity.Property(e => e.TimeStamp)
                    .HasColumnName("timeStamp")
                    .HasColumnType("datetime");

                entity.Property(e => e.VehicleNumber).HasColumnName("vehicleNumber");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

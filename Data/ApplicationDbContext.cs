using IT_13FinalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace IT_13FinalProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<HealthRecord> HealthRecords { get; set; } = null!;
        public DbSet<VitalSign> VitalSigns { get; set; } = null!;
        public DbSet<PharmacyInventory> PharmacyInventory { get; set; } = null!;
        public DbSet<PatientBill> PatientBills { get; set; } = null!;
        public DbSet<NurseNote> NurseNotes { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<LabResult> LabResults { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<RoleStatus> RoleStatuses { get; set; } = null!;
        public DbSet<RolePermissionEntry> RolePermissionEntries { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FullName).HasMaxLength(200);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                // Ensure unique username and email
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Patient entity
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(e => e.PatientId);
                entity.Property(e => e.PatientId)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn();
                
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Gender).HasMaxLength(10);
                entity.Property(e => e.CivilStatus).HasMaxLength(20);
                entity.Property(e => e.Occupation).HasMaxLength(100);
                entity.Property(e => e.Nationality).HasMaxLength(50);
                entity.Property(e => e.Religion).HasMaxLength(50);
                entity.Property(e => e.EmergencyContactName).HasMaxLength(100);
                entity.Property(e => e.EmergencyContactRelationship).HasMaxLength(50);
                entity.Property(e => e.EmergencyContactNumber).HasMaxLength(20);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure PatientBill entity
            modelBuilder.Entity<PatientBill>(entity =>
            {
                entity.HasKey(e => e.BillId);
                entity.Property(e => e.BillId)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn();
                
                entity.Property(e => e.PatientName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PatientEmail).HasMaxLength(100);
                entity.Property(e => e.AssessmentStatus).IsRequired().HasMaxLength(50).HasDefaultValue("Pending");
                entity.Property(e => e.TotalAmount).HasDefaultValue(0);
                entity.Property(e => e.InsuranceCoverage).HasDefaultValue(0);
                entity.Property(e => e.PatientResponsibility).HasDefaultValue(0);
                entity.Property(e => e.PaymentStatus).HasDefaultValue("Unpaid");
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.InsuranceProvider).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsArchived).HasDefaultValue(false);

                // Configure relationship with Patient
                entity.HasOne(e => e.Patient)
                    .WithMany()
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.AppointmentId);
                entity.Property(e => e.AppointmentId)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn();

                entity.Property(e => e.PatientName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("SCHEDULED");
                entity.Property(e => e.Type).HasMaxLength(100).HasDefaultValue("Consultation");

                entity.HasOne(e => e.Patient)
                    .WithMany()
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<LabResult>(entity =>
            {
                entity.HasKey(e => e.LabResultId);
                entity.Property(e => e.LabResultId)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn();

                entity.Property(e => e.PatientName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.TestName).IsRequired().HasMaxLength(200);

                entity.HasOne(e => e.Patient)
                    .WithMany()
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("audit_logs");
                entity.HasKey(e => e.AuditLogId);
                entity.Property(e => e.AuditLogId)
                    .HasColumnName("audit_log_id")
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn();
                entity.Property(e => e.TimestampUtc)
                    .HasColumnName("timestamp_utc");
                entity.Property(e => e.UserName)
                    .HasColumnName("user_name")
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.Role)
                    .HasColumnName("role")
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Action)
                    .HasColumnName("action")
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(e => e.IpAddress)
                    .HasColumnName("ip_address")
                    .HasMaxLength(100);
                entity.Property(e => e.Device)
                    .HasColumnName("device")
                    .HasMaxLength(200);
                entity.Property(e => e.Details)
                    .HasColumnName("details");
            });

            modelBuilder.Entity<RoleStatus>(entity =>
            {
                entity.ToTable("role_status");
                entity.HasKey(e => e.RoleStatusId);
                entity.Property(e => e.RoleStatusId)
                    .HasColumnName("role_status_id")
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn();
                entity.Property(e => e.RoleKey)
                    .HasColumnName("role_key")
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.IsEnabled)
                    .HasColumnName("is_enabled");
                entity.HasIndex(e => e.RoleKey).IsUnique();
            });

            modelBuilder.Entity<RolePermissionEntry>(entity =>
            {
                entity.ToTable("role_permissions");
                entity.HasKey(e => e.RolePermissionEntryId);
                entity.Property(e => e.RolePermissionEntryId)
                    .HasColumnName("role_permission_entry_id")
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn();
                entity.Property(e => e.RoleKey)
                    .HasColumnName("role_key")
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.PermissionKey)
                    .HasColumnName("permission_key")
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.IsAllowed)
                    .HasColumnName("is_allowed");
                entity.HasIndex(e => new { e.RoleKey, e.PermissionKey }).IsUnique();
            });
        }
    }
}

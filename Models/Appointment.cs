using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_13FinalProject.Models
{
    [Table("appointments")]
    public class Appointment
    {
        [Key]
        [Column("appointment_id")]
        public int AppointmentId { get; set; }

        [Column("external_id")]
        public Guid? ExternalId { get; set; }

        [Column("patient_id")]
        public int? PatientId { get; set; }

        public Patient? Patient { get; set; }

        [Column("doctor_id")]
        public int? DoctorId { get; set; }

        [Column("department_id")]
        public int? DepartmentId { get; set; }

        [Required]
        [StringLength(200)]
        [Column("patient_name")]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        [Column("appointment_date")]
        public DateTime StartDateTime { get; set; }

        [Column("duration_minutes")]
        public int DurationMinutes { get; set; } = 30;

        [StringLength(100)]
        [Column("appointment_type")]
        public string Type { get; set; } = "Consultation";

        [StringLength(50)]
        [Column("status")]
        public string Status { get; set; } = "SCHEDULED";

        [StringLength(500)]
        [Column("reason")]
        public string Reason { get; set; } = string.Empty;

        [Column("notes")]
        public string? Notes { get; set; }

        [StringLength(500)]
        [Column("tags")]
        public string? Tags { get; set; }

        [Column("is_cancelled")]
        public bool IsCancelled { get; set; }

        [Column("cancelled_at")]
        public DateTime? CancelledAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}

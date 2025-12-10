using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_13FinalProject.Models
{
    [Table("vital_signs")]
    public class VitalSign
    {
        [Key]
        [Column("vital_sign_id")]
        public int VitalSignId { get; set; }

        [Column("patient_id")]
        public int PatientId { get; set; }

        [Column("nurse_id")]
        public int? NurseId { get; set; }

        [Column("doctor_id")]
        public int? DoctorId { get; set; }

        [Column("recorded_date")]
        public DateTime RecordedDate { get; set; }

        [Column("chief_complaint")]
        [StringLength(500)]
        public string? ChiefComplaint { get; set; }

        [Column("blood_pressure")]
        [StringLength(50)]
        public string? BloodPressure { get; set; }

        [Column("heart_rate")]
        public int? HeartRate { get; set; }

        [Column("respiratory_rate")]
        public int? RespiratoryRate { get; set; }

        [Column("temperature")]
        public decimal? Temperature { get; set; }

        [Column("oxygen_saturation")]
        public decimal? OxygenSaturation { get; set; }

        [Column("weight")]
        public decimal? Weight { get; set; }

        [Column("height")]
        public decimal? Height { get; set; }

        [Column("body_mass_index")]
        public decimal? BodyMassIndex { get; set; }

        [Column("notes")]
        [StringLength(1000)]
        public string? Notes { get; set; }

        [Column("allergies")]
        [StringLength(500)]
        public string? Allergies { get; set; }

        [Column("past_illnesses")]
        [StringLength(500)]
        public string? PastIllnesses { get; set; }

        [Column("past_surgeries")]
        [StringLength(500)]
        public string? PastSurgeries { get; set; }

        [Column("current_medications")]
        [StringLength(500)]
        public string? CurrentMedications { get; set; }

        [Column("family_history")]
        [StringLength(500)]
        public string? FamilyHistory { get; set; }

        [Column("immunization_history")]
        [StringLength(500)]
        public string? ImmunizationHistory { get; set; }

        [Column("time_in_out")]
        [StringLength(50)]
        public string? TimeInOut { get; set; }

        [Column("department")]
        [StringLength(100)]
        public string? Department { get; set; }

        [Column("type_of_visit")]
        [StringLength(50)]
        public string? TypeOfVisit { get; set; }

        [Column("duration_symptoms")]
        [StringLength(500)]
        public string? DurationSymptoms { get; set; }

        [Column("remarks")]
        [StringLength(1000)]
        public string? Remarks { get; set; }

        [Column("doctor_assisted")]
        [StringLength(100)]
        public string? DoctorAssisted { get; set; }

        [Column("nurse_name")]
        [StringLength(100)]
        public string? NurseName { get; set; }

        [Column("is_archived")]
        public bool IsArchived { get; set; } = false;

        [Column("archived_at")]
        public DateTime? ArchivedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        // Navigation property
        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }
    }
}

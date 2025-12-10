using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_13FinalProject.Models
{
    [Table("consultations")]
    public class Consultation
    {
        [Key]
        [Column("consultation_id")]
        public int ConsultationId { get; set; }

        [Column("patient_id")]
        public int? PatientId { get; set; }

        [Column("nurse_id")]
        public int? NurseId { get; set; }

        [Column("doctor_id")]
        public int? DoctorId { get; set; }

        [Required]
        [StringLength(500)]
        [Column("allergies")]
        public string? Allergies { get; set; }

        [StringLength(500)]
        [Column("past_illnesses")]
        public string? PastIllnesses { get; set; }

        [StringLength(500)]
        [Column("past_surgeries")]
        public string? PastSurgeries { get; set; }

        [StringLength(500)]
        [Column("current_medications")]
        public string? CurrentMedications { get; set; }

        [StringLength(500)]
        [Column("family_history")]
        public string? FamilyHistory { get; set; }

        [StringLength(500)]
        [Column("immunization_history")]
        public string? ImmunizationHistory { get; set; }

        [StringLength(50)]
        [Column("blood_pressure")]
        public string? BloodPressure { get; set; }

        [StringLength(50)]
        [Column("heart_rate")]
        public string? HeartRate { get; set; }

        [StringLength(50)]
        [Column("respiratory_rate")]
        public string? RespiratoryRate { get; set; }

        [StringLength(50)]
        [Column("temperature")]
        public string? Temperature { get; set; }

        [StringLength(50)]
        [Column("oxygen_saturation")]
        public string? OxygenSaturation { get; set; }

        [StringLength(50)]
        [Column("weight")]
        public string? Weight { get; set; }

        [StringLength(50)]
        [Column("height")]
        public string? Height { get; set; }

        [StringLength(50)]
        [Column("bmi")]
        public string? Bmi { get; set; }

        [StringLength(200)]
        [Column("doctor_assisted")]
        public string? DoctorAssisted { get; set; }

        [StringLength(200)]
        [Column("nurse_name")]
        public string? NurseName { get; set; }

        [Column("date_of_visit")]
        public DateTime? DateOfVisit { get; set; }

        [StringLength(100)]
        [Column("time_in_out")]
        public string? TimeInOut { get; set; }

        [StringLength(200)]
        [Column("department")]
        public string? Department { get; set; }

        [StringLength(100)]
        [Column("type_of_visit")]
        public string? TypeOfVisit { get; set; }

        [StringLength(500)]
        [Column("chief_complaint")]
        public string? ChiefComplaint { get; set; }

        [StringLength(500)]
        [Column("duration_symptoms")]
        public string? DurationSymptoms { get; set; }

        [Column("remarks", TypeName = "text")]
        public string? Remarks { get; set; }

        [Column("is_archived")]
        public bool IsArchived { get; set; } = false;

        [Column("archived_at")]
        public DateTime? ArchivedAt { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Doctor Consultation Form Fields
        [StringLength(1000)]
        [Column("subjective_findings")]
        public string? SubjectiveFindings { get; set; }

        [StringLength(1000)]
        [Column("objective_findings")]
        public string? ObjectiveFindings { get; set; }

        [StringLength(1000)]
        [Column("assessment_diagnosis")]
        public string? AssessmentDiagnosis { get; set; }

        [StringLength(50)]
        [Column("icd10_code")]
        public string? Icd10Code { get; set; }

        [StringLength(500)]
        [Column("additional_tests_order")]
        public string? AdditionalTestsOrder { get; set; }

        [StringLength(1000)]
        [Column("doctor_remarks")]
        public string? DoctorRemarks { get; set; }

        [StringLength(1000)]
        [Column("nurse_remarks")]
        public string? NurseRemarks { get; set; }

        [StringLength(1000)]
        [Column("recommendation")]
        public string? Recommendation { get; set; }

        [StringLength(500)]
        [Column("medicine_name")]
        public string? MedicineName { get; set; }

        [StringLength(200)]
        [Column("dosage")]
        public string? Dosage { get; set; }

        [StringLength(200)]
        [Column("frequency")]
        public string? Frequency { get; set; }

        [StringLength(200)]
        [Column("duration")]
        public string? Duration { get; set; }

        [StringLength(500)]
        [Column("special_instructions")]
        public string? SpecialInstructions { get; set; }

        [Column("follow_up_date")]
        public DateTime? FollowUpDate { get; set; }

        [StringLength(50)]
        [Column("visit_status")]
        public string? VisitStatus { get; set; }

        [StringLength(200)]
        [Column("doctor_signature")]
        public string? DoctorSignature { get; set; }

        [Column("date_approved")]
        public DateTime? DateApproved { get; set; }

        [StringLength(1000)]
        [Column("doctor_initial_remarks")]
        public string? DoctorInitialRemarks { get; set; }

        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }
    }
}

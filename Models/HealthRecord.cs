using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_13FinalProject.Models
{
    [Table("health_records")]
    public class HealthRecord
    {
        [Key]
        [Column("record_id")]
        public int RecordId { get; set; }

        [Column("patient_id")]
        public int? PatientId { get; set; }

        [Column("doctor_id")]
        public int? DoctorId { get; set; }

        [Column("nurse_id")]
        public int? NurseId { get; set; }

        [Column("chief_complaint")]
        public string? ChiefComplaint { get; set; }

        [Column("symptom_duration")]
        public string? SymptomDuration { get; set; }

        [Column("visit_type")]
        public string? VisitType { get; set; }

        [Column("department")]
        public string? Department { get; set; }

        [Column("time_in_out")]
        public string? TimeInOut { get; set; }

        [Column("subjective_findings")]
        public string? SubjectiveFindings { get; set; }

        [Column("objective_findings")]
        public string? ObjectiveFindings { get; set; }

        [Column("assessment_diagnosis")]
        public string? AssessmentDiagnosis { get; set; }

        [Column("icd10_code")]
        public string? Icd10Code { get; set; }

        [Column("additional_tests_order")]
        public string? AdditionalTestsOrder { get; set; }

        [Column("doctor_remarks")]
        public string? DoctorRemarks { get; set; }

        [Column("nurse_remarks")]
        public string? NurseRemarks { get; set; }

        [Column("recommendations")]
        public string? Recommendations { get; set; }

        [Column("medicine_name")]
        public string? MedicineName { get; set; }

        [Column("dosage")]
        public string? Dosage { get; set; }

        [Column("frequency")]
        public string? Frequency { get; set; }

        [Column("duration")]
        public string? Duration { get; set; }

        [Column("treatment_notes")]
        public string? TreatmentNotes { get; set; }

        [Column("follow_up_date")]
        public DateTime? FollowUpDate { get; set; }

        [Column("visit_status")]
        public string? VisitStatus { get; set; }

        [Column("doctor_signature")]
        public string? DoctorSignature { get; set; }

        [Column("approval_date")]
        public DateTime? ApprovalDate { get; set; }

        [Column("record_date")]
        public DateTime? RecordDate { get; set; }

        [Column("diagnosis")]
        public string? Diagnosis { get; set; }

        [Column("treatment")]
        public string? Treatment { get; set; }

        [Column("prescription")]
        public string? Prescription { get; set; }

        [Column("special_instructions")]
        public string? SpecialInstructions { get; set; }

        [Column("doctor_initial_remarks")]
        public string? DoctorInitialRemarks { get; set; }

        [Column("is_archived")]
        public bool IsArchived { get; set; } = false;

        [Column("archived_at")]
        public DateTime? ArchivedAt { get; set; }

        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }
    }
}

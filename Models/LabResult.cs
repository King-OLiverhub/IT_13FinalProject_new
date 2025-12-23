using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_13FinalProject.Models
{
    [Table("lab_results")]
    public class LabResult
    {
        [Key]
        [Column("lab_result_id")]
        public int LabResultId { get; set; }

        [Column("external_id")]
        public Guid? ExternalId { get; set; }

        [Column("patient_id")]
        public int? PatientId { get; set; }

        public Patient? Patient { get; set; }

        [Required]
        [StringLength(200)]
        [Column("patient_name")]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Column("test_name")]
        public string TestName { get; set; } = string.Empty;

        [Column("result_summary")]
        public string? ResultSummary { get; set; }

        [Column("result_details")]
        public string? ResultDetails { get; set; }

        [Column("is_abnormal")]
        public bool IsAbnormal { get; set; }

        [Column("is_reviewed")]
        public bool IsReviewed { get; set; }

        [Column("reviewed_at")]
        public DateTime? ReviewedAt { get; set; }

        [Column("doctor_comment")]
        public string? DoctorComment { get; set; }

        [Column("result_date")]
        public DateTime ResultDate { get; set; } = DateTime.Now;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}

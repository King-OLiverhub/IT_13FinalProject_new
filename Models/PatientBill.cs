using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_13FinalProject.Models
{
    [Table("patient_bill")]
    public class PatientBill
    {
        [Key]
        [Column("id")]
        public int BillId { get; set; }

        [Column("patient_id")]
        public int? PatientId { get; set; }

        [Required]
        [Column("patient_name")]
        public string PatientName { get; set; } = string.Empty;

        [Column("patient_email")]
        public string? PatientEmail { get; set; }

        [Required]
        [Column("assessment_status")]
        public string AssessmentStatus { get; set; } = "Pending";

        [Column("date_of_visit")]
        public DateTime DateOfVisit { get; set; } = DateTime.Now;

        [Column("total_amount")]
        public decimal TotalAmount { get; set; } = 0;

        [Column("insurance_coverage")]
        public decimal InsuranceCoverage { get; set; } = 0;

        [Column("patient_responsibility")]
        public decimal PatientResponsibility { get; set; } = 0;

        [Column("payment_status")]
        public string PaymentStatus { get; set; } = "Unpaid";

        [Column("payment_method")]
        public string? PaymentMethod { get; set; }

        [Column("payment_date")]
        public DateTime? PaymentDate { get; set; }

        [Column("insurance_provider")]
        public string? InsuranceProvider { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("is_archived")]
        public bool IsArchived { get; set; } = false;

        [Column("archived_at")]
        public DateTime? ArchivedAt { get; set; }

        // Navigation property
        public virtual Patient? Patient { get; set; }

        // Computed properties
        [NotMapped]
        public string StatusKey
        {
            get
            {
                if (IsArchived) return "archived";
                if (PaymentStatus == "Paid") return "paid";
                if (PaymentStatus == "Partial") return "partial";
                return AssessmentStatus.ToLower().Replace(" ", "-");
            }
        }

        [NotMapped]
        public string DisplayStatus
        {
            get
            {
                if (IsArchived) return "Archived";
                if (PaymentStatus == "Paid") return "Paid";
                if (PaymentStatus == "Partial") return "Partial Payment";
                return AssessmentStatus;
            }
        }
    }
}

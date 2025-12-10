using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_13FinalProject.Models
{
    [Table("patients")]
    public class Patient
    {
        [Key]
        [Column("patient_id")]
        public int PatientId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(10)]
        [Column("gender")]
        public string? Gender { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(20)]
        [Column("phone")]
        public string? Phone { get; set; }

        [StringLength(100)]
        [Column("email")]
        public string? Email { get; set; }

        [Column("address", TypeName = "text")]
        public string? Address { get; set; }

        [StringLength(5)]
        [Column("blood_type")]
        public string? BloodType { get; set; }

        [StringLength(100)]
        [Column("emergency_contact")]
        public string? EmergencyContact { get; set; }

        [StringLength(100)]
        [Column("middle_name")]
        public string? MiddleName { get; set; }

        [StringLength(20)]
        [Column("civil_status")]
        public string? CivilStatus { get; set; }

        [StringLength(100)]
        [Column("occupation")]
        public string? Occupation { get; set; }

        [StringLength(50)]
        [Column("nationality")]
        public string? Nationality { get; set; }

        [StringLength(50)]
        [Column("religion")]
        public string? Religion { get; set; }

        [StringLength(100)]
        [Column("emergency_contact_name")]
        public string? EmergencyContactName { get; set; }

        [StringLength(50)]
        [Column("emergency_contact_relationship")]
        public string? EmergencyContactRelationship { get; set; }

        [StringLength(20)]
        [Column("emergency_contact_number")]
        public string? EmergencyContactNumber { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("is_archived")]
        public bool? IsArchived { get; set; }

        [Column("archived_at")]
        public DateTime? ArchivedAt { get; set; }
    }
}

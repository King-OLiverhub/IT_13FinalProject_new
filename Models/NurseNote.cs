using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_13FinalProject.Models
{
    [Table("nurse_notes")]
    public class NurseNote
    {
        [Key]
        [Column("nurse_note_id")]
        public int NurseNoteId { get; set; }

        [Column("patient_id")]
        public int PatientId { get; set; }

        [Column("category")]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Column("details", TypeName = "text")]
        public string Details { get; set; } = string.Empty;

        [Column("nurse_name")]
        [StringLength(100)]
        public string NurseName { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("is_archived")]
        public bool IsArchived { get; set; }

        [Column("archived_at")]
        public DateTime? ArchivedAt { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }
    }
}

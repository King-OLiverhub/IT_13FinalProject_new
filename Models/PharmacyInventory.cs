using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_13FinalProject.Models
{
    [Table("inventory")]
    public class PharmacyInventory
    {
        [Key]
        [Column("inventory_id")]
        public int InventoryId { get; set; }

        [Column("medication_id")]
        public int? MedicationId { get; set; }

        [Required]
        [StringLength(200)]
        [Column("item_name")]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(200)]
        [Column("generic_name")]
        public string? GenericName { get; set; }

        [StringLength(200)]
        [Column("brand_name")]
        public string? BrandName { get; set; }

        [StringLength(100)]
        [Column("item_type")]
        public string ItemType { get; set; } = "Medication";

        [StringLength(100)]
        [Column("category")]
        public string? Category { get; set; }

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Column("current_stock")]
        public int StockQuantity { get; set; } = 0;

        [Column("reorder_level")]
        public int ReorderLevel { get; set; } = 10;

        [Column("unit_price")]
        public decimal UnitPrice { get; set; } = 0;

        [StringLength(200)]
        [Column("supplier")]
        public string? Supplier { get; set; }

        [Column("last_restocked")]
        public DateTime? LastRestocked { get; set; }

        [Column("expiration_date")]
        public DateTime? ExpiryDate { get; set; }

        [StringLength(50)]
        [Column("unit")]
        public string Unit { get; set; } = "Piece(s)";

        [StringLength(100)]
        [Column("batch_number")]
        public string? BatchNumber { get; set; }

        [StringLength(200)]
        [Column("storage_requirements")]
        public string StorageRequirements { get; set; } = "Room temperature";

        [Column("prescription_required")]
        public bool PrescriptionRequired { get; set; } = false;

        [StringLength(100)]
        [Column("barcode")]
        public string? Barcode { get; set; }

        [Column("is_archived")]
        public bool IsArchived { get; set; } = false;

        [Column("archived_at")]
        public DateTime? ArchivedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Computed property for status
        [NotMapped]
        public string Status
        {
            get
            {
                if (IsArchived) return "Archived";
                if (ExpiryDate.HasValue && ExpiryDate.Value <= DateTime.Now.AddDays(30)) return "Expiring Soon";
                if (StockQuantity <= ReorderLevel) return "Low Stock";
                if (StockQuantity == 0) return "Out of Stock";
                return "Good";
            }
        }

        [NotMapped]
        public string StatusKey
        {
            get
            {
                if (IsArchived) return "archived";
                if (ExpiryDate.HasValue && ExpiryDate.Value <= DateTime.Now.AddDays(30)) return "expiring";
                if (StockQuantity <= ReorderLevel) return "low";
                if (StockQuantity == 0) return "critical";
                return "good";
            }
        }
    }
}

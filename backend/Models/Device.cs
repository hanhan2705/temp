using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    [Table("thiet_bi")]
    public class Device
    {
        [Key]
        [Column("id_tb")]
        public long Id { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("ten_thiet_bi")]
        public string Name { get; set; } = null!;

        [MaxLength(100)]
        [Column("loai")]
        public string? Type { get; set; }

        [Column("ngay_mua")]
        public DateTime? PurchaseDate { get; set; }

        [Column("nguyen_gia", TypeName = "decimal(15,2)")]
        public decimal? OriginalCost { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("trang_thai")]
        public string Status { get; set; } = "AVAILABLE"; // AVAILABLE, ASSIGNED, MAINTENANCE, DISPOSED

        [Column("id_nv_su_dung")]
        public long? AssignedUserId { get; set; }

        [ForeignKey("AssignedUserId")]
        public User? AssignedUser { get; set; }

        public Depreciation? Depreciation { get; set; }
    }
}

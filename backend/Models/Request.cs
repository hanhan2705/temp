using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    [Table("yeu_cau")]
    public class Request
    {
        [Key]
        [Column("id_yc")]
        public long Id { get; set; }

        [Required]
        [Column("id_nv")]
        public long UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Column("id_nv_gui")]
        public long? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User? CreatedByUser { get; set; }

        [MaxLength(500)]
        [Column("ly_do")]
        public string? Reason { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("loai_yeu_cau")]
        public string RequestType { get; set; } = null!; // ALLOCATION, RECOVERY

        [Required]
        [Column("ngay_gui")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        [Column("trang_thai_duyet")]
        public string? Status { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED, COMPLETED

        public ICollection<RequestDevice> RequestDevices { get; set; } = new List<RequestDevice>();
    }
}

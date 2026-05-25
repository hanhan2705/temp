using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    [Table("lich_su_cap_phat")]
    public class AllocationHistory
    {
        [Key]
        [Column("id_cp")]
        public long Id { get; set; }

        [Required]
        [Column("id_nv")]
        public long UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        [Column("id_tb")]
        public long DeviceId { get; set; }
        [ForeignKey("DeviceId")]
        public Device Device { get; set; } = null!;

        [Required]
        [Column("ngay_cap")]
        public DateTime AllocatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        [Column("trang_thai")]
        public string? Status { get; set; }
    }
}

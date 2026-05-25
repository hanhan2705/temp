using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    [Table("lich_su_thu_hoi")]
    public class ReturnHistory
    {
        [Key]
        [Column("id_th")]
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
        [Column("ngay_thu_hoi")]
        public DateTime ReturnedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        [Column("tinh_trang")]
        public string? Condition { get; set; }
    }
}

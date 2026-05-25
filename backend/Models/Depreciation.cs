using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    [Table("khau_hao")]
    public class Depreciation
    {
        [Key]
        [Column("id_kh")]
        public long Id { get; set; }

        [Required]
        [Column("id_tb")]
        public long DeviceId { get; set; }
        [ForeignKey("DeviceId")]
        public Device Device { get; set; } = null!;

        [Column("gia_tri_con_lai", TypeName = "decimal(15,2)")]
        public decimal? CurrentValue { get; set; }

        [MaxLength(100)]
        [Column("phuong_phap_tinh")]
        public string? Method { get; set; }

        [Column("thoi_gian_su_dung")]
        public int? UsefulLifeMonths { get; set; } = 36;

        [Column("gia_tri_thu_hoi", TypeName = "decimal(15,2)")]
        public decimal? SalvageValue { get; set; } = 0;

        [MaxLength(50)]
        [Column("ky_tinh")]
        public string? Period { get; set; } = "MONTH";
    }
}

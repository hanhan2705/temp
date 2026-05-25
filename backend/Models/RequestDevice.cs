using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    [Table("yeu_cau_thiet_bi")]
    public class RequestDevice
    {
        [Column("id_yc")]
        public long RequestId { get; set; }
        public Request Request { get; set; } = null!;

        [Column("id_tb")]
        public long DeviceId { get; set; }
        public Device Device { get; set; } = null!;

        [Required]
        [Column("so_luong")]
        public int Quantity { get; set; } = 1;

        [MaxLength(255)]
        [Column("ghi_chu")]
        public string? Note { get; set; }
    }
}

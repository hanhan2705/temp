using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    [Table("nhan_vien")]
    public class User
    {
        [Key]
        [Column("id_nv")]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("ho_ten")]
        public string FullName { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        [Column("email")]
        public string Email { get; set; } = null!;

        [Required]
        [Column("password_hash")]
        [JsonIgnore]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        [Column("vai_tro")]
        public string Role { get; set; } = null!; // IT_ADMIN, HR, EMPLOYEE

        [Required]
        [Column("trang_thai")]
        public bool Status { get; set; } = true;
    }
}

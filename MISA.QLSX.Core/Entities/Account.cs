using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    /// <summary>
    /// Đối tượng tài khoản đăng nhập
    /// </summary>
    [Table("account")]
    public class Account
    {
        /// <summary>
        /// ID tài khoản (UUID)
        /// </summary>
        [Key]
        [Column("account_id")]
        public Guid? AccountId { get; set; }

        /// <summary>
        /// Mã tài khoản
        /// </summary>
        [Column("account_code")]
        public string? AccountCode { get; set; }

        /// <summary>
        /// Tên đăng nhập
        /// </summary>
        [Column("username")]
        public string? Username { get; set; }

        /// <summary>
        /// Mật khẩu đã mã hóa
        /// </summary>
        [Column("password_hash")]
        public string? PasswordHash { get; set; }

        /// <summary>
        /// ID vai trò
        /// </summary>
        [Column("role_id")]
        public Guid? RoleId { get; set; }

        /// <summary>
        /// Trạng thái hoạt động (1 = hoạt động, 0 = khóa)
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Thời điểm đăng nhập thành công lần cuối
        /// </summary>
        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Thời điểm tạo tài khoản
        /// </summary>
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// ID người tạo tài khoản
        /// </summary>
        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// ID người cập nhật tài khoản lần cuối
        /// </summary>
        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// Thời điểm cập nhật tài khoản lần cuối
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}

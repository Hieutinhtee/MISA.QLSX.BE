using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    /// <summary>
    /// Thuc the luu metadata cua tep vat ly.
    /// </summary>
    [Table("file_resource")]
    public class FileResource
    {
        [Key]
        [Column("file_id")]
        public Guid? FileId { get; set; }

        [Column("original_name")]
        public string? OriginalName { get; set; }

        [Column("stored_name")]
        public string? StoredName { get; set; }

        [Column("relative_path")]
        public string? RelativePath { get; set; }

        [Column("mime_type")]
        public string? MimeType { get; set; }

        [Column("size_bytes")]
        public long? SizeBytes { get; set; }

        [Column("is_deleted")]
        public Guid? IsDeleted { get; set; } = Guid.Empty;

        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}

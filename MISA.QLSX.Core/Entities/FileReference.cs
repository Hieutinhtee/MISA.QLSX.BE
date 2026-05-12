using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    /// <summary>
    /// Thuc the lien ket tep voi du lieu nghiep vu.
    /// </summary>
    [Table("file_reference")]
    public class FileReference
    {
        [Key]
        [Column("file_reference_id")]
        public Guid? FileReferenceId { get; set; }

        [Column("file_id")]
        public Guid? FileId { get; set; }

        [Column("module_name")]
        public string? ModuleName { get; set; }

        [Column("entity_name")]
        public string? EntityName { get; set; }

        [Column("entity_id")]
        public Guid? EntityId { get; set; }

        [Column("purpose")]
        public string? Purpose { get; set; }

        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [NotMapped]
        public string? OriginalName { get; set; }

        [NotMapped]
        public string? MimeType { get; set; }

        [NotMapped]
        public long? SizeBytes { get; set; }
    }
}

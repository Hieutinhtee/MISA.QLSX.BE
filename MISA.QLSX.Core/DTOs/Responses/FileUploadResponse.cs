namespace MISA.QLSX.Core.DTOs.Responses
{
    /// <summary>
    /// DTO ket qua upload tep thanh cong.
    /// </summary>
    public class FileUploadResponse
    {
        /// <summary>
        /// ID tep vua upload.
        /// </summary>
        public Guid FileId { get; set; }

        /// <summary>
        /// Ten goc cua tep.
        /// </summary>
        public string? OriginalName { get; set; }

        /// <summary>
        /// Kieu noi dung MIME.
        /// </summary>
        public string? MimeType { get; set; }

        /// <summary>
        /// Dung luong tep theo byte.
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// Module nghiep vu.
        /// </summary>
        public string? ModuleName { get; set; }

        /// <summary>
        /// Ten thuc the nghiep vu.
        /// </summary>
        public string? EntityName { get; set; }

        /// <summary>
        /// ID ban ghi nghiep vu.
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Muc dich tep.
        /// </summary>
        public string? Purpose { get; set; }
    }
}

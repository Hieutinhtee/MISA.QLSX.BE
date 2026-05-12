namespace MISA.QLSX.Core.DTOs.Requests
{
    /// <summary>
    /// DTO metadata upload tep dung chung cho nhieu luong nghiep vu.
    /// </summary>
    public class FileUploadRequest
    {
        /// <summary>
        /// Ma module nghiep vu su dung tep.
        /// </summary>
        public string? ModuleName { get; set; }

        /// <summary>
        /// Ten thuc the nghiep vu.
        /// </summary>
        public string? EntityName { get; set; }

        /// <summary>
        /// ID ban ghi nghiep vu can gan tep.
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Muc dich su dung tep, vi du avatar, attachment.
        /// </summary>
        public string? Purpose { get; set; }
    }
}

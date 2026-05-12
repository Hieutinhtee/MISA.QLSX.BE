-- Migration 005: Local file storage metadata tables
-- Created: 2026-05-10

USE `misa_qlsx`;
SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

CREATE TABLE IF NOT EXISTS `file_resource` (
  `file_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột file_id: Khóa chính UUID của tệp',
  `original_name` VARCHAR(255) NOT NULL COMMENT 'Cột original_name: Tên gốc của tệp khi người dùng tải lên',
  `stored_name` VARCHAR(255) NOT NULL COMMENT 'Cột stored_name: Tên tệp lưu vật lý trên local storage',
  `relative_path` VARCHAR(500) NOT NULL COMMENT 'Cột relative_path: Đường dẫn tương đối của tệp trong thư mục uploads',
  `mime_type` VARCHAR(100) DEFAULT NULL COMMENT 'Cột mime_type: Kiểu nội dung MIME của tệp',
  `size_bytes` BIGINT NOT NULL COMMENT 'Cột size_bytes: Kích thước tệp theo byte',
  `is_deleted` CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000' COMMENT 'Cột is_deleted: Đánh dấu xóa mềm, 0 là chưa xóa, file_id là đã xóa',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tải tệp lên',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo metadata tệp',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật metadata tệp gần nhất',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật metadata tệp gần nhất',
  PRIMARY KEY (`file_id`),
  KEY `idx_file_resource_deleted` (`is_deleted`),
  KEY `idx_file_resource_created_at` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng file_resource: Lưu metadata tệp dùng chung nhiều luồng nghiệp vụ';

CREATE TABLE IF NOT EXISTS `file_reference` (
  `file_reference_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột file_reference_id: Khóa chính UUID của liên kết tệp',
  `file_id` CHAR(36) NOT NULL COMMENT 'Cột file_id: Khóa ngoại tham chiếu tệp metadata',
  `module_name` VARCHAR(100) NOT NULL COMMENT 'Cột module_name: Mã module nghiệp vụ sử dụng tệp',
  `entity_name` VARCHAR(100) NOT NULL COMMENT 'Cột entity_name: Tên thực thể nghiệp vụ',
  `entity_id` CHAR(36) NOT NULL COMMENT 'Cột entity_id: ID bản ghi nghiệp vụ được gắn tệp',
  `purpose` VARCHAR(100) DEFAULT NULL COMMENT 'Cột purpose: Mục đích sử dụng tệp trong luồng nghiệp vụ',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo liên kết tệp',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo liên kết tệp',
  PRIMARY KEY (`file_reference_id`),
  KEY `idx_file_reference_file_id` (`file_id`),
  KEY `idx_file_reference_entity` (`module_name`,`entity_name`,`entity_id`),
  CONSTRAINT `fk_file_reference_resource` FOREIGN KEY (`file_id`) REFERENCES `file_resource` (`file_id`) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng file_reference: Liên kết tệp với các bản ghi nghiệp vụ dùng chung';

SET FOREIGN_KEY_CHECKS = 1;

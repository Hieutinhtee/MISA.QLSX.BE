-- Migration 003: Approval Workflow + Department Upgrade
-- Created: 2026-05-02

USE `misa_qlsx`;
SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- =============================================
-- 1. Bảng approval_request (yêu cầu phê duyệt)
-- =============================================
DROP TABLE IF EXISTS `approval_step`;
DROP TABLE IF EXISTS `approval_request`;
DROP TABLE IF EXISTS `department_member_history`;
DROP TABLE IF EXISTS `department_manager_history`;

CREATE TABLE `approval_request` (
  `approval_request_id` CHAR(36) NOT NULL DEFAULT (UUID()),
  `request_code` VARCHAR(30) NOT NULL COMMENT 'Mã yêu cầu tự sinh',
  `request_type` ENUM(
    'department_member_transfer',
    'department_manager_change',
    'contract_change',
    'leave_request'
  ) NOT NULL COMMENT 'Loại yêu cầu phê duyệt',
  `title` VARCHAR(255) NOT NULL COMMENT 'Tiêu đề yêu cầu',
  `description` TEXT DEFAULT NULL COMMENT 'Mô tả chi tiết',
  `payload` JSON NOT NULL COMMENT 'Dữ liệu thay đổi cần áp dụng khi duyệt',
  `effective_date` DATE DEFAULT NULL COMMENT 'Ngày có hiệu lực của thay đổi',
  `status` ENUM('pending','approved','rejected','cancelled') NOT NULL DEFAULT 'pending',
  `current_step` INT NOT NULL DEFAULT 1,
  `total_steps` INT NOT NULL DEFAULT 1,
  `is_deleted` CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
  `created_by` CHAR(36) NOT NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_by` CHAR(36) DEFAULT NULL,
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`approval_request_id`),
  UNIQUE KEY `uk_request_code` (`request_code`),
  KEY `idx_request_type` (`request_type`),
  KEY `idx_request_status` (`status`),
  KEY `idx_request_created_by` (`created_by`),
  KEY `idx_request_deleted` (`is_deleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Bảng approval_request: Lưu yêu cầu phê duyệt chung cho mọi quy trình';

-- =============================================
-- 2. Bảng approval_step (bước phê duyệt)
-- =============================================
CREATE TABLE `approval_step` (
  `approval_step_id` CHAR(36) NOT NULL DEFAULT (UUID()),
  `approval_request_id` CHAR(36) NOT NULL,
  `step_order` INT NOT NULL COMMENT 'Thứ tự bước (1, 2, ...)',
  `approver_role` VARCHAR(20) NOT NULL COMMENT 'Role cần duyệt: ADMIN, HR, MANAGER',
  `approver_id` CHAR(36) DEFAULT NULL COMMENT 'Người duyệt cụ thể (null = bất kỳ ai có role)',
  `status` ENUM('pending','approved','rejected') NOT NULL DEFAULT 'pending',
  `comment` TEXT DEFAULT NULL,
  `acted_at` DATETIME DEFAULT NULL,
  `acted_by` CHAR(36) DEFAULT NULL,
  PRIMARY KEY (`approval_step_id`),
  KEY `idx_step_request` (`approval_request_id`),
  KEY `idx_step_role` (`approver_role`),
  KEY `idx_step_status` (`status`),
  CONSTRAINT `fk_step_request` FOREIGN KEY (`approval_request_id`)
    REFERENCES `approval_request` (`approval_request_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Bảng approval_step: Lưu từng bước phê duyệt của yêu cầu';

-- =============================================
-- 3. Alter department: thêm is_active + inactive_effective_date
-- =============================================
ALTER TABLE `department`
  ADD COLUMN `is_active` TINYINT(1) NOT NULL DEFAULT 1
    COMMENT 'Trạng thái sử dụng: 1=đang sử dụng, 0=ngừng' AFTER `manager_employee_id`,
  ADD COLUMN `inactive_effective_date` DATE DEFAULT NULL
    COMMENT 'Ngày áp dụng ngừng sử dụng' AFTER `is_active`;

-- =============================================
-- 4. Bảng department_member_history
-- =============================================
CREATE TABLE `department_member_history` (
  `history_id` CHAR(36) NOT NULL DEFAULT (UUID()),
  `employee_id` CHAR(36) NOT NULL,
  `department_id` CHAR(36) NOT NULL,
  `action` ENUM('join','leave','transfer') NOT NULL,
  `effective_date` DATE NOT NULL,
  `approval_request_id` CHAR(36) DEFAULT NULL,
  `note` TEXT DEFAULT NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `created_by` CHAR(36) DEFAULT NULL,
  PRIMARY KEY (`history_id`),
  KEY `idx_member_hist_emp` (`employee_id`),
  KEY `idx_member_hist_dept` (`department_id`),
  KEY `idx_member_hist_effective` (`effective_date`),
  CONSTRAINT `fk_member_hist_emp` FOREIGN KEY (`employee_id`)
    REFERENCES `employee` (`employee_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_member_hist_dept` FOREIGN KEY (`department_id`)
    REFERENCES `department` (`department_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Bảng department_member_history: Lịch sử thuyên chuyển thành viên phòng ban';

-- =============================================
-- 5. Bảng department_manager_history
-- =============================================
CREATE TABLE `department_manager_history` (
  `history_id` CHAR(36) NOT NULL DEFAULT (UUID()),
  `department_id` CHAR(36) NOT NULL,
  `manager_employee_id` CHAR(36) NOT NULL,
  `effective_date` DATE NOT NULL,
  `end_date` DATE DEFAULT NULL,
  `approval_request_id` CHAR(36) DEFAULT NULL,
  `note` TEXT DEFAULT NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `created_by` CHAR(36) DEFAULT NULL,
  PRIMARY KEY (`history_id`),
  KEY `idx_mgr_hist_dept` (`department_id`),
  KEY `idx_mgr_hist_effective` (`effective_date`),
  CONSTRAINT `fk_mgr_hist_dept` FOREIGN KEY (`department_id`)
    REFERENCES `department` (`department_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_mgr_hist_emp` FOREIGN KEY (`manager_employee_id`)
    REFERENCES `employee` (`employee_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Bảng department_manager_history: Lịch sử bổ nhiệm trưởng phòng';

SET FOREIGN_KEY_CHECKS = 1;

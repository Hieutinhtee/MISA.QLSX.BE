CREATE DATABASE IF NOT EXISTS `misa_qlsx` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `misa_qlsx`;

SET NAMES utf8mb4;
SET SQL_MODE = 'NO_AUTO_VALUE_ON_ZERO';
SET FOREIGN_KEY_CHECKS = 0;
START TRANSACTION;

DROP TABLE IF EXISTS `contract_history`;
DROP TABLE IF EXISTS `employee_tax_profile`;
DROP TABLE IF EXISTS `deduction_policy`;
DROP TABLE IF EXISTS `tax_bracket`;
DROP TABLE IF EXISTS `salary_policy`;
DROP TABLE IF EXISTS `payroll_snapshot`;
DROP TABLE IF EXISTS `payroll_item`;
DROP TABLE IF EXISTS `payroll`;
DROP TABLE IF EXISTS `salary_period`;
DROP TABLE IF EXISTS `leave_request`;
DROP TABLE IF EXISTS `suggestion`;
DROP TABLE IF EXISTS `evaluation`;
DROP TABLE IF EXISTS `business_trip`;
DROP TABLE IF EXISTS `attendance`;
DROP TABLE IF EXISTS `employee`;
DROP TABLE IF EXISTS `department`;
DROP TABLE IF EXISTS `account`;
DROP TABLE IF EXISTS `role`;
DROP TABLE IF EXISTS `contract_allowance`;
DROP TABLE IF EXISTS `contract`;
DROP TABLE IF EXISTS `contract_template`;
DROP TABLE IF EXISTS `allowance`;
DROP TABLE IF EXISTS `position`;
DROP TABLE IF EXISTS `degree`;
DROP TABLE IF EXISTS `shift`;



CREATE TABLE `shift` (
  `shift_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột shift_id: Khóa chính UUID của ca làm việc',
  `shift_code` VARCHAR(20) NOT NULL COMMENT 'Cột shift_code: Mã ca làm việc duy nhất để tra cứu',
  `shift_name` VARCHAR(100) NOT NULL COMMENT 'Cột shift_name: Tên hiển thị của ca làm việc',
  `start_time` TIME NOT NULL COMMENT 'Cột start_time: Thời gian bắt đầu ca',
  `break_start_time` TIME DEFAULT NULL COMMENT 'Cột break_start_time: Thời gian bắt đầu nghỉ giữa ca',
  `end_time` TIME NOT NULL COMMENT 'Cột end_time: Thời gian kết thúc ca',
  `break_end_time` TIME DEFAULT NULL COMMENT 'Cột break_end_time: Thời gian kết thúc nghỉ giữa ca',
  `working_hours` DECIMAL(5,2) NOT NULL COMMENT 'Cột working_hours: Tổng số giờ làm việc được tính công',
  `break_hours` DECIMAL(5,2) NOT NULL DEFAULT 0.00 COMMENT 'Cột break_hours: Tổng số giờ nghỉ của ca',
  `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Cột is_active: Trạng thái hoạt động, 1 là hoạt động, 0 là ngừng',
  `description` VARCHAR(255) DEFAULT NULL COMMENT 'Cột description: Ghi chú mô tả thêm cho ca làm việc',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo dữ liệu',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo bản ghi',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất',
  PRIMARY KEY (`shift_id`),
  UNIQUE KEY `uk_shift_code` (`shift_code`),
  KEY `idx_shift_name` (`shift_name`),
  KEY `idx_shift_active` (`is_active`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng shift: Lưu danh mục ca làm việc trong hệ thống';


CREATE TABLE `degree` (
  `degree_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột degree_id: Khóa chính UUID của bằng cấp',
  `degree_code` VARCHAR(20) NOT NULL COMMENT 'Cột degree_code: Mã bằng cấp duy nhất để tra cứu',
  `degree_name` VARCHAR(100) NOT NULL COMMENT 'Cột degree_name: Tên bằng cấp',
  `description` TEXT DEFAULT NULL COMMENT 'Cột description: Ghi chú mô tả thêm cho bằng cấp',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo bản ghi',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo dữ liệu bằng cấp',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của bằng cấp',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của bằng cấp',
  PRIMARY KEY (`degree_id`),
  UNIQUE KEY `uk_degree_code` (`degree_code`),
  UNIQUE KEY `uk_degree_name` (`degree_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng degree: Lưu danh mục bằng cấp chuyên môn';

CREATE TABLE `position` (
  `position_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột position_id: Khóa chính UUID của chức vụ',
  `position_code` VARCHAR(20) NOT NULL COMMENT 'Cột position_code: Mã chức vụ duy nhất để tra cứu',
  `position_name` VARCHAR(100) NOT NULL COMMENT 'Cột position_name: Tên chức vụ',
  `description` TEXT DEFAULT NULL COMMENT 'Cột description: Ghi chú mô tả thêm cho chức vụ',
  `allowance` DECIMAL(15,2) NOT NULL DEFAULT 0 COMMENT 'Cột allowance: Mức phụ cấp chức vụ',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo bản ghi',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo dữ liệu chức vụ',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của chức vụ',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của chức vụ',
  PRIMARY KEY (`position_id`),
  UNIQUE KEY `uk_position_code` (`position_code`),
  UNIQUE KEY `uk_position_name` (`position_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng position: Lưu danh mục chức vụ công việc';

CREATE TABLE `allowance` (
  `allowance_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột allowance_id: Khóa chính UUID của phụ cấp',
  `allowance_code` VARCHAR(50) NOT NULL COMMENT 'Cột allowance_code: Mã phụ cấp để tra cứu theo phiên bản',
  `allowance_name` VARCHAR(255) NOT NULL COMMENT 'Cột allowance_name: Tên phụ cấp',
  `calculation_type` ENUM('FIXED','PERCENT') NOT NULL COMMENT 'Cột calculation_type: Kiểu tính phụ cấp, FIXED là số tiền cố định, PERCENT là phần trăm',
  `amount` DECIMAL(15,2) DEFAULT NULL COMMENT 'Cột amount: Giá trị tiền phụ cấp khi calculation_type là FIXED',
  `percent` DECIMAL(6,2) DEFAULT NULL COMMENT 'Cột percent: Giá trị phần trăm phụ cấp khi calculation_type là PERCENT',
  `version` INT NOT NULL DEFAULT 1 COMMENT 'Cột version: Phiên bản cấu hình phụ cấp',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo bản ghi phụ cấp',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo dữ liệu phụ cấp',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của phụ cấp',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của phụ cấp',
  PRIMARY KEY (`allowance_id`),
  UNIQUE KEY `uk_allowance_code_version` (`allowance_code`,`version`),
  KEY `idx_allowance_name` (`allowance_name`),
  KEY `idx_allowance_code` (`allowance_code`),
  KEY `idx_allowance_calculation_type` (`calculation_type`),
  KEY `idx_allowance_version` (`version`),
  CONSTRAINT `chk_allowance_value_by_type` CHECK (
    (`calculation_type` = 'FIXED' AND `amount` IS NOT NULL AND `percent` IS NULL)
    OR
    (`calculation_type` = 'PERCENT' AND `percent` IS NOT NULL AND `amount` IS NULL)
  )
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng allowance: Lưu danh mục phụ cấp và cách tính phụ cấp';

CREATE TABLE `contract_template` (
  `template_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột template_id: Khóa chính UUID của mẫu hợp đồng',
  `template_code` VARCHAR(50) NOT NULL COMMENT 'Cột template_code: Mã mẫu hợp đồng duy nhất để tra cứu',
  `template_name` VARCHAR(255) NOT NULL COMMENT 'Cột template_name: Tên mẫu hợp đồng',
  `contract_type` ENUM('Thử việc','Có thời hạn','Không thời hạn') NOT NULL COMMENT 'Cột contract_type: Loại hợp đồng của mẫu, gồm thử việc, có thời hạn hoặc không thời hạn',
  `content` TEXT DEFAULT NULL COMMENT 'Cột content: Nội dung mẫu hợp đồng dạng HTML hoặc văn bản',
  `version` INT NOT NULL DEFAULT 1 COMMENT 'Cột version: Phiên bản của mẫu hợp đồng',
  `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Cột is_active: Trạng thái hoạt động của mẫu, 1 là hoạt động, 0 là ngừng',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo mẫu hợp đồng',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo mẫu hợp đồng',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của mẫu hợp đồng',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của mẫu hợp đồng',
  PRIMARY KEY (`template_id`),
  UNIQUE KEY `uk_contract_template_code_version` (`template_code`,`version`),
  KEY `idx_contract_template_code` (`template_code`),
  KEY `idx_contract_template_name` (`template_name`),
  KEY `idx_contract_template_type` (`contract_type`),
  KEY `idx_contract_template_active` (`is_active`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng contract_template: Lưu mẫu hợp đồng để tái sử dụng';

CREATE TABLE `contract` (
  `contract_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột contract_id: Khóa chính UUID của hợp đồng',
  `contract_code` VARCHAR(50) NOT NULL COMMENT 'Cột contract_code: Mã hợp đồng duy nhất để tra cứu',
  `template_id` CHAR(36) NOT NULL COMMENT 'Cột template_id: Khóa ngoại tham chiếu mẫu hợp đồng',
  `company_representative_id` CHAR(36) NOT NULL COMMENT 'Cột company_representative_id: UUID nhân viên đại diện công ty ký hợp đồng',
  `company_signer_title` VARCHAR(255) NOT NULL COMMENT 'Cột company_signer_title: Chức danh người đại diện công ty ký',
  `employee_id` CHAR(36) NOT NULL COMMENT 'Cột employee_id: Khóa ngoại tham chiếu nhân viên được ký hợp đồng',
  `effective_date` DATE NOT NULL COMMENT 'Cột effective_date: Ngày hợp đồng có hiệu lực',
  `term_months` INT DEFAULT NULL COMMENT 'Cột term_months: Thời hạn hợp đồng tính theo tháng, dùng cho hợp đồng có thời hạn hoặc thử việc',
  `base_salary` DECIMAL(15,2) NOT NULL COMMENT 'Cột base_salary: Lương cơ bản theo hợp đồng',
  `insurance_salary` DECIMAL(15,2) NOT NULL COMMENT 'Cột insurance_salary: Mức lương đóng bảo hiểm',
  `salary_ratio` DECIMAL(6,2) NOT NULL COMMENT 'Cột salary_ratio: Tỷ lệ hưởng lương của nhân viên',
  `summary` TEXT DEFAULT NULL COMMENT 'Cột summary: Trích yếu nội dung hợp đồng',
  `attachment_link` TEXT DEFAULT NULL COMMENT 'Cột attachment_link: Đường dẫn tệp đính kèm hợp đồng',
  `is_signed` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Cột is_signed: Trạng thái ký hợp đồng, 0 chưa ký, 1 đã ký',
  `signed_at` DATETIME DEFAULT NULL COMMENT 'Cột signed_at: Thời điểm ký hợp đồng',
  `contract_status` ENUM('draft','signed','active','expired','terminated') NOT NULL DEFAULT 'draft' COMMENT 'Cột contract_status: Trạng thái hợp đồng: nháp, đã ký, hiệu lực, hết hạn, chấm dứt',
  `end_date` DATE DEFAULT NULL COMMENT 'Cột end_date: Ngày kết thúc hợp đồng',
  `terminated_at` DATETIME DEFAULT NULL COMMENT 'Cột terminated_at: Thời điểm chấm dứt hợp đồng',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo hợp đồng',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo hợp đồng',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của hợp đồng',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của hợp đồng',
  PRIMARY KEY (`contract_id`),
  UNIQUE KEY `uk_contract_code` (`contract_code`),
  UNIQUE KEY `uk_contract_id_employee` (`contract_id`,`employee_id`),
  KEY `idx_contract_template_id` (`template_id`),
  KEY `idx_contract_employee_id` (`employee_id`),
  KEY `idx_contract_company_representative_id` (`company_representative_id`),
  KEY `idx_contract_effective_date` (`effective_date`),
  KEY `idx_contract_is_signed` (`is_signed`),
  KEY `idx_contract_status` (`contract_status`),
  KEY `idx_contract_effective_end` (`employee_id`,`effective_date`,`end_date`),
  CONSTRAINT `fk_contract_template` FOREIGN KEY (`template_id`) REFERENCES `contract_template` (`template_id`) ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng contract: Lưu thông tin hợp đồng lao động của nhân viên';

CREATE TABLE `contract_allowance` (
  `contract_allowance_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột contract_allowance_id: Khóa chính UUID của liên kết hợp đồng phụ cấp',
  `contract_id` CHAR(36) NOT NULL COMMENT 'Cột contract_id: Khóa ngoại tham chiếu hợp đồng',
  `allowance_id` CHAR(36) NOT NULL COMMENT 'Cột allowance_id: Khóa ngoại tham chiếu phụ cấp',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo liên kết hợp đồng và phụ cấp',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo dữ liệu liên kết phụ cấp',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của liên kết phụ cấp',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của liên kết phụ cấp',
  PRIMARY KEY (`contract_allowance_id`),
  UNIQUE KEY `uk_contract_allowance_mapping` (`contract_id`,`allowance_id`),
  KEY `idx_contract_allowance_contract_id` (`contract_id`),
  KEY `idx_contract_allowance_allowance_id` (`allowance_id`),
  CONSTRAINT `fk_contract_allowance_contract` FOREIGN KEY (`contract_id`) REFERENCES `contract` (`contract_id`) ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT `fk_contract_allowance_allowance` FOREIGN KEY (`allowance_id`) REFERENCES `allowance` (`allowance_id`) ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng contract_allowance: Lưu danh sách phụ cấp áp dụng cho từng hợp đồng';

CREATE TABLE `role` (
  `role_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột role_id: Khóa chính UUID của vai trò hệ thống',
  `role_code` VARCHAR(20) NOT NULL COMMENT 'Cột role_code: Mã vai trò duy nhất để tra cứu',
  `role_name` VARCHAR(100) NOT NULL COMMENT 'Cột role_name: Tên vai trò',
  `description` VARCHAR(255) DEFAULT NULL COMMENT 'Cột description: Ghi chú mô tả thêm cho vai trò',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo vai trò',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo dữ liệu vai trò',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của vai trò',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của vai trò',
  PRIMARY KEY (`role_id`),
  UNIQUE KEY `uk_role_code` (`role_code`),
  UNIQUE KEY `uk_role_name` (`role_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng role: Lưu danh mục vai trò phân quyền';

CREATE TABLE `account` (
  `account_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột account_id: Khóa chính UUID của tài khoản',
  `account_code` VARCHAR(20) NOT NULL COMMENT 'Cột account_code: Mã tài khoản duy nhất để tra cứu',
  `username` VARCHAR(50) NOT NULL COMMENT 'Cột username: Tên đăng nhập',
  `password_hash` VARCHAR(255) NOT NULL COMMENT 'Cột password_hash: Chuỗi mật khẩu đã mã hóa',
  `role_id` CHAR(36) NOT NULL COMMENT 'Cột role_id: Khóa ngoại tham chiếu vai trò',
  `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Cột is_active: Trạng thái tài khoản, 1 hoạt động, 0 bị khóa',
  `last_login_at` DATETIME DEFAULT NULL COMMENT 'Cột last_login_at: Thời điểm đăng nhập thành công gần nhất',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo tài khoản',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo dữ liệu tài khoản',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của tài khoản',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của tài khoản',
  PRIMARY KEY (`account_id`),
  UNIQUE KEY `uk_account_code` (`account_code`),
  UNIQUE KEY `uk_account_username` (`username`),
  KEY `idx_account_role_id` (`role_id`),
  KEY `idx_account_active` (`is_active`),
  CONSTRAINT `fk_account_role` FOREIGN KEY (`role_id`) REFERENCES `role` (`role_id`) ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng account: Lưu thông tin tài khoản đăng nhập';

CREATE TABLE `department` (
  `department_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột department_id: Khóa chính UUID của phòng ban',
  `department_code` VARCHAR(20) NOT NULL COMMENT 'Cột department_code: Mã phòng ban duy nhất để tra cứu',
  `department_name` VARCHAR(100) NOT NULL COMMENT 'Cột department_name: Tên phòng ban',
  `description` TEXT DEFAULT NULL COMMENT 'Cột description: Ghi chú mô tả thêm cho phòng ban',
  `manager_employee_id` CHAR(36) DEFAULT NULL COMMENT 'Cột manager_employee_id: UUID nhân viên giữ vai trò trưởng phòng',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo phòng ban',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo dữ liệu phòng ban',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của phòng ban',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của phòng ban',
  PRIMARY KEY (`department_id`),
  UNIQUE KEY `uk_department_code` (`department_code`),
  UNIQUE KEY `uk_department_name` (`department_name`),
  KEY `idx_department_manager_employee_id` (`manager_employee_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng department: Lưu danh mục phòng ban trong tổ chức';

CREATE TABLE `employee` (
  `employee_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột employee_id: Khóa chính UUID của nhân viên',
  `employee_code` VARCHAR(20) NOT NULL COMMENT 'Cột employee_code: Mã nhân viên duy nhất để tra cứu',
  `full_name` VARCHAR(120) NOT NULL COMMENT 'Cột full_name: Họ và tên đầy đủ của nhân viên',
  `gender` VARCHAR(10) NOT NULL COMMENT 'Cột gender: Giới tính của nhân viên',
  `date_of_birth` DATE NOT NULL COMMENT 'Cột date_of_birth: Ngày sinh của nhân viên',
  `address` VARCHAR(255) NOT NULL COMMENT 'Cột address: Địa chỉ cư trú hiện tại',
  `phone_number` VARCHAR(20) NOT NULL COMMENT 'Cột phone_number: Số điện thoại liên hệ',
  `email` VARCHAR(100) NOT NULL COMMENT 'Cột email: Địa chỉ thư điện tử của nhân viên',
  `join_date` DATE NOT NULL COMMENT 'Cột join_date: Ngày bắt đầu làm việc chính thức',
  `department_id` CHAR(36) DEFAULT NULL COMMENT 'Cột department_id: Khóa ngoại tham chiếu phòng ban',
  `shift_id` CHAR(36) DEFAULT NULL COMMENT 'Cột shift_id: Khóa ngoại tham chiếu ca làm việc',
  `national_id` VARCHAR(20) NOT NULL COMMENT 'Cột national_id: Số định danh cá nhân hoặc CCCD',
  `degree_id` CHAR(36) NOT NULL COMMENT 'Cột degree_id: Khóa ngoại tham chiếu bằng cấp',
  `contract_id` CHAR(36) DEFAULT NULL COMMENT 'Cột contract_id: Khóa ngoại tham chiếu hợp đồng lao động hiện tại',
  `position_id` CHAR(36) DEFAULT NULL COMMENT 'Cột position_id: Khóa ngoại tham chiếu chức vụ',
  `account_id` CHAR(36) DEFAULT NULL COMMENT 'Cột account_id: Khóa ngoại tham chiếu tài khoản đăng nhập',
  `avatar_url` VARCHAR(255) NOT NULL DEFAULT 'profile.jpg' COMMENT 'Cột avatar_url: Đường dẫn hoặc tên tệp ảnh đại diện',
  `place_of_birth` VARCHAR(255) DEFAULT NULL COMMENT 'Cột place_of_birth: Nơi sinh',
  `hometown` VARCHAR(255) DEFAULT NULL COMMENT 'Cột hometown: Quê quán',
  `ethnic` VARCHAR(50) DEFAULT 'Kinh' COMMENT 'Cột ethnic: Dân tộc',
  `religion` VARCHAR(50) DEFAULT 'Không' COMMENT 'Cột religion: Tôn giáo',
  `nationality` VARCHAR(50) DEFAULT 'Việt Nam' COMMENT 'Cột nationality: Quốc tịch',
  `marital_status` VARCHAR(50) DEFAULT 'Độc thân' COMMENT 'Cột marital_status: Tình trạng hôn nhân',
  `personal_email` VARCHAR(100) DEFAULT NULL COMMENT 'Cột personal_email: Email cá nhân',
  `facebook_url` VARCHAR(255) DEFAULT NULL COMMENT 'Cột facebook_url: Link Facebook cá nhân',
  `zalo_number` VARCHAR(20) DEFAULT NULL COMMENT 'Cột zalo_number: Số điện thoại Zalo',
  `temporary_address` VARCHAR(255) DEFAULT NULL COMMENT 'Cột temporary_address: Địa chỉ tạm trú',
  `bank_account_number` VARCHAR(50) DEFAULT NULL COMMENT 'Cột bank_account_number: Số tài khoản ngân hàng',
  `bank_name` VARCHAR(100) DEFAULT NULL COMMENT 'Cột bank_name: Tên ngân hàng',
  `bank_branch` VARCHAR(100) DEFAULT NULL COMMENT 'Cột bank_branch: Chi nhánh ngân hàng',
  `emergency_contact_name` VARCHAR(120) DEFAULT NULL COMMENT 'Cột emergency_contact_name: Tên người liên hệ khẩn cấp',
  `emergency_contact_phone` VARCHAR(20) DEFAULT NULL COMMENT 'Cột emergency_contact_phone: Số điện thoại người liên hệ khẩn cấp',
  `emergency_contact_relationship` VARCHAR(50) DEFAULT NULL COMMENT 'Cột emergency_contact_relationship: Quan hệ với người liên hệ khẩn cấp',
  `height` DECIMAL(5,2) DEFAULT NULL COMMENT 'Cột height: Chiều cao (cm)',
  `weight` DECIMAL(5,2) DEFAULT NULL COMMENT 'Cột weight: Cân nặng (kg)',
  `blood_group` VARCHAR(10) DEFAULT NULL COMMENT 'Cột blood_group: Nhóm máu',
  `health_status` VARCHAR(255) DEFAULT NULL COMMENT 'Cột health_status: Tình trạng sức khỏe',
  `social_insurance_number` VARCHAR(20) DEFAULT NULL COMMENT 'Cột social_insurance_number: Số sổ bảo hiểm xã hội',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo hồ sơ nhân viên',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo dữ liệu nhân viên',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của nhân viên',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của nhân viên',
  PRIMARY KEY (`employee_id`),
  UNIQUE KEY `uk_employee_code` (`employee_code`),
  UNIQUE KEY `uk_employee_national_id` (`national_id`),
  UNIQUE KEY `uk_employee_account_id` (`account_id`),
  UNIQUE KEY `uk_employee_email` (`email`),
  KEY `idx_employee_department_id` (`department_id`),
  KEY `idx_employee_shift_id` (`shift_id`),
  KEY `idx_employee_degree_id` (`degree_id`),
  KEY `idx_employee_contract_id` (`contract_id`),
  KEY `idx_employee_contract_employee` (`contract_id`,`employee_id`),
  KEY `idx_employee_position_id` (`position_id`),
  KEY `idx_employee_phone_number` (`phone_number`),
  CONSTRAINT `fk_employee_department` FOREIGN KEY (`department_id`) REFERENCES `department` (`department_id`) ON UPDATE CASCADE ON DELETE SET NULL,
  CONSTRAINT `fk_employee_shift` FOREIGN KEY (`shift_id`) REFERENCES `shift` (`shift_id`) ON UPDATE CASCADE ON DELETE SET NULL,
  CONSTRAINT `fk_employee_degree` FOREIGN KEY (`degree_id`) REFERENCES `degree` (`degree_id`) ON UPDATE CASCADE,
  CONSTRAINT `fk_employee_contract` FOREIGN KEY (`contract_id`,`employee_id`) REFERENCES `contract` (`contract_id`,`employee_id`) ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT `fk_employee_position` FOREIGN KEY (`position_id`) REFERENCES `position` (`position_id`) ON UPDATE CASCADE ON DELETE SET NULL,
  CONSTRAINT `fk_employee_account` FOREIGN KEY (`account_id`) REFERENCES `account` (`account_id`) ON UPDATE CASCADE ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng employee: Lưu hồ sơ thông tin nhân viên';

ALTER TABLE `department`
  ADD CONSTRAINT `fk_department_manager_employee`
  FOREIGN KEY (`manager_employee_id`) REFERENCES `employee` (`employee_id`) ON UPDATE CASCADE ON DELETE SET NULL;

ALTER TABLE `contract`
  ADD CONSTRAINT `fk_contract_employee`
  FOREIGN KEY (`employee_id`) REFERENCES `employee` (`employee_id`) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE `contract`
  ADD CONSTRAINT `fk_contract_company_representative`
  FOREIGN KEY (`company_representative_id`) REFERENCES `employee` (`employee_id`) ON UPDATE CASCADE;

CREATE TABLE `attendance` (
  `attendance_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột attendance_id: Khóa chính UUID của bản ghi chấm công',
  `attendance_code` VARCHAR(20) NOT NULL COMMENT 'Cột attendance_code: Mã chấm công duy nhất để tra cứu',
  `employee_id` CHAR(36) NOT NULL COMMENT 'Cột employee_id: Khóa ngoại tham chiếu nhân viên',
  `shift_id` CHAR(36) DEFAULT NULL COMMENT 'Cột shift_id: Khóa ngoại tham chiếu ca làm việc trong ngày',
  `attendance_date` DATE NOT NULL COMMENT 'Cột attendance_date: Ngày chấm công',
  `working_hours` DECIMAL(5,2) NOT NULL DEFAULT 0 COMMENT 'Cột working_hours: Số giờ làm việc thực tế',
  `overtime_hours` DECIMAL(5,2) NOT NULL DEFAULT 0 COMMENT 'Cột overtime_hours: Số giờ tăng ca',
  `penalty_amount` DECIMAL(15,2) NOT NULL DEFAULT 0 COMMENT 'Cột penalty_amount: Số tiền phạt bị trừ',
  `bonus_amount` DECIMAL(15,2) NOT NULL DEFAULT 0 COMMENT 'Cột bonus_amount: Số tiền thưởng được cộng',
  `net_income` DECIMAL(15,2) NOT NULL DEFAULT 0 COMMENT 'Cột net_income: Thu nhập thực nhận theo chấm công',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo bản ghi chấm công',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo dữ liệu chấm công',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của chấm công',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của chấm công',
  PRIMARY KEY (`attendance_id`),
  UNIQUE KEY `uk_attendance_code` (`attendance_code`),
  UNIQUE KEY `uk_attendance_employee_date_shift` (`employee_id`,`attendance_date`,`shift_id`),
  KEY `idx_attendance_date` (`attendance_date`),
  KEY `idx_attendance_shift_id` (`shift_id`),
  CONSTRAINT `fk_attendance_employee` FOREIGN KEY (`employee_id`) REFERENCES `employee` (`employee_id`) ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT `fk_attendance_shift` FOREIGN KEY (`shift_id`) REFERENCES `shift` (`shift_id`) ON UPDATE CASCADE ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng attendance: Lưu dữ liệu chấm công và thu nhập theo ngày';

CREATE TABLE `business_trip` (
  `business_trip_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột business_trip_id: Khóa chính UUID của công tác',
  `business_trip_code` VARCHAR(20) NOT NULL COMMENT 'Cột business_trip_code: Mã công tác duy nhất để tra cứu',
  `employee_id` CHAR(36) NOT NULL COMMENT 'Cột employee_id: Khóa ngoại tham chiếu nhân viên',
  `start_date` DATE NOT NULL COMMENT 'Cột start_date: Ngày bắt đầu công tác',
  `end_date` DATE NOT NULL COMMENT 'Cột end_date: Ngày kết thúc công tác',
  `location` VARCHAR(200) NOT NULL COMMENT 'Cột location: Địa điểm công tác',
  `purpose` TEXT NOT NULL COMMENT 'Cột purpose: Mục đích hoặc nội dung công tác',
  `support_amount` DECIMAL(15,2) NOT NULL DEFAULT 0 COMMENT 'Cột support_amount: Mức hỗ trợ chi phí công tác',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo bản ghi công tác',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo dữ liệu công tác',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của công tác',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của công tác',
  PRIMARY KEY (`business_trip_id`),
  UNIQUE KEY `uk_business_trip_code` (`business_trip_code`),
  KEY `idx_business_trip_employee_id` (`employee_id`),
  KEY `idx_business_trip_date_range` (`start_date`,`end_date`),
  CONSTRAINT `fk_business_trip_employee` FOREIGN KEY (`employee_id`) REFERENCES `employee` (`employee_id`) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng business_trip: Lưu thông tin đăng ký và theo dõi công tác';

CREATE TABLE `evaluation` (
  `evaluation_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột evaluation_id: Khóa chính UUID của bản ghi đánh giá',
  `evaluation_code` VARCHAR(20) NOT NULL COMMENT 'Cột evaluation_code: Mã đánh giá duy nhất để tra cứu',
  `employee_id` CHAR(36) NOT NULL COMMENT 'Cột employee_id: Khóa ngoại tham chiếu nhân viên',
  `evaluation_type` VARCHAR(50) NOT NULL COMMENT 'Cột evaluation_type: Loại đánh giá như khen thưởng hoặc vi phạm',
  `reason` TEXT NOT NULL COMMENT 'Cột reason: Lý do hoặc nội dung đánh giá',
  `amount` DECIMAL(15,2) NOT NULL COMMENT 'Cột amount: Số tiền liên quan đến đánh giá',
  `evaluation_date` DATE NOT NULL COMMENT 'Cột evaluation_date: Ngày áp dụng đánh giá',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo bản ghi đánh giá',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo dữ liệu đánh giá',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của đánh giá',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của đánh giá',
  PRIMARY KEY (`evaluation_id`),
  UNIQUE KEY `uk_evaluation_code` (`evaluation_code`),
  KEY `idx_evaluation_employee_id` (`employee_id`),
  KEY `idx_evaluation_date` (`evaluation_date`),
  KEY `idx_evaluation_type` (`evaluation_type`),
  CONSTRAINT `fk_evaluation_employee` FOREIGN KEY (`employee_id`) REFERENCES `employee` (`employee_id`) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng evaluation: Lưu thông tin khen thưởng và xử lý vi phạm';

CREATE TABLE `suggestion` (
  `suggestion_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột suggestion_id: Khóa chính UUID của kiến nghị',
  `suggestion_code` VARCHAR(20) NOT NULL COMMENT 'Cột suggestion_code: Mã kiến nghị duy nhất để tra cứu',
  `employee_id` CHAR(36) NOT NULL COMMENT 'Cột employee_id: Khóa ngoại tham chiếu nhân viên',
  `title` VARCHAR(200) NOT NULL COMMENT 'Cột title: Tiêu đề kiến nghị',
  `content` TEXT NOT NULL COMMENT 'Cột content: Nội dung chi tiết kiến nghị',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo kiến nghị',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo dữ liệu kiến nghị',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của kiến nghị',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của kiến nghị',
  PRIMARY KEY (`suggestion_id`),
  UNIQUE KEY `uk_suggestion_code` (`suggestion_code`),
  KEY `idx_suggestion_employee_id` (`employee_id`),
  KEY `idx_suggestion_created_at` (`created_at`),
  CONSTRAINT `fk_suggestion_employee` FOREIGN KEY (`employee_id`) REFERENCES `employee` (`employee_id`) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng suggestion: Lưu thông tin kiến nghị và đề xuất của nhân viên';

CREATE TABLE `leave_request` (
  `leave_request_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột leave_request_id: Khóa chính UUID của đơn nghỉ phép',
  `leave_request_code` VARCHAR(20) NOT NULL COMMENT 'Cột leave_request_code: Mã đơn nghỉ phép duy nhất để tra cứu',
  `employee_id` CHAR(36) NOT NULL COMMENT 'Cột employee_id: Khóa ngoại tham chiếu nhân viên',
  `start_date` DATE NOT NULL COMMENT 'Cột start_date: Ngày bắt đầu nghỉ phép',
  `return_date` DATE NOT NULL COMMENT 'Cột return_date: Ngày dự kiến đi làm lại',
  `reason` TEXT NOT NULL COMMENT 'Cột reason: Lý do xin nghỉ phép',
  `approval_status` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Cột approval_status: Trạng thái duyệt, 0 chờ duyệt, 1 duyệt, 2 từ chối',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo đơn nghỉ phép',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo dữ liệu đơn nghỉ phép',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của đơn nghỉ phép',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của đơn nghỉ phép',
  PRIMARY KEY (`leave_request_id`),
  UNIQUE KEY `uk_leave_request_code` (`leave_request_code`),
  KEY `idx_leave_request_employee_id` (`employee_id`),
  KEY `idx_leave_request_date_range` (`start_date`,`return_date`),
  KEY `idx_leave_request_approval_status` (`approval_status`),
  CONSTRAINT `fk_leave_request_employee` FOREIGN KEY (`employee_id`) REFERENCES `employee` (`employee_id`) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng leave_request: Lưu thông tin đơn xin nghỉ phép và trạng thái duyệt';

CREATE TABLE `salary_period` (
  `salary_period_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột salary_period_id: Khóa chính UUID của kỳ lương',
  `start_date` DATE NOT NULL COMMENT 'Cột start_date: Ngày bắt đầu kỳ lương',
  `end_date` DATE NOT NULL COMMENT 'Cột end_date: Ngày kết thúc kỳ lương',
  `status` ENUM('draft','locked','paid') NOT NULL DEFAULT 'draft' COMMENT 'Cột status: Trạng thái kỳ lương, draft là nháp, locked là khóa, paid là đã chi trả',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo kỳ lương',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo dữ liệu kỳ lương',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của kỳ lương',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của kỳ lương',
  PRIMARY KEY (`salary_period_id`),
  UNIQUE KEY `uk_salary_period_date_range` (`start_date`,`end_date`),
  KEY `idx_salary_period_status` (`status`),
  CONSTRAINT `chk_salary_period_date_range` CHECK (`start_date` <= `end_date`),
  CONSTRAINT `chk_salary_period_start_day` CHECK (DAY(`start_date`) = 1),
  CONSTRAINT `chk_salary_period_end_last_day` CHECK (`end_date` = LAST_DAY(`start_date`))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng salary_period: Lưu định nghĩa kỳ lương theo tháng';

CREATE TABLE `payroll` (
  `payroll_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột payroll_id: Khóa chính UUID của bảng lương tháng nhân viên',
  `payroll_code` VARCHAR(50) NOT NULL COMMENT 'Cột payroll_code: Mã bảng lương duy nhất để tra cứu',
  `salary_period_id` CHAR(36) NOT NULL COMMENT 'Cột salary_period_id: Khóa ngoại tham chiếu kỳ lương',
  `employee_id` CHAR(36) NOT NULL COMMENT 'Cột employee_id: Khóa ngoại tham chiếu nhân viên',
  `status` ENUM('draft','processing','locked','paid') NOT NULL DEFAULT 'draft' COMMENT 'Cột status: Trạng thái bảng lương: nháp, đang xử lý, khóa, đã thanh toán',
  `gross_salary` DECIMAL(15,2) NOT NULL DEFAULT 0 COMMENT 'Cột gross_salary: Tổng lương gross của kỳ',
  `net_salary` DECIMAL(15,2) NOT NULL DEFAULT 0 COMMENT 'Cột net_salary: Tổng lương net thực nhận của kỳ',
  `taxable_salary` DECIMAL(15,2) NOT NULL DEFAULT 0 COMMENT 'Cột taxable_salary: Tổng thu nhập chịu thuế của kỳ',
  `pit_tax_amount` DECIMAL(15,2) NOT NULL DEFAULT 0 COMMENT 'Cột pit_tax_amount: Tổng thuế TNCN phải đóng',
  `insurance_deduction` DECIMAL(15,2) NOT NULL DEFAULT 0 COMMENT 'Cột insurance_deduction: Tổng khấu trừ bảo hiểm (BHXH+BHYT+BHTN)',
  `working_days_actual` DECIMAL(6,2) NOT NULL DEFAULT 0 COMMENT 'Cột working_days_actual: Số ngày công thực tế làm việc',
  `working_days_standard` DECIMAL(6,2) NOT NULL DEFAULT 0 COMMENT 'Cột working_days_standard: Số ngày công chuẩn trong tháng',
  `total_allowance` DECIMAL(15,2) NOT NULL DEFAULT 0 COMMENT 'Cột total_allowance: Tổng các khoản phụ cấp của kỳ',
  `total_addition` DECIMAL(15,2) NOT NULL DEFAULT 0 COMMENT 'Cột total_addition: Tổng các khoản cộng thêm của kỳ',
  `total_deduction` DECIMAL(15,2) NOT NULL DEFAULT 0 COMMENT 'Cột total_deduction: Tổng các khoản khấu trừ của kỳ',
  `locked_at` DATETIME DEFAULT NULL COMMENT 'Cột locked_at: Thời điểm khóa bảng lương',
  `paid_at` DATETIME DEFAULT NULL COMMENT 'Cột paid_at: Thời điểm thanh toán bảng lương',
  `salary_policy_id` CHAR(36) DEFAULT NULL COMMENT 'Cột salary_policy_id: Khóa ngoại tham chiếu chính sách lương áp dụng',
  `deduction_policy_id` CHAR(36) DEFAULT NULL COMMENT 'Cột deduction_policy_id: Khóa ngoại tham chiếu chính sách giảm trừ/bảo hiểm',
  `employee_tax_profile_id` CHAR(36) DEFAULT NULL COMMENT 'Cột employee_tax_profile_id: Khóa ngoại tham chiếu hồ sơ thuế nhân viên',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo bảng lương',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo dữ liệu bảng lương',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của bảng lương',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của bảng lương',
  PRIMARY KEY (`payroll_id`),
  UNIQUE KEY `uk_payroll_code` (`payroll_code`),
  UNIQUE KEY `uk_payroll_period_employee` (`salary_period_id`,`employee_id`),
  KEY `idx_payroll_status` (`status`),
  KEY `idx_payroll_employee_id` (`employee_id`),
  KEY `idx_payroll_salary_policy` (`salary_policy_id`),
  KEY `idx_payroll_deduction_policy` (`deduction_policy_id`),
  KEY `idx_payroll_employee_tax_profile` (`employee_tax_profile_id`),
  CONSTRAINT `fk_payroll_salary_period` FOREIGN KEY (`salary_period_id`) REFERENCES `salary_period` (`salary_period_id`) ON UPDATE CASCADE,
  CONSTRAINT `fk_payroll_employee` FOREIGN KEY (`employee_id`) REFERENCES `employee` (`employee_id`) ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT `fk_payroll_salary_policy` FOREIGN KEY (`salary_policy_id`) REFERENCES `salary_policy` (`policy_id`) ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT `fk_payroll_deduction_policy` FOREIGN KEY (`deduction_policy_id`) REFERENCES `deduction_policy` (`deduction_policy_id`) ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT `fk_payroll_employee_tax_profile` FOREIGN KEY (`employee_tax_profile_id`) REFERENCES `employee_tax_profile` (`employee_tax_profile_id`) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng payroll: Lưu bảng lương tổng theo tháng của từng nhân viên';

CREATE TABLE `payroll_item` (
  `payroll_item_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột payroll_item_id: Khóa chính UUID của chi tiết bảng lương',
  `payroll_item_code` VARCHAR(50) NOT NULL COMMENT 'Cột payroll_item_code: Mã chi tiết bảng lương duy nhất để tra cứu',
  `payroll_id` CHAR(36) NOT NULL COMMENT 'Cột payroll_id: Khóa ngoại tham chiếu bảng lương tổng',
  `item_type` ENUM('addition','deduction') NOT NULL COMMENT 'Cột item_type: Loại khoản mục lương phát sinh theo tháng, gồm cộng thêm, khấu trừ',
  `item_name` VARCHAR(255) NOT NULL COMMENT 'Cột item_name: Tên khoản mục hiển thị trên bảng lương',
  `formula_component` ENUM('base_salary','allowance','bonus','penalty','insurance','tax','other') DEFAULT 'other' COMMENT 'Cột formula_component: Phân loại thành phần công thức tính lương',
  `amount` DECIMAL(15,2) NOT NULL COMMENT 'Cột amount: Giá trị tiền của khoản mục',
  `source_table` ENUM('attendance','evaluation','business_trip','leave_request','manual') NOT NULL DEFAULT 'manual' COMMENT 'Cột source_table: Nguồn phát sinh khoản mục lương theo tháng',
  `source_id` CHAR(36) DEFAULT NULL COMMENT 'Cột source_id: UUID bản ghi nguồn tạo ra khoản mục',
  `note` VARCHAR(255) DEFAULT NULL COMMENT 'Cột note: Ghi chú thêm cho khoản mục',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo chi tiết bảng lương',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo dữ liệu chi tiết bảng lương',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất của chi tiết bảng lương',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất của chi tiết bảng lương',
  PRIMARY KEY (`payroll_item_id`),
  UNIQUE KEY `uk_payroll_item_code` (`payroll_item_code`),
  KEY `idx_payroll_item_payroll_id` (`payroll_id`),
  KEY `idx_payroll_item_type` (`item_type`),
  KEY `idx_payroll_item_source` (`source_table`,`source_id`),
  CONSTRAINT `chk_payroll_item_source_pair` CHECK (
    (`source_table` = 'manual' AND `source_id` IS NULL)
    OR
    (`source_table` <> 'manual' AND `source_id` IS NOT NULL)
  ),
  CONSTRAINT `fk_payroll_item_payroll` FOREIGN KEY (`payroll_id`) REFERENCES `payroll` (`payroll_id`) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng payroll_item: Lưu các khoản phụ cấp, cộng thêm và khấu trừ theo tháng';

CREATE TABLE `payroll_snapshot` (
  `payroll_snapshot_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột payroll_snapshot_id: Khóa chính UUID của snapshot payroll',
  `payroll_id` CHAR(36) NOT NULL COMMENT 'Cột payroll_id: Khóa ngoại tham chiếu bảng payroll được snapshot',
  `snapshot_at` DATETIME NOT NULL COMMENT 'Cột snapshot_at: Thời điểm chụp snapshot khi lock kỳ lương',
  `contract_payload` JSON DEFAULT NULL COMMENT 'Cột contract_payload: Snapshot dữ liệu hợp đồng phục vụ tính lương',
  `policy_payload` JSON DEFAULT NULL COMMENT 'Cột policy_payload: Snapshot dữ liệu policy lương/giảm trừ/thuế',
  `tax_profile_payload` JSON DEFAULT NULL COMMENT 'Cột tax_profile_payload: Snapshot dữ liệu hồ sơ thuế nhân viên',
  `payroll_payload` JSON NOT NULL COMMENT 'Cột payroll_payload: Snapshot dữ liệu payroll tổng tại thời điểm lock',
  `items_payload` JSON DEFAULT NULL COMMENT 'Cột items_payload: Snapshot danh sách payroll item tại thời điểm lock',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo bản ghi snapshot',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo snapshot',
  PRIMARY KEY (`payroll_snapshot_id`),
  UNIQUE KEY `uk_payroll_snapshot_payroll` (`payroll_id`),
  KEY `idx_payroll_snapshot_at` (`snapshot_at`),
  CONSTRAINT `fk_payroll_snapshot_payroll` FOREIGN KEY (`payroll_id`) REFERENCES `payroll` (`payroll_id`) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng payroll_snapshot: Lưu snapshot đầy đủ khi khóa bảng lương';

-- =============================
-- POLICY & CONFIGURATION TABLES
-- =============================

CREATE TABLE `salary_policy` (
  `policy_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột policy_id: Khóa chính UUID của chính sách lương',
  `policy_code` VARCHAR(50) NOT NULL COMMENT 'Cột policy_code: Mã chính sách lương duy nhất để tra cứu',
  `policy_name` VARCHAR(255) NOT NULL COMMENT 'Cột policy_name: Tên chính sách lương',
  `standard_workdays` DECIMAL(6,2) NOT NULL DEFAULT 22.00 COMMENT 'Cột standard_workdays: Số ngày công chuẩn trong tháng để tính lương ngày',
  `overtime_multiplier_weekday` DECIMAL(6,2) NOT NULL DEFAULT 1.50 COMMENT 'Cột overtime_multiplier_weekday: Hệ số lương tăng cho ngày thường',
  `overtime_multiplier_weekend` DECIMAL(6,2) NOT NULL DEFAULT 2.00 COMMENT 'Cột overtime_multiplier_weekend: Hệ số lương tăng cho ngày cuối tuần',
  `overtime_multiplier_holiday` DECIMAL(6,2) NOT NULL DEFAULT 3.00 COMMENT 'Cột overtime_multiplier_holiday: Hệ số lương tăng cho ngày lễ',
  `effective_from` DATE NOT NULL COMMENT 'Cột effective_from: Ngày bắt đầu áp dụng chính sách',
  `effective_to` DATE DEFAULT NULL COMMENT 'Cột effective_to: Ngày kết thúc áp dụng chính sách (NULL = còn hiệu lực)',
  `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Cột is_active: Trạng thái hoạt động, 1 là hoạt động, 0 là ngừng',
  `description` TEXT DEFAULT NULL COMMENT 'Cột description: Ghi chú mô tả thêm về chính sách lương',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo chính sách',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất',
  PRIMARY KEY (`policy_id`),
  UNIQUE KEY `uk_salary_policy_code` (`policy_code`),
  KEY `idx_salary_policy_effective` (`effective_from`,`effective_to`),
  KEY `idx_salary_policy_active` (`is_active`),
  CONSTRAINT `chk_salary_policy_workdays` CHECK (`standard_workdays` > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng salary_policy: Lưu chính sách lương và thông số tính lương theo tháng';

CREATE TABLE `tax_bracket` (
  `tax_bracket_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột tax_bracket_id: Khóa chính UUID của bậc thuế TNCN',
  `bracket_code` VARCHAR(50) NOT NULL COMMENT 'Cột bracket_code: Mã bậc thuế để tra cứu',
  `bracket_name` VARCHAR(255) NOT NULL COMMENT 'Cột bracket_name: Tên bậc thuế hiển thị',
  `lower_bound` DECIMAL(15,2) NOT NULL COMMENT 'Cột lower_bound: Cận dưới của thu nhập chịu thuế',
  `upper_bound` DECIMAL(15,2) DEFAULT NULL COMMENT 'Cột upper_bound: Cận trên của thu nhập chịu thuế (NULL = không giới hạn)',
  `tax_rate` DECIMAL(6,2) NOT NULL COMMENT 'Cột tax_rate: Tỷ lệ thuế suất (%)',
  `quick_deduction` DECIMAL(15,2) NOT NULL DEFAULT 0 COMMENT 'Cột quick_deduction: Khoản giảm trừ nhanh cho bậc này',
  `effective_from` DATE NOT NULL COMMENT 'Cột effective_from: Ngày bắt đầu áp dụng bậc thuế',
  `effective_to` DATE DEFAULT NULL COMMENT 'Cột effective_to: Ngày kết thúc áp dụng bậc thuế (NULL = còn hiệu lực)',
  `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Cột is_active: Trạng thái hoạt động',
  `description` TEXT DEFAULT NULL COMMENT 'Cột description: Ghi chú mô tả về bậc thuế',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo bậc thuế',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất',
  PRIMARY KEY (`tax_bracket_id`),
  UNIQUE KEY `uk_tax_bracket_code` (`bracket_code`),
  KEY `idx_tax_bracket_effective` (`effective_from`,`effective_to`),
  KEY `idx_tax_bracket_bound` (`lower_bound`,`upper_bound`),
  KEY `idx_tax_bracket_active` (`is_active`),
  CONSTRAINT `chk_tax_bracket_bound` CHECK (`upper_bound` IS NULL OR `upper_bound` > `lower_bound`),
  CONSTRAINT `chk_tax_bracket_rate` CHECK (`tax_rate` >= 0 AND `tax_rate` <= 100)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng tax_bracket: Lưu bậc thuế TNCN theo từng năm';

CREATE TABLE `deduction_policy` (
  `deduction_policy_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột deduction_policy_id: Khóa chính UUID của chính sách giảm trừ/bảo hiểm',
  `policy_code` VARCHAR(50) NOT NULL COMMENT 'Cột policy_code: Mã chính sách duy nhất để tra cứu',
  `policy_name` VARCHAR(255) NOT NULL COMMENT 'Cột policy_name: Tên chính sách',
  `social_insurance_rate` DECIMAL(6,2) NOT NULL DEFAULT 8.00 COMMENT 'Cột social_insurance_rate: Tỷ lệ đóng BHXH (%)',
  `health_insurance_rate` DECIMAL(6,2) NOT NULL DEFAULT 1.50 COMMENT 'Cột health_insurance_rate: Tỷ lệ đóng BHYT (%)',
  `unemployment_insurance_rate` DECIMAL(6,2) NOT NULL DEFAULT 1.00 COMMENT 'Cột unemployment_insurance_rate: Tỷ lệ đóng BHTN (%)',
  `personal_deduction_amount` DECIMAL(15,2) NOT NULL DEFAULT 11000000 COMMENT 'Cột personal_deduction_amount: Khoản giảm trừ cá nhân',
  `dependent_deduction_amount` DECIMAL(15,2) NOT NULL DEFAULT 4400000 COMMENT 'Cột dependent_deduction_amount: Khoản giảm trừ cho mỗi người phụ thuộc',
  `effective_from` DATE NOT NULL COMMENT 'Cột effective_from: Ngày bắt đầu áp dụng chính sách',
  `effective_to` DATE DEFAULT NULL COMMENT 'Cột effective_to: Ngày kết thúc áp dụng chính sách (NULL = còn hiệu lực)',
  `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Cột is_active: Trạng thái hoạt động',
  `description` TEXT DEFAULT NULL COMMENT 'Cột description: Ghi chú mô tả về chính sách',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo chính sách',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất',
  PRIMARY KEY (`deduction_policy_id`),
  UNIQUE KEY `uk_deduction_policy_code` (`policy_code`),
  KEY `idx_deduction_policy_effective` (`effective_from`,`effective_to`),
  KEY `idx_deduction_policy_active` (`is_active`),
  CONSTRAINT `chk_deduction_rates` CHECK (
    `social_insurance_rate` >= 0 AND `social_insurance_rate` <= 100
    AND `health_insurance_rate` >= 0 AND `health_insurance_rate` <= 100
    AND `unemployment_insurance_rate` >= 0 AND `unemployment_insurance_rate` <= 100
  )
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng deduction_policy: Lưu chính sách giảm trừ và đóng bảo hiểm theo từng năm';

CREATE TABLE `employee_tax_profile` (
  `employee_tax_profile_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột employee_tax_profile_id: Khóa chính UUID của hồ sơ thuế nhân viên',
  `employee_id` CHAR(36) NOT NULL COMMENT 'Cột employee_id: Khóa ngoại tham chiếu nhân viên',
  `tax_code` VARCHAR(30) DEFAULT NULL COMMENT 'Cột tax_code: Mã số thuế của nhân viên',
  `dependent_count` INT NOT NULL DEFAULT 0 COMMENT 'Cột dependent_count: Số người phụ thuộc',
  `is_resident` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Cột is_resident: Có phải cư dân Việt Nam hay không (1 = có)',
  `effective_from` DATE NOT NULL COMMENT 'Cột effective_from: Ngày bắt đầu áp dụng hồ sơ thuế',
  `effective_to` DATE DEFAULT NULL COMMENT 'Cột effective_to: Ngày kết thúc áp dụng hồ sơ thuế (NULL = còn hiệu lực)',
  `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Cột is_active: Trạng thái hoạt động',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm tạo hồ sơ',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Cột updated_by: UUID người cập nhật gần nhất',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Cột updated_at: Thời điểm cập nhật gần nhất',
  PRIMARY KEY (`employee_tax_profile_id`),
  UNIQUE KEY `uk_employee_tax_profile` (`employee_id`,`effective_from`),
  KEY `idx_employee_tax_effective` (`effective_from`,`effective_to`),
  KEY `idx_employee_tax_active` (`is_active`),
  CONSTRAINT `fk_employee_tax_profile_employee` FOREIGN KEY (`employee_id`) REFERENCES `employee` (`employee_id`) ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT `chk_employee_tax_dependent` CHECK (`dependent_count` >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng employee_tax_profile: Lưu hồ sơ thuế và thông tin phụ thuộc của nhân viên';

CREATE TABLE `contract_history` (
  `contract_history_id` CHAR(36) NOT NULL DEFAULT (UUID()) COMMENT 'Cột contract_history_id: Khóa chính UUID của bản ghi lịch sử',
  `contract_id` CHAR(36) NOT NULL COMMENT 'Cột contract_id: Khóa ngoại tham chiếu hợp đồng',
  `change_type` ENUM('created','signed','updated_salary','updated_allowance','status_changed','terminated') NOT NULL COMMENT 'Cột change_type: Loại thay đổi hợp đồng',
  `old_values` JSON DEFAULT NULL COMMENT 'Cột old_values: Giá trị cũ dưới dạng JSON',
  `new_values` JSON DEFAULT NULL COMMENT 'Cột new_values: Giá trị mới dưới dạng JSON',
  `effective_date` DATE NOT NULL COMMENT 'Cột effective_date: Ngày thay đổi có hiệu lực',
  `reason` VARCHAR(255) DEFAULT NULL COMMENT 'Cột reason: Lý do thay đổi',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Cột created_at: Thời điểm ghi lại',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Cột created_by: UUID người tạo bản ghi',
  PRIMARY KEY (`contract_history_id`),
  KEY `idx_contract_history_contract` (`contract_id`),
  KEY `idx_contract_history_effective` (`effective_date`),
  CONSTRAINT `fk_contract_history_contract` FOREIGN KEY (`contract_id`) REFERENCES `contract` (`contract_id`) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng contract_history: Lưu lịch sử thay đổi chi tiết của hợp đồng';

-- =========================================================

-- =========================================================
-- DỮ LIỆU MẪU
-- =========================================================

INSERT INTO `shift` (`shift_id`,`shift_code`,`shift_name`,`start_time`,`break_start_time`,`end_time`,`break_end_time`,`working_hours`,`break_hours`,`is_active`,`description`) VALUES
(UUID(),'SHIFT_AM','Ca sáng','08:00:00','12:00:00','17:00:00','13:00:00',8.00,1.00,1,'Ca làm việc buổi sáng'),
(UUID(),'SHIFT_PM','Ca chiều','13:00:00','17:00:00','22:00:00','18:00:00',8.00,1.00,1,'Ca làm việc buổi chiều'),
(UUID(),'SHIFT_HC','Hành chính','08:30:00','12:00:00','17:30:00','13:30:00',8.00,1.00,1,'Ca hành chính');

INSERT INTO `degree` (`degree_id`,`degree_code`,`degree_name`,`description`) VALUES
(UUID(),'DEG_CAO_DANG','Cao đẳng','Bằng cao đẳng'),
(UUID(),'DEG_DAI_HOC','Đại học','Bằng đại học'),
(UUID(),'DEG_THAC_SI','Thạc sĩ','Bằng thạc sĩ'),
(UUID(),'DEG_TIEN_SI','Tiến sĩ','Bằng tiến sĩ');

INSERT INTO `position` (`position_id`,`position_code`,`position_name`,`description`,`allowance`) VALUES
(UUID(),'POS_STAFF','Nhân viên','Nhân viên nghiệp vụ',500000),
(UUID(),'POS_TEAM_LEAD','Tổ trưởng','Quản lý nhóm',1200000),
(UUID(),'POS_MANAGER','Trưởng phòng','Quản lý phòng ban',2500000),
(UUID(),'POS_DEPUTY','Phó phòng','Phó quản lý phòng ban',1800000),
(UUID(),'POS_DIRECTOR','Giám đốc','Điều hành khối',4000000);

INSERT INTO `allowance` (`allowance_id`,`allowance_code`,`allowance_name`,`calculation_type`,`amount`,`percent`,`version`) VALUES
(UUID(),'ALW_MEAL','Phụ cấp ăn trưa','FIXED',500000,NULL,1),
(UUID(),'ALW_PHONE','Phụ cấp điện thoại','FIXED',300000,NULL,1),
(UUID(),'ALW_RESP','Phụ cấp trách nhiệm','PERCENT',NULL,10.00,1),
(UUID(),'ALW_TRAVEL','Phụ cấp đi lại','FIXED',400000,NULL,1),
(UUID(),'ALW_HOUSING','Phụ cấp nhà ở','FIXED',800000,NULL,1);

INSERT INTO `contract_template` (`template_id`,`template_code`,`template_name`,`contract_type`,`content`,`version`,`is_active`) VALUES
(UUID(),'TPL_PROBATION','Mẫu hợp đồng thử việc','Thử việc','Nội dung mẫu thử việc',1,1),
(UUID(),'TPL_FIXED','Mẫu hợp đồng xác định thời hạn','Có thời hạn','Nội dung mẫu có thời hạn',1,1),
(UUID(),'TPL_OPEN','Mẫu hợp đồng không xác định thời hạn','Không thời hạn','Nội dung mẫu không thời hạn',1,1);

INSERT INTO `role` (`role_id`,`role_code`,`role_name`,`description`) VALUES
(UUID(),'ROLE_ADMIN','Quản trị viên','Toàn quyền hệ thống'),
(UUID(),'ROLE_HR','Nhân sự','Quản lý hồ sơ nhân sự'),
(UUID(),'ROLE_EMP','Nhân viên','Người dùng nhân viên');

INSERT INTO `account` (`account_id`,`account_code`,`username`,`password_hash`,`role_id`,`is_active`) VALUES
(UUID(),'ACC_ADMIN','admin','$2b$12$c5aggsyJ5ztAgCidkiawFO9CSNHfSULpZsC3goirv0lMS10QNWw6m',(SELECT role_id FROM role WHERE role_code='ROLE_ADMIN'),1),
(UUID(),'ACC_HR01','hr','$2b$12$tBFQXJhmYIMjGZ84D1SZH.vaOPbOFyeyXgnfDjcnJ46D4WMvvyg0G',(SELECT role_id FROM role WHERE role_code='ROLE_HR'),1),
(UUID(),'ACC_MGR01','manager','$2b$12$DcaI5cocW509bVHakUh9Q.pvZX7Z88aBkZNO/PR61lMu/5eOEC8vm',(SELECT role_id FROM role WHERE role_code='ROLE_HR'),1),
(UUID(),'ACC_EMP01','nhanvien','$2b$12$4MLovO9KvVr9ig.MDhytyuG4EAWXyMjyaL9eX1pPhrZMdQ.Vg3sNa',(SELECT role_id FROM role WHERE role_code='ROLE_EMP'),1);

INSERT INTO `department` (`department_id`,`department_code`,`department_name`,`description`) VALUES
(UUID(),'DEP_HR','Phòng nhân sự','Quản lý nhân sự và hành chính'),
(UUID(),'DEP_FIN','Phòng tài chính','Kế toán và tài chính'),
(UUID(),'DEP_SALES','Phòng kinh doanh','Kinh doanh và chăm sóc khách hàng'),
(UUID(),'DEP_PROD','Phòng sản xuất','Vận hành và sản xuất');

INSERT INTO `employee` (
  `employee_id`,`employee_code`,`full_name`,`gender`,`date_of_birth`,`address`,`phone_number`,`email`,`join_date`,
  `department_id`,`shift_id`,`national_id`,`degree_id`,`contract_id`,`position_id`,`account_id`,`avatar_url`,
  `place_of_birth`, `hometown`, `ethnic`, `religion`, `nationality`, `marital_status`, `personal_email`, 
  `facebook_url`, `zalo_number`, `temporary_address`, `bank_account_number`, `bank_name`, `bank_branch`, 
  `emergency_contact_name`, `emergency_contact_phone`, `emergency_contact_relationship`, `height`, `weight`, 
  `blood_group`, `health_status`, `social_insurance_number`
)
SELECT
  UUID(),
  CONCAT('EMP', LPAD(n, 4, '0')),
  ELT(MOD(n, 20) + 1, 
    'Nguyễn Minh Tuấn', 'Trần Thu Trang', 'Trần Đức Hải', 'Nguyễn Mai Phương', 
    'Lê Thanh Bình', 'Lê Thị Ngọc Ánh', 'Phạm Quốc Việt', 'Phạm Thanh Hương', 
    'Hoàng Trọng Nghĩa', 'Hoàng Thúy Quỳnh', 'Vũ Tuấn Anh', 'Vũ Hà My', 
    'Đặng Văn Khoa', 'Đặng Cẩm Tú', 'Bùi Xuân Bách', 'Bùi Thảo Ngọc', 
    'Đỗ Tiến Đạt', 'Đỗ Mai Lan', 'Ngô Hoàng Phúc', 'Ngô Kim Liên'
  ),
  CASE WHEN MOD(n,2)=0 THEN 'Nam' ELSE 'Nữ' END,
  DATE_ADD('1990-01-01', INTERVAL n * 120 DAY),
  CONCAT('Số ', n, ' Tôn Thất Thuyết, Cầu Giấy, Hà Nội'),
  CONCAT('0987', LPAD(n, 6, '0')),
  CONCAT('emp', LPAD(n, 4, '0'), '@misa.com.vn'),
  DATE_ADD('2023-01-01', INTERVAL n DAY),
  CASE MOD(n,4)
    WHEN 1 THEN (SELECT department_id FROM department WHERE department_code='DEP_HR')
    WHEN 2 THEN (SELECT department_id FROM department WHERE department_code='DEP_FIN')
    WHEN 3 THEN (SELECT department_id FROM department WHERE department_code='DEP_SALES')
    ELSE (SELECT department_id FROM department WHERE department_code='DEP_PROD')
  END,
  CASE MOD(n,3)
    WHEN 1 THEN (SELECT shift_id FROM shift WHERE shift_code='SHIFT_AM')
    WHEN 2 THEN (SELECT shift_id FROM shift WHERE shift_code='SHIFT_PM')
    ELSE (SELECT shift_id FROM shift WHERE shift_code='SHIFT_HC')
  END,
  CONCAT('001090', LPAD(n, 6, '0')),
  CASE MOD(n,4)
    WHEN 1 THEN (SELECT degree_id FROM degree WHERE degree_code='DEG_CAO_DANG')
    WHEN 2 THEN (SELECT degree_id FROM degree WHERE degree_code='DEG_DAI_HOC')
    WHEN 3 THEN (SELECT degree_id FROM degree WHERE degree_code='DEG_THAC_SI')
    ELSE (SELECT degree_id FROM degree WHERE degree_code='DEG_TIEN_SI')
  END,
  NULL,
  CASE
    WHEN MOD(n,10)=0 THEN (SELECT position_id FROM position WHERE position_code='POS_MANAGER')
    WHEN MOD(n,5)=0 THEN (SELECT position_id FROM position WHERE position_code='POS_TEAM_LEAD')
    ELSE (SELECT position_id FROM position WHERE position_code='POS_STAFF')
  END,
  CASE n
    WHEN 1 THEN (SELECT account_id FROM account WHERE account_code='ACC_ADMIN')
    WHEN 2 THEN (SELECT account_id FROM account WHERE account_code='ACC_HR01')
    WHEN 3 THEN (SELECT account_id FROM account WHERE account_code='ACC_MGR01')
    WHEN 4 THEN (SELECT account_id FROM account WHERE account_code='ACC_EMP01')
    ELSE NULL
  END,
  'profile.jpg',
  'Hà Nội', 'Thái Bình', 'Kinh', 'Không', 'Việt Nam', 'Độc thân', 
  CONCAT('nguyenvana', n, '@gmail.com'), 
  CONCAT('https://fb.com/nguyenvana', n), 
  CONCAT('0987', LPAD(n, 6, '0')), 
  CONCAT('Tạm trú tại Cầu Giấy, Hà Nội'),
  CONCAT('102', LPAD(n, 7, '0')), 'Vietcombank', 'Thăng Long',
  ELT(MOD(n, 5) + 1, 'Nguyễn Hữu Tài', 'Lê Kim Dung', 'Trần Văn Cường', 'Đặng Thị Yến', 'Phạm Hải Đăng'), 
  CONCAT('0912', LPAD(n, 6, '0')), 
  ELT(MOD(n, 3) + 1, 'Bố', 'Mẹ', 'Anh/Chị'),
  165 + MOD(n, 20), 55 + MOD(n, 15), 'O', 'Tốt', CONCAT('791', LPAD(n, 7, '0'))
FROM (
  SELECT d.n + (t.n * 10) AS n
  FROM (
    SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4
    UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9
  ) d
  CROSS JOIN (
    SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5
  ) t
) seq
WHERE seq.n BETWEEN 1 AND 50;

-- Set Manager for departments
UPDATE `department` d
JOIN `employee` e ON (
  (d.department_code = 'DEP_HR' AND e.employee_code = 'EMP0001') OR
  (d.department_code = 'DEP_FIN' AND e.employee_code = 'EMP0002') OR
  (d.department_code = 'DEP_SALES' AND e.employee_code = 'EMP0003') OR
  (d.department_code = 'DEP_PROD' AND e.employee_code = 'EMP0004')
)
SET d.manager_employee_id = e.employee_id;

INSERT INTO `contract` (
  `contract_id`,`contract_code`,`template_id`,`company_representative_id`,`company_signer_title`,`employee_id`,`effective_date`,`term_months`,
  `base_salary`,`insurance_salary`,`salary_ratio`,`summary`,`attachment_link`,`is_signed`,`signed_at`
)
SELECT
  UUID(),
  CONCAT('CT-', e.employee_code),
  CASE
    WHEN RIGHT(e.employee_code,2) IN ('01','02','03','04','05','06','07','08','09','10') THEN (SELECT template_id FROM contract_template WHERE template_code='TPL_PROBATION')
    WHEN MOD(CAST(RIGHT(e.employee_code,2) AS UNSIGNED),2)=0 THEN (SELECT template_id FROM contract_template WHERE template_code='TPL_FIXED')
    ELSE (SELECT template_id FROM contract_template WHERE template_code='TPL_OPEN')
  END,
  (SELECT employee_id FROM employee WHERE employee_code='EMP0001'),
  'Giám đốc nhân sự',
  e.employee_id,
  e.join_date,
  CASE
    WHEN RIGHT(e.employee_code,2) IN ('01','02','03','04','05','06','07','08','09','10') THEN 2
    WHEN MOD(CAST(RIGHT(e.employee_code,2) AS UNSIGNED),2)=0 THEN 24
    ELSE NULL
  END,
  9000000 + CAST(RIGHT(e.employee_code,2) AS UNSIGNED) * 120000,
  6500000 + CAST(RIGHT(e.employee_code,2) AS UNSIGNED) * 80000,
  100.00,
  CONCAT('Hợp đồng lao động cho ', e.full_name),
  CONCAT('https://files.misa.local/contracts/', e.employee_code, '.pdf'),
  1,
  NOW()
FROM employee e;

UPDATE employee e
JOIN contract c ON c.employee_id = e.employee_id
SET e.contract_id = c.contract_id;

INSERT INTO `contract_allowance` (`contract_allowance_id`,`contract_id`,`allowance_id`)
SELECT UUID(), c.contract_id, (SELECT allowance_id FROM allowance WHERE allowance_code='ALW_MEAL')
FROM contract c;

INSERT INTO `contract_allowance` (`contract_allowance_id`,`contract_id`,`allowance_id`)
SELECT UUID(), c.contract_id, (SELECT allowance_id FROM allowance WHERE allowance_code='ALW_PHONE')
FROM contract c;

INSERT INTO `salary_period` (`salary_period_id`,`start_date`,`end_date`,`status`) VALUES
(UUID(),'2026-01-01','2026-01-31','paid'),
(UUID(),'2026-02-01','2026-02-28','paid'),
(UUID(),'2026-03-01','2026-03-31','locked');

INSERT INTO `attendance` (
  `attendance_id`,`attendance_code`,`employee_id`,`shift_id`,`attendance_date`,`working_hours`,`overtime_hours`,`penalty_amount`,`bonus_amount`,`net_income`
)
SELECT
  UUID(),
  CONCAT('AT', REPLACE(DATE_FORMAT(DATE_ADD('2026-03-01', INTERVAL (d-1) DAY),'%Y-%m-%d'),'-',''), RIGHT(e.employee_code,4)),
  e.employee_id,
  e.shift_id,
  DATE_ADD('2026-03-01', INTERVAL (d-1) DAY),
  8.00,
  CASE WHEN d = 4 AND MOD(CAST(RIGHT(e.employee_code,2) AS UNSIGNED),3)=0 THEN 2.00 ELSE 0.00 END,
  CASE WHEN d = 2 AND MOD(CAST(RIGHT(e.employee_code,2) AS UNSIGNED),5)=0 THEN 100000 ELSE 0 END,
  CASE WHEN d = 4 AND MOD(CAST(RIGHT(e.employee_code,2) AS UNSIGNED),7)=0 THEN 150000 ELSE 0 END,
  0
FROM employee e
CROSS JOIN (
  SELECT 1 AS d UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4
) day_seq;

INSERT INTO `evaluation` (`evaluation_id`,`evaluation_code`,`employee_id`,`evaluation_type`,`reason`,`amount`,`evaluation_date`)
SELECT
  UUID(),
  CONCAT('EV', RIGHT(e.employee_code,4)),
  e.employee_id,
  CASE WHEN MOD(CAST(RIGHT(e.employee_code,2) AS UNSIGNED),2)=0 THEN 'Khen thưởng' ELSE 'Vi phạm' END,
  CASE WHEN MOD(CAST(RIGHT(e.employee_code,2) AS UNSIGNED),2)=0 THEN 'Hoàn thành xuất sắc nhiệm vụ' ELSE 'Đi muộn' END,
  CASE WHEN MOD(CAST(RIGHT(e.employee_code,2) AS UNSIGNED),2)=0 THEN 500000 ELSE 100000 END,
  '2026-03-15'
FROM employee e
WHERE MOD(CAST(RIGHT(e.employee_code,2) AS UNSIGNED), 4) = 0;


-- =========================================================
-- VIEWS
-- =========================================================



-- View danh sách nhân viên phục vụ hiển thị màn hình
-- Có thể chạy nhiều lần an toàn
DROP VIEW IF EXISTS `vw_employee_detail`;

CREATE OR REPLACE VIEW `vw_employee_detail` AS
SELECT
  e.`employee_id`,
  e.`employee_code`,
  e.`full_name`,
  e.`gender`,
  e.`date_of_birth`,
  e.`address`,
  e.`phone_number`,
  e.`email`,
  e.`join_date`,
  e.`contract_id`,
  e.`national_id`,
  e.`avatar_url`,
  d.`department_name`,
  s.`shift_name`,
  g.`degree_name`,
  p.`position_name`,
  a.`username` AS `account_name`,
  e.`created_at`,
  e.`updated_at`
FROM `employee` e
LEFT JOIN `department` d ON e.`department_id` = d.`department_id`
LEFT JOIN `shift` s ON e.`shift_id` = s.`shift_id`
LEFT JOIN `degree` g ON e.`degree_id` = g.`degree_id`
LEFT JOIN `position` p ON e.`position_id` = p.`position_id`
LEFT JOIN `account` a ON e.`account_id` = a.`account_id`;



-- View danh sách hợp đồng phục vụ hiển thị màn hình
-- Có thể chạy nhiều lần an toàn
DROP VIEW IF EXISTS `vw_contract_detail`;

CREATE OR REPLACE VIEW `vw_contract_detail` AS
SELECT
  c.`contract_id`,
  c.`contract_code`,
  c.`template_id`,
  t.`template_code`,
  t.`template_name`,
  t.`contract_type`,
  c.`company_representative_id`,
  rep.`employee_code` AS `company_representative_code`,
  rep.`full_name` AS `company_representative_name`,
  c.`company_signer_title`,
  c.`employee_id`,
  e.`employee_code`,
  e.`full_name` AS `employee_name`,
  c.`effective_date`,
  c.`term_months`,
  c.`base_salary`,
  c.`insurance_salary`,
  c.`salary_ratio`,
  c.`summary`,
  c.`attachment_link`,
  c.`is_signed`,
  c.`signed_at`,
  c.`contract_status`,
  c.`end_date`,
  c.`terminated_at`,
  c.`created_at`,
  c.`created_by`,
  c.`updated_by`,
  c.`updated_at`
FROM `contract` c
LEFT JOIN `contract_template` t ON c.`template_id` = t.`template_id`
LEFT JOIN `employee` e ON c.`employee_id` = e.`employee_id`
LEFT JOIN `employee` rep ON c.`company_representative_id` = rep.`employee_id`;




-- ==========================================================
-- View danh sách công tác kèm thông tin nhân viên
-- Có thể chạy nhiều lần an toàn
-- ==========================================================
DROP VIEW IF EXISTS `vw_business_trip_detail`;

CREATE OR REPLACE VIEW `vw_business_trip_detail` AS
SELECT
  bt.`business_trip_id`,
  bt.`business_trip_code`,
  bt.`employee_id`,
  e.`employee_code`,
  e.`full_name` AS `employee_name`,
  bt.`start_date`,
  bt.`end_date`,
  bt.`location`,
  bt.`purpose`,
  bt.`support_amount`,
  bt.`created_at`,
  bt.`created_by`,
  bt.`updated_by`,
  bt.`updated_at`
FROM `business_trip` bt
LEFT JOIN `employee` e ON bt.`employee_id` = e.`employee_id`;


-- ==========================================================
-- View danh sách đánh giá thưởng phạt kèm thông tin nhân viên
-- Có thể chạy nhiều lần an toàn
-- ==========================================================
DROP VIEW IF EXISTS `vw_evaluation_detail`;

CREATE OR REPLACE VIEW `vw_evaluation_detail` AS
SELECT
  ev.`evaluation_id`,
  ev.`evaluation_code`,
  ev.`employee_id`,
  e.`employee_code`,
  e.`full_name` AS `employee_name`,
  ev.`evaluation_type`,
  ev.`reason`,
  ev.`amount`,
  ev.`evaluation_date`,
  ev.`created_at`,
  ev.`created_by`,
  ev.`updated_by`,
  ev.`updated_at`
FROM `evaluation` ev
LEFT JOIN `employee` e ON ev.`employee_id` = e.`employee_id`;


COMMIT;

-- Payroll foundation migration (safe additive)
-- Date: 2026-04-21

USE `misa_qlsx`;

-- =============================
-- 1) Additive columns to existing tables
-- =============================

SET @db_name := DATABASE();

SET @sql := (
  SELECT IF(
    EXISTS (
      SELECT 1
      FROM information_schema.COLUMNS
      WHERE TABLE_SCHEMA = @db_name
        AND TABLE_NAME = 'contract'
        AND COLUMN_NAME = 'contract_status'
    ),
    'SELECT 1',
    "ALTER TABLE `contract` ADD COLUMN `contract_status` ENUM('draft','signed','active','expired','terminated') NOT NULL DEFAULT 'draft' AFTER `is_signed`"
  )
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(
    EXISTS (
      SELECT 1
      FROM information_schema.COLUMNS
      WHERE TABLE_SCHEMA = @db_name
        AND TABLE_NAME = 'contract'
        AND COLUMN_NAME = 'end_date'
    ),
    'SELECT 1',
    "ALTER TABLE `contract` ADD COLUMN `end_date` DATE DEFAULT NULL AFTER `effective_date`"
  )
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(
    EXISTS (
      SELECT 1
      FROM information_schema.COLUMNS
      WHERE TABLE_SCHEMA = @db_name
        AND TABLE_NAME = 'contract'
        AND COLUMN_NAME = 'terminated_at'
    ),
    'SELECT 1',
    "ALTER TABLE `contract` ADD COLUMN `terminated_at` DATETIME DEFAULT NULL AFTER `signed_at`"
  )
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(
    EXISTS (
      SELECT 1
      FROM information_schema.COLUMNS
      WHERE TABLE_SCHEMA = @db_name
        AND TABLE_NAME = 'payroll'
        AND COLUMN_NAME = 'pit_tax_amount'
    ),
    'SELECT 1',
    "ALTER TABLE `payroll` ADD COLUMN `pit_tax_amount` DECIMAL(15,2) NOT NULL DEFAULT 0 AFTER `taxable_salary`"
  )
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(
    EXISTS (
      SELECT 1
      FROM information_schema.COLUMNS
      WHERE TABLE_SCHEMA = @db_name
        AND TABLE_NAME = 'payroll'
        AND COLUMN_NAME = 'insurance_deduction'
    ),
    'SELECT 1',
    "ALTER TABLE `payroll` ADD COLUMN `insurance_deduction` DECIMAL(15,2) NOT NULL DEFAULT 0 AFTER `pit_tax_amount`"
  )
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(
    EXISTS (
      SELECT 1
      FROM information_schema.COLUMNS
      WHERE TABLE_SCHEMA = @db_name
        AND TABLE_NAME = 'payroll'
        AND COLUMN_NAME = 'working_days_actual'
    ),
    'SELECT 1',
    "ALTER TABLE `payroll` ADD COLUMN `working_days_actual` DECIMAL(6,2) NOT NULL DEFAULT 0 AFTER `insurance_deduction`"
  )
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(
    EXISTS (
      SELECT 1
      FROM information_schema.COLUMNS
      WHERE TABLE_SCHEMA = @db_name
        AND TABLE_NAME = 'payroll'
        AND COLUMN_NAME = 'working_days_standard'
    ),
    'SELECT 1',
    "ALTER TABLE `payroll` ADD COLUMN `working_days_standard` DECIMAL(6,2) NOT NULL DEFAULT 0 AFTER `working_days_actual`"
  )
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(
    EXISTS (
      SELECT 1
      FROM information_schema.COLUMNS
      WHERE TABLE_SCHEMA = @db_name
        AND TABLE_NAME = 'payroll'
        AND COLUMN_NAME = 'locked_at'
    ),
    'SELECT 1',
    "ALTER TABLE `payroll` ADD COLUMN `locked_at` DATETIME DEFAULT NULL AFTER `updated_at`"
  )
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(
    EXISTS (
      SELECT 1
      FROM information_schema.COLUMNS
      WHERE TABLE_SCHEMA = @db_name
        AND TABLE_NAME = 'payroll'
        AND COLUMN_NAME = 'paid_at'
    ),
    'SELECT 1',
    "ALTER TABLE `payroll` ADD COLUMN `paid_at` DATETIME DEFAULT NULL AFTER `locked_at`"
  )
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(
    EXISTS (
      SELECT 1
      FROM information_schema.COLUMNS
      WHERE TABLE_SCHEMA = @db_name
        AND TABLE_NAME = 'payroll_item'
        AND COLUMN_NAME = 'formula_component'
    ),
    'SELECT 1',
    "ALTER TABLE `payroll_item` ADD COLUMN `formula_component` ENUM('base_salary','allowance','bonus','penalty','insurance','tax','other') DEFAULT 'other' AFTER `item_name`"
  )
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(
    EXISTS (
      SELECT 1
      FROM information_schema.STATISTICS
      WHERE TABLE_SCHEMA = @db_name
        AND TABLE_NAME = 'contract'
        AND INDEX_NAME = 'idx_contract_status'
    ),
    'SELECT 1',
    'CREATE INDEX `idx_contract_status` ON `contract`(`contract_status`)'
  )
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(
    EXISTS (
      SELECT 1
      FROM information_schema.STATISTICS
      WHERE TABLE_SCHEMA = @db_name
        AND TABLE_NAME = 'contract'
        AND INDEX_NAME = 'idx_contract_effective_end'
    ),
    'SELECT 1',
    'CREATE INDEX `idx_contract_effective_end` ON `contract`(`employee_id`,`effective_date`,`end_date`)'
  )
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(
    EXISTS (
      SELECT 1
      FROM information_schema.STATISTICS
      WHERE TABLE_SCHEMA = @db_name
        AND TABLE_NAME = 'payroll'
        AND INDEX_NAME = 'idx_payroll_period_employee'
    ),
    'SELECT 1',
    'CREATE INDEX `idx_payroll_period_employee` ON `payroll`(`salary_period_id`,`employee_id`)'
  )
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- =============================
-- 2) New policy/config tables
-- =============================

CREATE TABLE IF NOT EXISTS `salary_policy` (
  `policy_id` CHAR(36) NOT NULL DEFAULT (UUID()),
  `policy_code` VARCHAR(50) NOT NULL,
  `policy_name` VARCHAR(255) NOT NULL,
  `standard_workdays` DECIMAL(6,2) NOT NULL DEFAULT 22.00,
  `overtime_multiplier_weekday` DECIMAL(6,2) NOT NULL DEFAULT 1.50,
  `overtime_multiplier_weekend` DECIMAL(6,2) NOT NULL DEFAULT 2.00,
  `overtime_multiplier_holiday` DECIMAL(6,2) NOT NULL DEFAULT 3.00,
  `effective_from` DATE NOT NULL,
  `effective_to` DATE DEFAULT NULL,
  `is_active` TINYINT(1) NOT NULL DEFAULT 1,
  `description` TEXT DEFAULT NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `created_by` CHAR(36) DEFAULT NULL,
  `updated_by` CHAR(36) DEFAULT NULL,
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`policy_id`),
  UNIQUE KEY `uk_salary_policy_code` (`policy_code`),
  KEY `idx_salary_policy_effective` (`effective_from`,`effective_to`),
  KEY `idx_salary_policy_active` (`is_active`),
  CONSTRAINT `chk_salary_policy_workdays` CHECK (`standard_workdays` > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `tax_bracket` (
  `tax_bracket_id` CHAR(36) NOT NULL DEFAULT (UUID()),
  `bracket_code` VARCHAR(50) NOT NULL,
  `bracket_name` VARCHAR(255) NOT NULL,
  `lower_bound` DECIMAL(15,2) NOT NULL,
  `upper_bound` DECIMAL(15,2) DEFAULT NULL,
  `tax_rate` DECIMAL(6,2) NOT NULL,
  `quick_deduction` DECIMAL(15,2) NOT NULL DEFAULT 0,
  `effective_from` DATE NOT NULL,
  `effective_to` DATE DEFAULT NULL,
  `is_active` TINYINT(1) NOT NULL DEFAULT 1,
  `description` TEXT DEFAULT NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `created_by` CHAR(36) DEFAULT NULL,
  `updated_by` CHAR(36) DEFAULT NULL,
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`tax_bracket_id`),
  UNIQUE KEY `uk_tax_bracket_code` (`bracket_code`),
  KEY `idx_tax_bracket_effective` (`effective_from`,`effective_to`),
  KEY `idx_tax_bracket_bound` (`lower_bound`,`upper_bound`),
  KEY `idx_tax_bracket_active` (`is_active`),
  CONSTRAINT `chk_tax_bracket_bound` CHECK (`upper_bound` IS NULL OR `upper_bound` > `lower_bound`),
  CONSTRAINT `chk_tax_bracket_rate` CHECK (`tax_rate` >= 0 AND `tax_rate` <= 100)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `deduction_policy` (
  `deduction_policy_id` CHAR(36) NOT NULL DEFAULT (UUID()),
  `policy_code` VARCHAR(50) NOT NULL,
  `policy_name` VARCHAR(255) NOT NULL,
  `social_insurance_rate` DECIMAL(6,2) NOT NULL DEFAULT 8.00,
  `health_insurance_rate` DECIMAL(6,2) NOT NULL DEFAULT 1.50,
  `unemployment_insurance_rate` DECIMAL(6,2) NOT NULL DEFAULT 1.00,
  `personal_deduction_amount` DECIMAL(15,2) NOT NULL DEFAULT 11000000,
  `dependent_deduction_amount` DECIMAL(15,2) NOT NULL DEFAULT 4400000,
  `effective_from` DATE NOT NULL,
  `effective_to` DATE DEFAULT NULL,
  `is_active` TINYINT(1) NOT NULL DEFAULT 1,
  `description` TEXT DEFAULT NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `created_by` CHAR(36) DEFAULT NULL,
  `updated_by` CHAR(36) DEFAULT NULL,
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`deduction_policy_id`),
  UNIQUE KEY `uk_deduction_policy_code` (`policy_code`),
  KEY `idx_deduction_policy_effective` (`effective_from`,`effective_to`),
  KEY `idx_deduction_policy_active` (`is_active`),
  CONSTRAINT `chk_deduction_rates` CHECK (
    `social_insurance_rate` >= 0 AND `social_insurance_rate` <= 100
    AND `health_insurance_rate` >= 0 AND `health_insurance_rate` <= 100
    AND `unemployment_insurance_rate` >= 0 AND `unemployment_insurance_rate` <= 100
  )
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `employee_tax_profile` (
  `employee_tax_profile_id` CHAR(36) NOT NULL DEFAULT (UUID()),
  `employee_id` CHAR(36) NOT NULL,
  `tax_code` VARCHAR(30) DEFAULT NULL,
  `dependent_count` INT NOT NULL DEFAULT 0,
  `is_resident` TINYINT(1) NOT NULL DEFAULT 1,
  `effective_from` DATE NOT NULL,
  `effective_to` DATE DEFAULT NULL,
  `is_active` TINYINT(1) NOT NULL DEFAULT 1,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `created_by` CHAR(36) DEFAULT NULL,
  `updated_by` CHAR(36) DEFAULT NULL,
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`employee_tax_profile_id`),
  UNIQUE KEY `uk_employee_tax_profile` (`employee_id`,`effective_from`),
  KEY `idx_employee_tax_effective` (`effective_from`,`effective_to`),
  KEY `idx_employee_tax_active` (`is_active`),
  CONSTRAINT `fk_employee_tax_profile_employee`
    FOREIGN KEY (`employee_id`) REFERENCES `employee`(`employee_id`)
    ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT `chk_employee_tax_dependent` CHECK (`dependent_count` >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `contract_history` (
  `contract_history_id` CHAR(36) NOT NULL DEFAULT (UUID()),
  `contract_id` CHAR(36) NOT NULL,
  `change_type` ENUM('created','signed','updated_salary','updated_allowance','status_changed','terminated') NOT NULL,
  `old_values` JSON DEFAULT NULL,
  `new_values` JSON DEFAULT NULL,
  `effective_date` DATE NOT NULL,
  `reason` VARCHAR(255) DEFAULT NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `created_by` CHAR(36) DEFAULT NULL,
  PRIMARY KEY (`contract_history_id`),
  KEY `idx_contract_history_contract` (`contract_id`),
  KEY `idx_contract_history_effective` (`effective_date`),
  CONSTRAINT `fk_contract_history_contract`
    FOREIGN KEY (`contract_id`) REFERENCES `contract`(`contract_id`)
    ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================
-- 3) Seed base configuration
-- =============================

INSERT INTO `salary_policy`
(`policy_id`,`policy_code`,`policy_name`,`standard_workdays`,`effective_from`,`description`,`is_active`)
VALUES
('a1000000-0000-0000-0000-000000000001','SALPOL_2026','Chinh sach luong chuan 2026',22.00,'2026-01-01','Dung cho tinh luong ngay cong: (baseSalary*salaryRatio/100)/22',1)
ON DUPLICATE KEY UPDATE
  `policy_name` = VALUES(`policy_name`),
  `standard_workdays` = VALUES(`standard_workdays`),
  `effective_from` = VALUES(`effective_from`),
  `is_active` = VALUES(`is_active`),
  `description` = VALUES(`description`);

INSERT INTO `deduction_policy`
(`deduction_policy_id`,`policy_code`,`policy_name`,`social_insurance_rate`,`health_insurance_rate`,`unemployment_insurance_rate`,`personal_deduction_amount`,`dependent_deduction_amount`,`effective_from`,`is_active`)
VALUES
('a2000000-0000-0000-0000-000000000001','DEDPOL_2026','Giam tru va bao hiem 2026',8.00,1.50,1.00,11000000,4400000,'2026-01-01',1)
ON DUPLICATE KEY UPDATE
  `policy_name` = VALUES(`policy_name`),
  `social_insurance_rate` = VALUES(`social_insurance_rate`),
  `health_insurance_rate` = VALUES(`health_insurance_rate`),
  `unemployment_insurance_rate` = VALUES(`unemployment_insurance_rate`),
  `personal_deduction_amount` = VALUES(`personal_deduction_amount`),
  `dependent_deduction_amount` = VALUES(`dependent_deduction_amount`),
  `effective_from` = VALUES(`effective_from`),
  `is_active` = VALUES(`is_active`);

INSERT INTO `tax_bracket`
(`tax_bracket_id`,`bracket_code`,`bracket_name`,`lower_bound`,`upper_bound`,`tax_rate`,`quick_deduction`,`effective_from`,`is_active`)
VALUES
('a3000000-0000-0000-0000-000000000001','PIT_B1_2026','Bac 1',0,5000000,5.00,0,'2026-01-01',1),
('a3000000-0000-0000-0000-000000000002','PIT_B2_2026','Bac 2',5000000,10000000,10.00,250000,'2026-01-01',1),
('a3000000-0000-0000-0000-000000000003','PIT_B3_2026','Bac 3',10000000,18000000,15.00,750000,'2026-01-01',1),
('a3000000-0000-0000-0000-000000000004','PIT_B4_2026','Bac 4',18000000,32000000,20.00,1650000,'2026-01-01',1),
('a3000000-0000-0000-0000-000000000005','PIT_B5_2026','Bac 5',32000000,52000000,25.00,3250000,'2026-01-01',1),
('a3000000-0000-0000-0000-000000000006','PIT_B6_2026','Bac 6',52000000,80000000,30.00,5850000,'2026-01-01',1),
('a3000000-0000-0000-0000-000000000007','PIT_B7_2026','Bac 7',80000000,NULL,35.00,9850000,'2026-01-01',1)
ON DUPLICATE KEY UPDATE
  `bracket_name` = VALUES(`bracket_name`),
  `lower_bound` = VALUES(`lower_bound`),
  `upper_bound` = VALUES(`upper_bound`),
  `tax_rate` = VALUES(`tax_rate`),
  `quick_deduction` = VALUES(`quick_deduction`),
  `effective_from` = VALUES(`effective_from`),
  `is_active` = VALUES(`is_active`);
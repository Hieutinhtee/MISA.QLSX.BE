-- Migration 004: Link leave_request to approval_request
-- Created: 2026-05-09

USE `misa_qlsx`;
SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

ALTER TABLE `leave_request`
  ADD COLUMN `approval_request_id` CHAR(36) DEFAULT NULL COMMENT 'Khóa ngoại tham chiếu yêu cầu phê duyệt' AFTER `approval_status`,
  ADD KEY `idx_leave_request_approval_request_id` (`approval_request_id`),
  ADD CONSTRAINT `fk_leave_request_approval_request`
    FOREIGN KEY (`approval_request_id`) REFERENCES `approval_request` (`approval_request_id`)
    ON UPDATE CASCADE ON DELETE SET NULL;

SET FOREIGN_KEY_CHECKS = 1;
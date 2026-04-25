USE `misa_qlsx`;

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

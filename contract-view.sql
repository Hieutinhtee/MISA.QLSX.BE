USE `misa_qlsx`;

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
  c.`created_at`,
  c.`updated_at`
FROM `contract` c
LEFT JOIN `contract_template` t ON c.`template_id` = t.`template_id`
LEFT JOIN `employee` e ON c.`employee_id` = e.`employee_id`
LEFT JOIN `employee` rep ON c.`company_representative_id` = rep.`employee_id`;

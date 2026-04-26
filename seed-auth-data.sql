USE `misa_qlsx`;

START TRANSACTION;

-- 1) Seed role
INSERT INTO `role` (`role_code`, `role_name`, `description`)
VALUES
  ('ADMIN', 'Quản trị hệ thống', 'Toàn quyền quản trị'),
  ('HR', 'Nhân sự', 'Quản lý hồ sơ và hợp đồng nhân sự'),
  ('MANAGER', 'Quản lý', 'Quản lý đội nhóm theo phòng ban'),
  ('EMPLOYEE', 'Nhân viên', 'Người dùng nhân viên')
AS new
ON DUPLICATE KEY UPDATE
  `role_name` = new.`role_name`,
  `description` = new.`description`;

-- 2) Seed degree mặc định nếu thiếu
INSERT INTO `degree` (`degree_code`, `degree_name`, `description`)
VALUES ('DEG-SEED', 'Bằng cấp mặc định seed', 'Dùng cho dữ liệu test phân quyền')
AS new
ON DUPLICATE KEY UPDATE
  `degree_name` = new.`degree_name`,
  `description` = new.`description`;

-- 3) Seed 4 phòng ban khác nhau
INSERT INTO `department` (`department_code`, `department_name`, `description`)
VALUES
  ('DPT-ADM', 'Phòng Điều hành', 'Phòng ban cho user ADMIN'),
  ('DPT-HR', 'Phòng Nhân sự', 'Phòng ban cho user HR'),
  ('DPT-MGR', 'Phòng Quản lý', 'Phòng ban cho user MANAGER'),
  ('DPT-EMP', 'Phòng Vận hành', 'Phòng ban cho user EMPLOYEE')
AS new
ON DUPLICATE KEY UPDATE
  `department_name` = new.`department_name`,
  `description` = new.`description`;

-- 4) Seed account (password gốc: 123456, hash BCrypt)
INSERT INTO `account` (`account_code`, `username`, `password_hash`, `role_id`, `is_active`)
SELECT
  'ACC-ADMIN',
  'admin',
  '$2b$12$c5aggsyJ5ztAgCidkiawFO9CSNHfSULpZsC3goirv0lMS10QNWw6m',
  r.`role_id`,
  1
FROM `role` r
WHERE r.`role_code` = 'ADMIN'
ON DUPLICATE KEY UPDATE
  `password_hash` = '$2b$12$c5aggsyJ5ztAgCidkiawFO9CSNHfSULpZsC3goirv0lMS10QNWw6m',
  `role_id` = (SELECT `role_id` FROM `role` WHERE `role_code` = 'ADMIN' LIMIT 1),
  `is_active` = 1;

INSERT INTO `account` (`account_code`, `username`, `password_hash`, `role_id`, `is_active`)
SELECT
  'ACC-HR',
  'hr',
  '$2b$12$tBFQXJhmYIMjGZ84D1SZH.vaOPbOFyeyXgnfDjcnJ46D4WMvvyg0G',
  r.`role_id`,
  1
FROM `role` r
WHERE r.`role_code` = 'HR'
ON DUPLICATE KEY UPDATE
  `password_hash` = '$2b$12$tBFQXJhmYIMjGZ84D1SZH.vaOPbOFyeyXgnfDjcnJ46D4WMvvyg0G',
  `role_id` = (SELECT `role_id` FROM `role` WHERE `role_code` = 'HR' LIMIT 1),
  `is_active` = 1;

INSERT INTO `account` (`account_code`, `username`, `password_hash`, `role_id`, `is_active`)
SELECT
  'ACC-MANAGER',
  'manager',
  '$2b$12$DcaI5cocW509bVHakUh9Q.pvZX7Z88aBkZNO/PR61lMu/5eOEC8vm',
  r.`role_id`,
  1
FROM `role` r
WHERE r.`role_code` = 'MANAGER'
ON DUPLICATE KEY UPDATE
  `password_hash` = '$2b$12$DcaI5cocW509bVHakUh9Q.pvZX7Z88aBkZNO/PR61lMu/5eOEC8vm',
  `role_id` = (SELECT `role_id` FROM `role` WHERE `role_code` = 'MANAGER' LIMIT 1),
  `is_active` = 1;

INSERT INTO `account` (`account_code`, `username`, `password_hash`, `role_id`, `is_active`)
SELECT
  'ACC-EMP',
  'nhanvien',
  '$2b$12$4MLovO9KvVr9ig.MDhytyuG4EAWXyMjyaL9eX1pPhrZMdQ.Vg3sNa',
  r.`role_id`,
  1
FROM `role` r
WHERE r.`role_code` = 'EMPLOYEE'
ON DUPLICATE KEY UPDATE
  `password_hash` = '$2b$12$4MLovO9KvVr9ig.MDhytyuG4EAWXyMjyaL9eX1pPhrZMdQ.Vg3sNa',
  `role_id` = (SELECT `role_id` FROM `role` WHERE `role_code` = 'EMPLOYEE' LIMIT 1),
  `is_active` = 1;

-- 5) Seed employee gắn với từng account, mỗi account thuộc một phòng ban khác nhau
INSERT INTO `employee`
(
  `employee_id`, `employee_code`, `full_name`, `gender`, `date_of_birth`, `address`,
  `phone_number`, `email`, `join_date`, `department_id`, `shift_id`, `national_id`,
  `degree_id`, `contract_id`, `position_id`, `account_id`, `avatar_url`
)
SELECT
  '50000000-0000-0000-0000-000000000001',
  'NV-ADMIN',
  'Nguyen Admin',
  'Nam',
  '1990-01-01',
  'Ha Noi',
  '0900000001',
  'admin@misa.local',
  '2024-01-01',
  d.`department_id`,
  NULL,
  '111111111111',
  g.`degree_id`,
  NULL,
  NULL,
  a.`account_id`,
  'profile.jpg'
FROM `department` d
JOIN `account` a ON a.`username` = 'admin'
JOIN `degree` g ON g.`degree_code` = 'DEG-SEED'
WHERE d.`department_code` = 'DPT-ADM'
ON DUPLICATE KEY UPDATE
  `full_name` = 'Nguyen Admin',
  `department_id` = (SELECT `department_id` FROM `department` WHERE `department_code` = 'DPT-ADM' LIMIT 1),
  `account_id` = (SELECT `account_id` FROM `account` WHERE `username` = 'admin' LIMIT 1),
  `email` = 'admin@misa.local',
  `phone_number` = '0900000001';

INSERT INTO `employee`
(
  `employee_id`, `employee_code`, `full_name`, `gender`, `date_of_birth`, `address`,
  `phone_number`, `email`, `join_date`, `department_id`, `shift_id`, `national_id`,
  `degree_id`, `contract_id`, `position_id`, `account_id`, `avatar_url`
)
SELECT
  '50000000-0000-0000-0000-000000000002',
  'NV-HR',
  'Tran HR',
  'Nu',
  '1992-02-02',
  'Da Nang',
  '0900000002',
  'hr@misa.local',
  '2024-01-01',
  d.`department_id`,
  NULL,
  '222222222222',
  g.`degree_id`,
  NULL,
  NULL,
  a.`account_id`,
  'profile.jpg'
FROM `department` d
JOIN `account` a ON a.`username` = 'hr'
JOIN `degree` g ON g.`degree_code` = 'DEG-SEED'
WHERE d.`department_code` = 'DPT-HR'
ON DUPLICATE KEY UPDATE
  `full_name` = 'Tran HR',
  `department_id` = (SELECT `department_id` FROM `department` WHERE `department_code` = 'DPT-HR' LIMIT 1),
  `account_id` = (SELECT `account_id` FROM `account` WHERE `username` = 'hr' LIMIT 1),
  `email` = 'hr@misa.local',
  `phone_number` = '0900000002';

INSERT INTO `employee`
(
  `employee_id`, `employee_code`, `full_name`, `gender`, `date_of_birth`, `address`,
  `phone_number`, `email`, `join_date`, `department_id`, `shift_id`, `national_id`,
  `degree_id`, `contract_id`, `position_id`, `account_id`, `avatar_url`
)
SELECT
  '50000000-0000-0000-0000-000000000003',
  'NV-MANAGER',
  'Le Manager',
  'Nam',
  '1988-03-03',
  'Hai Phong',
  '0900000003',
  'manager@misa.local',
  '2024-01-01',
  d.`department_id`,
  NULL,
  '333333333333',
  g.`degree_id`,
  NULL,
  NULL,
  a.`account_id`,
  'profile.jpg'
FROM `department` d
JOIN `account` a ON a.`username` = 'manager'
JOIN `degree` g ON g.`degree_code` = 'DEG-SEED'
WHERE d.`department_code` = 'DPT-MGR'
ON DUPLICATE KEY UPDATE
  `full_name` = 'Le Manager',
  `department_id` = (SELECT `department_id` FROM `department` WHERE `department_code` = 'DPT-MGR' LIMIT 1),
  `account_id` = (SELECT `account_id` FROM `account` WHERE `username` = 'manager' LIMIT 1),
  `email` = 'manager@misa.local',
  `phone_number` = '0900000003';

INSERT INTO `employee`
(
  `employee_id`, `employee_code`, `full_name`, `gender`, `date_of_birth`, `address`,
  `phone_number`, `email`, `join_date`, `department_id`, `shift_id`, `national_id`,
  `degree_id`, `contract_id`, `position_id`, `account_id`, `avatar_url`
)
SELECT
  '50000000-0000-0000-0000-000000000004',
  'NV-EMP',
  'Pham Nhan Vien',
  'Nu',
  '1995-04-04',
  'Can Tho',
  '0900000004',
  'nhanvien@misa.local',
  '2024-01-01',
  d.`department_id`,
  NULL,
  '444444444444',
  g.`degree_id`,
  NULL,
  NULL,
  a.`account_id`,
  'profile.jpg'
FROM `department` d
JOIN `account` a ON a.`username` = 'nhanvien'
JOIN `degree` g ON g.`degree_code` = 'DEG-SEED'
WHERE d.`department_code` = 'DPT-EMP'
ON DUPLICATE KEY UPDATE
  `full_name` = 'Pham Nhan Vien',
  `department_id` = (SELECT `department_id` FROM `department` WHERE `department_code` = 'DPT-EMP' LIMIT 1),
  `account_id` = (SELECT `account_id` FROM `account` WHERE `username` = 'nhanvien' LIMIT 1),
  `email` = 'nhanvien@misa.local',
  `phone_number` = '0900000004';

COMMIT;

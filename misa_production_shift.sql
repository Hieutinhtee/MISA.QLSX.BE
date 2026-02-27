CREATE TABLE misa_production_development.production_shift (
  production_shift_id char(36) NOT NULL DEFAULT (UUID()) COMMENT 'ID ca làm việc',
  production_shift_code varchar(20) NOT NULL COMMENT 'Mã ca làm việc',
  production_shift_name varchar(50) NOT NULL COMMENT 'Tên ca làm việc',
  production_shift_begin_time time NOT NULL COMMENT 'Thời gian bắt đầu ca',
  production_shift_begin_break_time time DEFAULT NULL COMMENT 'Thời gian bắt đầu nghỉ giữa ca',
  production_shift_end_time time NOT NULL COMMENT 'Thời gian kết thúc ca',
  production_shift_end_break_time time DEFAULT NULL COMMENT 'Thời gian kết thúc nghỉ giữa ca',
  production_shift_working_time decimal(18, 2) NOT NULL COMMENT 'Tổng thời gian làm việc (giờ)',
  production_shift_break_time decimal(18, 2) NOT NULL DEFAULT 0.00 COMMENT 'Tổng thời gian nghỉ (giờ)',
  production_shift_is_active tinyint NOT NULL DEFAULT 1 COMMENT 'Trạng thái hoạt động: 1-Đang hoạt động, 0-Ngừng hoạt động',
  production_shift_created_by varchar(255) NOT NULL COMMENT 'Người tạo',
  production_shift_created_date datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Ngày tạo',
  production_shift_modified_by varchar(255) DEFAULT NULL COMMENT 'Người sửa',
  production_shift_modified_date datetime DEFAULT NULL COMMENT 'Ngày sửa',
  PRIMARY KEY (production_shift_id)
)
ENGINE = INNODB,
CHARACTER SET utf8mb4,
COLLATE utf8mb4_0900_as_ci,
COMMENT = 'Danh mục ca làm việc thuộc phân hệ quản lý sản xuất';

ALTER TABLE misa_production_development.production_shift
ADD INDEX ix_productionshift_productionshiftcreatedby (production_shift_created_by);

ALTER TABLE misa_production_development.production_shift
ADD INDEX ix_productionshift_productionshiftmodifiedby (production_shift_modified_by);

ALTER TABLE misa_production_development.production_shift
ADD INDEX ix_productionshift_productionshiftname (production_shift_name);

ALTER TABLE misa_production_development.production_shift
ADD UNIQUE INDEX uix_productionshift_productionshiftcode (production_shift_code);
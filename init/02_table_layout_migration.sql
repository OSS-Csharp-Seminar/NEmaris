USE NEmaris;

DROP PROCEDURE IF EXISTS add_table_layout_columns;

DELIMITER //
CREATE PROCEDURE add_table_layout_columns()
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'restaurant_tables'
          AND column_name = 'floor'
    ) THEN
        ALTER TABLE restaurant_tables ADD COLUMN floor INT NOT NULL DEFAULT 1 AFTER status;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'restaurant_tables'
          AND column_name = 'position_x'
    ) THEN
        ALTER TABLE restaurant_tables ADD COLUMN position_x DECIMAL(5,2) NOT NULL DEFAULT 0 AFTER floor;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'restaurant_tables'
          AND column_name = 'position_y'
    ) THEN
        ALTER TABLE restaurant_tables ADD COLUMN position_y DECIMAL(5,2) NOT NULL DEFAULT 0 AFTER position_x;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'restaurant_tables'
          AND column_name = 'shape'
    ) THEN
        ALTER TABLE restaurant_tables ADD COLUMN shape INT NOT NULL DEFAULT 0 AFTER position_y;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'restaurant_tables'
          AND column_name = 'rotation'
    ) THEN
        ALTER TABLE restaurant_tables ADD COLUMN rotation INT NOT NULL DEFAULT 0 AFTER shape;
    END IF;
END //
DELIMITER ;

CALL add_table_layout_columns();
DROP PROCEDURE add_table_layout_columns;

DROP PROCEDURE IF EXISTS create_idx_tables_floor;

DELIMITER //
CREATE PROCEDURE create_idx_tables_floor()
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.statistics
        WHERE table_schema = DATABASE()
          AND table_name = 'restaurant_tables'
          AND index_name = 'idx_tables_floor'
    ) THEN
        CREATE INDEX idx_tables_floor ON restaurant_tables(floor);
    END IF;
END //
DELIMITER ;

CALL create_idx_tables_floor();
DROP PROCEDURE create_idx_tables_floor;

INSERT INTO restaurant_tables
    (table_number, capacity, zone, status, floor, position_x, position_y, shape, rotation, created_at, updated_at)
VALUES
    ('F1-T1', 2, 'Main', 0, 1, 20, 22, 0, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F1-T2', 4, 'Main', 1, 1, 42, 22, 0, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F1-T3', 4, 'Main', 0, 1, 64, 22, 1, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F1-T4', 2, 'Main', 0, 1, 84, 22, 0, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F1-T5', 6, 'Center', 2, 1, 23, 48, 2, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F1-T6', 4, 'Center', 0, 1, 52, 48, 1, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F1-T7', 6, 'Center', 1, 1, 80, 48, 2, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F1-T8', 4, 'Window', 0, 1, 23, 76, 0, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F1-T9', 8, 'Window', 0, 1, 54, 76, 2, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F1-T10', 5, 'Window', 2, 1, 84, 76, 1, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F2-T1', 2, 'Main', 0, 2, 22, 24, 0, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F2-T2', 6, 'Main', 2, 2, 52, 24, 2, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F2-T3', 4, 'Main', 0, 2, 82, 24, 1, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F2-T4', 3, 'Center', 1, 2, 24, 52, 1, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F2-T5', 10, 'Center', 0, 2, 58, 52, 2, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F2-T6', 2, 'Center', 0, 2, 86, 52, 0, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F2-T7', 4, 'Window', 1, 2, 28, 78, 0, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F2-T8', 6, 'Window', 0, 2, 58, 78, 1, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F2-T9', 4, 'Window', 2, 2, 84, 78, 0, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F3-T1', 2, 'Main', 1, 3, 22, 24, 0, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F3-T2', 4, 'Main', 0, 3, 45, 24, 0, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F3-T3', 5, 'Main', 2, 3, 72, 24, 1, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F3-T4', 8, 'Center', 0, 3, 34, 54, 2, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F3-T5', 4, 'Center', 0, 3, 68, 54, 1, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F3-T6', 6, 'Center', 2, 3, 86, 54, 2, 90, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F3-T7', 3, 'Window', 0, 3, 27, 80, 1, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    ('F3-T8', 6, 'Window', 1, 3, 62, 80, 2, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6))
ON DUPLICATE KEY UPDATE
    capacity = VALUES(capacity),
    zone = VALUES(zone),
    status = VALUES(status),
    floor = VALUES(floor),
    position_x = VALUES(position_x),
    position_y = VALUES(position_y),
    shape = VALUES(shape),
    rotation = VALUES(rotation),
    updated_at = UTC_TIMESTAMP(6);

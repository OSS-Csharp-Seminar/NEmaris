USE NEmaris;

DROP PROCEDURE IF EXISTS add_table_guest_count;

DELIMITER //
CREATE PROCEDURE add_table_guest_count()
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'restaurant_tables'
          AND column_name = 'guest_count'
    ) THEN
        ALTER TABLE restaurant_tables
            ADD COLUMN guest_count INT NOT NULL DEFAULT 0 AFTER capacity;
    END IF;
END //
DELIMITER ;

CALL add_table_guest_count();
DROP PROCEDURE add_table_guest_count;

UPDATE restaurant_tables
SET guest_count = 1
WHERE guest_count = 0 AND status IN (1, 2);

USE NEmaris;

DROP PROCEDURE IF EXISTS add_menu_item_stock_quantity;

DELIMITER //
CREATE PROCEDURE add_menu_item_stock_quantity()
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'menu_items'
          AND column_name = 'stock_quantity'
    ) THEN
        ALTER TABLE menu_items
            ADD COLUMN stock_quantity INT NOT NULL DEFAULT 0 AFTER is_available;
    END IF;
END //
DELIMITER ;

CALL add_menu_item_stock_quantity();
DROP PROCEDURE add_menu_item_stock_quantity;

UPDATE menu_items
SET stock_quantity = CASE sku
    WHEN 'DRK-COL' THEN 24
    WHEN 'COF-CAP' THEN 30
    WHEN 'COF-ESP' THEN 40
    WHEN 'BRK-BEN' THEN 12
    WHEN 'LCH-WRP' THEN 16
    WHEN 'DIN-RUMP-WOK' THEN 8
    ELSE stock_quantity
END
WHERE sku IN ('DRK-COL', 'COF-CAP', 'COF-ESP', 'BRK-BEN', 'LCH-WRP', 'DIN-RUMP-WOK')
  AND stock_quantity = 0;

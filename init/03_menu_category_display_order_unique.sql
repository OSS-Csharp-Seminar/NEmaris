USE NEmaris;

CREATE TEMPORARY TABLE tmp_duplicate_menu_category_orders AS
SELECT
    ranked.id,
    bounds.max_display_order + ROW_NUMBER() OVER (ORDER BY ranked.display_order, ranked.id) AS new_display_order
FROM (
    SELECT
        id,
        display_order,
        ROW_NUMBER() OVER (PARTITION BY display_order ORDER BY id) AS duplicate_rank
    FROM menu_categories
) ranked
CROSS JOIN (
    SELECT COALESCE(MAX(display_order), 0) AS max_display_order
    FROM menu_categories
) bounds
WHERE ranked.duplicate_rank > 1;

UPDATE menu_categories category
JOIN tmp_duplicate_menu_category_orders duplicate_order
    ON category.id = duplicate_order.id
SET category.display_order = duplicate_order.new_display_order;

DROP TEMPORARY TABLE tmp_duplicate_menu_category_orders;

DROP PROCEDURE IF EXISTS create_uq_menu_categories_display_order;

DELIMITER //
CREATE PROCEDURE create_uq_menu_categories_display_order()
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.statistics
        WHERE table_schema = DATABASE()
          AND table_name = 'menu_categories'
          AND index_name = 'uq_menu_categories_display_order'
    ) THEN
        CREATE UNIQUE INDEX uq_menu_categories_display_order ON menu_categories(display_order);
    END IF;
END //
DELIMITER ;

CALL create_uq_menu_categories_display_order();
DROP PROCEDURE create_uq_menu_categories_display_order;

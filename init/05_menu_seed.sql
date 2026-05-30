USE NEmaris;

CREATE TEMPORARY TABLE tmp_menu_seed_categories (
    name VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
    description TEXT NULL,
    display_order INT NOT NULL,
    PRIMARY KEY (name)
);

INSERT INTO tmp_menu_seed_categories (name, description, display_order)
VALUES
    ('Drinks', NULL, 1),
    ('Breakfast', '08:00-12:00', 2),
    ('Lunch', '12:00-17:00', 3),
    ('Dinner', '18:00-22:30', 4);

CREATE TEMPORARY TABLE tmp_menu_category_order_conflicts AS
SELECT
    category.id,
    bounds.max_display_order + ROW_NUMBER() OVER (ORDER BY category.display_order, category.id) AS new_display_order
FROM menu_categories category
JOIN tmp_menu_seed_categories seed
    ON category.display_order = seed.display_order
CROSS JOIN (
    SELECT COALESCE(MAX(display_order), 0) AS max_display_order
    FROM menu_categories
) bounds
WHERE category.name <> seed.name;

UPDATE menu_categories category
JOIN tmp_menu_category_order_conflicts conflict
    ON category.id = conflict.id
SET
    category.display_order = conflict.new_display_order,
    category.updated_at = UTC_TIMESTAMP(6);

INSERT INTO menu_categories (name, description, display_order, created_at, updated_at)
SELECT
    seed.name,
    seed.description,
    seed.display_order,
    UTC_TIMESTAMP(6),
    UTC_TIMESTAMP(6)
FROM tmp_menu_seed_categories seed
ON DUPLICATE KEY UPDATE
    description = VALUES(description),
    display_order = VALUES(display_order),
    updated_at = UTC_TIMESTAMP(6);

CREATE TEMPORARY TABLE tmp_menu_seed_items (
    category_name VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
    name VARCHAR(150) COLLATE utf8mb4_unicode_ci NOT NULL,
    description TEXT NULL,
    price DECIMAL(10,2) NOT NULL,
    status INT NOT NULL,
    is_available TINYINT(1) NOT NULL,
    sku VARCHAR(50) COLLATE utf8mb4_unicode_ci NOT NULL,
    PRIMARY KEY (sku)
);

INSERT INTO tmp_menu_seed_items
    (category_name, name, description, price, status, is_available, sku)
VALUES
    ('Drinks', 'Coca-cola', NULL, 4.00, 1, 1, 'DRK-COL'),
    ('Drinks', 'Cappuccino', NULL, 2.90, 1, 1, 'COF-CAP'),
    ('Drinks', 'Espresso', NULL, 1.60, 1, 1, 'COF-ESP'),
    ('Breakfast', 'Eggs Benedict', 'whitebread toast, 2 poached eggs, hollandaise sauce', 7.50, 1, 1, 'BRK-BEN'),
    ('Lunch', 'Chicken Wrap', 'tortilla, grilled chicken, mayo, salad, tomatoes, beans', 8.00, 1, 1, 'LCH-WRP'),
    ('Dinner', 'Rumpsteak', '100% beef rumpsteak, rice, wok', 24.90, 1, 1, 'DIN-RUMP-WOK');

INSERT INTO menu_items
    (category_id, name, description, price, status, is_available, sku, created_at, updated_at)
SELECT
    category.id,
    seed.name,
    seed.description,
    seed.price,
    seed.status,
    seed.is_available,
    seed.sku,
    UTC_TIMESTAMP(6),
    UTC_TIMESTAMP(6)
FROM tmp_menu_seed_items seed
JOIN menu_categories category
    ON category.name = seed.category_name
ON DUPLICATE KEY UPDATE
    category_id = VALUES(category_id),
    name = VALUES(name),
    description = VALUES(description),
    price = VALUES(price),
    status = VALUES(status),
    is_available = VALUES(is_available),
    updated_at = UTC_TIMESTAMP(6);

DROP TEMPORARY TABLE tmp_menu_seed_items;
DROP TEMPORARY TABLE tmp_menu_category_order_conflicts;
DROP TEMPORARY TABLE tmp_menu_seed_categories;

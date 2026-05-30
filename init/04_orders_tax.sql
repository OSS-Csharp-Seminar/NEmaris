-- ============================================================
-- MIGRATION: add tax_rate and tax_amount to orders, and rebuild
-- order_items triggers so the totals reflect tax.
--
-- total_amount = GREATEST(subtotal - discount_amount, 0)
--              + ROUND(GREATEST(subtotal - discount_amount, 0) * tax_rate, 2)
-- tax_amount   = ROUND(GREATEST(subtotal - discount_amount, 0) * tax_rate, 2)
-- ============================================================

ALTER TABLE orders
    ADD COLUMN tax_rate   DECIMAL(5,4)  NOT NULL DEFAULT 0.0000 AFTER discount_amount,
    ADD COLUMN tax_amount DECIMAL(10,2) NOT NULL DEFAULT 0.00   AFTER tax_rate,
    ADD CONSTRAINT chk_orders_tax_rate   CHECK (tax_rate   >= 0 AND tax_rate <= 1),
    ADD CONSTRAINT chk_orders_tax_amount CHECK (tax_amount >= 0);

DROP TRIGGER IF EXISTS trg_order_items_after_insert;
DROP TRIGGER IF EXISTS trg_order_items_after_update;
DROP TRIGGER IF EXISTS trg_order_items_after_delete;

DELIMITER $$

CREATE TRIGGER trg_order_items_after_insert
AFTER INSERT ON order_items
FOR EACH ROW
BEGIN
    UPDATE orders o
    SET
        subtotal     = (SELECT COALESCE(SUM(line_total), 0.00) FROM order_items WHERE order_id = NEW.order_id),
        tax_amount   = ROUND(GREATEST(
                          (SELECT COALESCE(SUM(line_total), 0.00) FROM order_items WHERE order_id = NEW.order_id)
                          - o.discount_amount, 0.00) * o.tax_rate, 2),
        total_amount = GREATEST(
                          (SELECT COALESCE(SUM(line_total), 0.00) FROM order_items WHERE order_id = NEW.order_id)
                          - o.discount_amount, 0.00)
                       + ROUND(GREATEST(
                          (SELECT COALESCE(SUM(line_total), 0.00) FROM order_items WHERE order_id = NEW.order_id)
                          - o.discount_amount, 0.00) * o.tax_rate, 2)
    WHERE o.id = NEW.order_id;
END$$

CREATE TRIGGER trg_order_items_after_update
AFTER UPDATE ON order_items
FOR EACH ROW
BEGIN
    UPDATE orders o
    SET
        subtotal     = (SELECT COALESCE(SUM(line_total), 0.00) FROM order_items WHERE order_id = NEW.order_id),
        tax_amount   = ROUND(GREATEST(
                          (SELECT COALESCE(SUM(line_total), 0.00) FROM order_items WHERE order_id = NEW.order_id)
                          - o.discount_amount, 0.00) * o.tax_rate, 2),
        total_amount = GREATEST(
                          (SELECT COALESCE(SUM(line_total), 0.00) FROM order_items WHERE order_id = NEW.order_id)
                          - o.discount_amount, 0.00)
                       + ROUND(GREATEST(
                          (SELECT COALESCE(SUM(line_total), 0.00) FROM order_items WHERE order_id = NEW.order_id)
                          - o.discount_amount, 0.00) * o.tax_rate, 2)
    WHERE o.id = NEW.order_id;
END$$

CREATE TRIGGER trg_order_items_after_delete
AFTER DELETE ON order_items
FOR EACH ROW
BEGIN
    UPDATE orders o
    SET
        subtotal     = (SELECT COALESCE(SUM(line_total), 0.00) FROM order_items WHERE order_id = OLD.order_id),
        tax_amount   = ROUND(GREATEST(
                          (SELECT COALESCE(SUM(line_total), 0.00) FROM order_items WHERE order_id = OLD.order_id)
                          - o.discount_amount, 0.00) * o.tax_rate, 2),
        total_amount = GREATEST(
                          (SELECT COALESCE(SUM(line_total), 0.00) FROM order_items WHERE order_id = OLD.order_id)
                          - o.discount_amount, 0.00)
                       + ROUND(GREATEST(
                          (SELECT COALESCE(SUM(line_total), 0.00) FROM order_items WHERE order_id = OLD.order_id)
                          - o.discount_amount, 0.00) * o.tax_rate, 2)
    WHERE o.id = OLD.order_id;
END$$

DELIMITER ;

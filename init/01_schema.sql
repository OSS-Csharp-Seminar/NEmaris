-- ============================================================
-- NEmaris Database Schema
-- Built on ASP.NET Core Identity (MySQL / Pomelo provider)
-- MySQL 8.0+ | utf8mb4_unicode_ci throughout
--
-- Identity tables are created first, then all domain tables
-- that reference them via VARCHAR(255) FKs to AspNetUsers.Id
-- ============================================================

CREATE DATABASE IF NOT EXISTS NEmaris
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE NEmaris;

-- ============================================================
-- ASP.NET CORE IDENTITY TABLES
-- Matches the schema emitted by Pomelo.EntityFrameworkCore
-- .MySql when scaffolding with AddDefaultIdentity<IdentityUser>
-- Custom domain columns are added directly to AspNetUsers so
-- the application can access them via the same UserManager<T>.
-- ============================================================

CREATE TABLE AspNetUsers (
    Id                      VARCHAR(255)    NOT NULL,
    -- Domain extensions (mapped to your ApplicationUser model)
    FirstName               VARCHAR(100)    NOT NULL,
    LastName                VARCHAR(100)    NOT NULL,
    Phone                   VARCHAR(30)     NULL,
    Role                    INT             NOT NULL,
    Status                  INT             NOT NULL,
    LastLoginAt             DATETIME(6)     NULL,
    CreatedAt               DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt               DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    -- Standard Identity columns
    UserName                VARCHAR(256)    NULL,
    NormalizedUserName      VARCHAR(256)    NULL,
    Email                   VARCHAR(256)    NULL,
    NormalizedEmail         VARCHAR(256)    NULL,
    EmailConfirmed          TINYINT(1)      NOT NULL DEFAULT 0,
    PasswordHash            LONGTEXT        NULL,
    SecurityStamp           VARCHAR(255)    NULL,
    ConcurrencyStamp        VARCHAR(255)    NULL,
    PhoneNumber             VARCHAR(30)     NULL,
    PhoneNumberConfirmed    TINYINT(1)      NOT NULL DEFAULT 0,
    TwoFactorEnabled        TINYINT(1)      NOT NULL DEFAULT 0,
    LockoutEnd              DATETIME(6)     NULL,
    LockoutEnabled          TINYINT(1)      NOT NULL DEFAULT 0,
    AccessFailedCount       INT             NOT NULL DEFAULT 0,

    CONSTRAINT pk_AspNetUsers                    PRIMARY KEY (Id),
    CONSTRAINT uq_AspNetUsers_NormalizedUserName UNIQUE (NormalizedUserName),
    CONSTRAINT uq_AspNetUsers_NormalizedEmail    UNIQUE (NormalizedEmail)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE AspNetRoles (
    Id               VARCHAR(255)    NOT NULL,
    Name             VARCHAR(256)    NULL,
    NormalizedName   VARCHAR(256)    NULL,
    ConcurrencyStamp VARCHAR(255)    NULL,

    CONSTRAINT pk_AspNetRoles              PRIMARY KEY (Id),
    CONSTRAINT uq_AspNetRoles_NormalizedName UNIQUE (NormalizedName)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE AspNetUserRoles (
    UserId  VARCHAR(255)    NOT NULL,
    RoleId  VARCHAR(255)    NOT NULL,

    CONSTRAINT pk_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT fk_AspNetUserRoles_User
        FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
        ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_AspNetUserRoles_Role
        FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id)
        ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE AspNetUserClaims (
    Id          INT             NOT NULL AUTO_INCREMENT,
    UserId      VARCHAR(255)    NOT NULL,
    ClaimType   LONGTEXT        NULL,
    ClaimValue  LONGTEXT        NULL,

    CONSTRAINT pk_AspNetUserClaims PRIMARY KEY (Id),
    CONSTRAINT fk_AspNetUserClaims_User
        FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
        ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE AspNetRoleClaims (
    Id          INT             NOT NULL AUTO_INCREMENT,
    RoleId      VARCHAR(255)    NOT NULL,
    ClaimType   LONGTEXT        NULL,
    ClaimValue  LONGTEXT        NULL,

    CONSTRAINT pk_AspNetRoleClaims PRIMARY KEY (Id),
    CONSTRAINT fk_AspNetRoleClaims_Role
        FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id)
        ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE AspNetUserLogins (
    LoginProvider       VARCHAR(255)    NOT NULL,
    ProviderKey         VARCHAR(255)    NOT NULL,
    ProviderDisplayName LONGTEXT        NULL,
    UserId              VARCHAR(255)    NOT NULL,

    CONSTRAINT pk_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT fk_AspNetUserLogins_User
        FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
        ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE AspNetUserTokens (
    UserId          VARCHAR(255)    NOT NULL,
    LoginProvider   VARCHAR(255)    NOT NULL,
    Name            VARCHAR(255)    NOT NULL,
    Value           LONGTEXT        NULL,

    CONSTRAINT pk_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT fk_AspNetUserTokens_User
        FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
        ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- ============================================================
-- TABLE: menu_categories
-- ============================================================
CREATE TABLE menu_categories (
    id              BIGINT          NOT NULL AUTO_INCREMENT,
    name            VARCHAR(100)    NOT NULL,
    description     TEXT            NULL,
    display_order   INT             NOT NULL DEFAULT 0,
    created_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),

    CONSTRAINT pk_menu_categories      PRIMARY KEY (id),
    CONSTRAINT uq_menu_categories_name UNIQUE (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- ============================================================
-- TABLE: menu_items
-- ============================================================
CREATE TABLE menu_items (
    id              BIGINT          NOT NULL AUTO_INCREMENT,
    category_id     BIGINT          NOT NULL,
    name            VARCHAR(150)    NOT NULL,
    description     TEXT            NULL,
    price           DECIMAL(10,2)   NOT NULL,
    status          INT             NOT NULL,
    is_available    TINYINT(1)      NOT NULL DEFAULT 1,
    sku             VARCHAR(50)     NULL,
    created_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),

    CONSTRAINT pk_menu_items          PRIMARY KEY (id),
    CONSTRAINT uq_menu_items_sku      UNIQUE (sku),
    CONSTRAINT fk_menu_items_category
        FOREIGN KEY (category_id) REFERENCES menu_categories(id)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT chk_menu_items_price   CHECK (price >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- ============================================================
-- TABLE: restaurant_tables
-- ============================================================
CREATE TABLE restaurant_tables (
    id              BIGINT          NOT NULL AUTO_INCREMENT,
    table_number    VARCHAR(20)     NOT NULL,
    capacity        INT             NOT NULL,
    zone            VARCHAR(100)    NULL,
    status          INT             NOT NULL,
    created_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),

    CONSTRAINT pk_restaurant_tables        PRIMARY KEY (id),
    CONSTRAINT uq_restaurant_tables_number UNIQUE (table_number),
    CONSTRAINT chk_restaurant_tables_cap   CHECK (capacity > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- ============================================================
-- TABLE: reservations
-- guest_id              → customer who made the reservation
-- reserved_by_user_id   → staff who logged it (NULL = online /
--                          self-service booking)
-- reservation_date      → the date on which the guest created
--                          this reservation record
-- ============================================================
CREATE TABLE reservations (
    id                      BIGINT          NOT NULL AUTO_INCREMENT,
    guest_id                VARCHAR(255)    NOT NULL,
    table_id                BIGINT          NOT NULL,
    reserved_by_user_id     VARCHAR(255)    NULL,
    reservation_date        DATE            NOT NULL,
    start_time              DATETIME(6)     NOT NULL,
    end_time                DATETIME(6)     NOT NULL,
    party_size              INT             NOT NULL,
    status                  INT             NOT NULL,
    special_request         TEXT            NULL,
    created_at              DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at              DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),

    CONSTRAINT pk_reservations           PRIMARY KEY (id),
    CONSTRAINT fk_reservations_guest
        FOREIGN KEY (guest_id) REFERENCES AspNetUsers(Id)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_reservations_table
        FOREIGN KEY (table_id) REFERENCES restaurant_tables(id)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_reservations_reserved_by
        FOREIGN KEY (reserved_by_user_id) REFERENCES AspNetUsers(Id)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT chk_reservations_party    CHECK (party_size > 0),
    CONSTRAINT chk_reservations_times    CHECK (end_time > start_time)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- ============================================================
-- TABLE: orders
-- Financial columns (subtotal, total_amount) are maintained
-- automatically by the triggers defined below.
-- discount_amount is set by the application layer.
-- ============================================================
CREATE TABLE orders (
    id                  BIGINT          NOT NULL AUTO_INCREMENT,
    order_number        VARCHAR(50)     NOT NULL,
    table_id            BIGINT          NOT NULL,
    waiter_user_id      VARCHAR(255)    NOT NULL,
    guest_id            VARCHAR(255)    NULL,
    reservation_id      BIGINT          NULL,
    status              INT             NOT NULL,
    payment_status      INT             NOT NULL,
    subtotal            DECIMAL(10,2)   NOT NULL DEFAULT 0.00,
    discount_amount     DECIMAL(10,2)   NOT NULL DEFAULT 0.00,
    total_amount        DECIMAL(10,2)   NOT NULL DEFAULT 0.00,
    notes               TEXT            NULL,
    opened_at           DATETIME(6)     NOT NULL,
    closed_at           DATETIME(6)     NULL,
    created_at          DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at          DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),

    CONSTRAINT pk_orders               PRIMARY KEY (id),
    CONSTRAINT uq_orders_order_number  UNIQUE (order_number),
    CONSTRAINT fk_orders_table
        FOREIGN KEY (table_id) REFERENCES restaurant_tables(id)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_orders_waiter
        FOREIGN KEY (waiter_user_id) REFERENCES AspNetUsers(Id)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_orders_guest
        FOREIGN KEY (guest_id) REFERENCES AspNetUsers(Id)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT fk_orders_reservation
        FOREIGN KEY (reservation_id) REFERENCES reservations(id)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT chk_orders_subtotal     CHECK (subtotal >= 0),
    CONSTRAINT chk_orders_discount     CHECK (discount_amount >= 0),
    CONSTRAINT chk_orders_total        CHECK (total_amount >= 0),
    CONSTRAINT chk_orders_closed       CHECK (closed_at IS NULL OR closed_at >= opened_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- ============================================================
-- TABLE: order_items
-- unit_price is snapshotted at insertion time so that future
-- price changes on menu_items never affect historical orders.
-- line_total = quantity * unit_price (enforced by application
-- before insert; triggers then roll it up into orders).
-- ============================================================
CREATE TABLE order_items (
    id              BIGINT          NOT NULL AUTO_INCREMENT,
    order_id        BIGINT          NOT NULL,
    menu_item_id    BIGINT          NOT NULL,
    quantity        INT             NOT NULL,
    unit_price      DECIMAL(10,2)   NOT NULL,
    line_total      DECIMAL(10,2)   NOT NULL,
    note            TEXT            NULL,
    created_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at      DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),

    CONSTRAINT pk_order_items          PRIMARY KEY (id),
    CONSTRAINT fk_order_items_order
        FOREIGN KEY (order_id) REFERENCES orders(id)
        ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_order_items_menu_item
        FOREIGN KEY (menu_item_id) REFERENCES menu_items(id)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT chk_order_items_qty     CHECK (quantity > 0),
    CONSTRAINT chk_order_items_price   CHECK (unit_price >= 0),
    CONSTRAINT chk_order_items_total   CHECK (line_total >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- ============================================================
-- TABLE: payments
-- Intentionally has no updated_at — payments are immutable.
-- ============================================================
CREATE TABLE payments (
    id                  BIGINT          NOT NULL AUTO_INCREMENT,
    order_id            BIGINT          NOT NULL,
    payment_method      INT             NOT NULL,
    amount              DECIMAL(10,2)   NOT NULL,
    reference_number    VARCHAR(100)    NOT NULL,
    paid_at             DATETIME(6)     NOT NULL,
    created_at          DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6),

    CONSTRAINT pk_payments                     PRIMARY KEY (id),
    CONSTRAINT uq_payments_reference_number    UNIQUE (reference_number),
    CONSTRAINT fk_payments_order
        FOREIGN KEY (order_id) REFERENCES orders(id)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT chk_payments_amount             CHECK (amount > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- ============================================================
-- TRIGGERS
-- Recalculate orders.subtotal and orders.total_amount after
-- any change to order_items. discount_amount is owned by the
-- application; triggers read it but never write it.
-- total_amount = subtotal - discount_amount (floored at 0
-- to guard against a discount that exceeds the subtotal).
-- ============================================================

DELIMITER $$

CREATE TRIGGER trg_order_items_after_insert
AFTER INSERT ON order_items
FOR EACH ROW
BEGIN
    UPDATE orders
    SET
        subtotal     = (
            SELECT COALESCE(SUM(line_total), 0.00)
            FROM order_items
            WHERE order_id = NEW.order_id
        ),
        total_amount = GREATEST(
            (SELECT COALESCE(SUM(line_total), 0.00) FROM order_items WHERE order_id = NEW.order_id)
            - discount_amount,
            0.00
        ),
        updated_at   = CURRENT_TIMESTAMP(6)
    WHERE id = NEW.order_id;
END$$


CREATE TRIGGER trg_order_items_after_update
AFTER UPDATE ON order_items
FOR EACH ROW
BEGIN
    UPDATE orders
    SET
        subtotal     = (
            SELECT COALESCE(SUM(line_total), 0.00)
            FROM order_items
            WHERE order_id = NEW.order_id
        ),
        total_amount = GREATEST(
            (SELECT COALESCE(SUM(line_total), 0.00) FROM order_items WHERE order_id = NEW.order_id)
            - discount_amount,
            0.00
        ),
        updated_at   = CURRENT_TIMESTAMP(6)
    WHERE id = NEW.order_id;
END$$


CREATE TRIGGER trg_order_items_after_delete
AFTER DELETE ON order_items
FOR EACH ROW
BEGIN
    UPDATE orders
    SET
        subtotal     = (
            SELECT COALESCE(SUM(line_total), 0.00)
            FROM order_items
            WHERE order_id = OLD.order_id
        ),
        total_amount = GREATEST(
            (SELECT COALESCE(SUM(line_total), 0.00) FROM order_items WHERE order_id = OLD.order_id)
            - discount_amount,
            0.00
        ),
        updated_at   = CURRENT_TIMESTAMP(6)
    WHERE id = OLD.order_id;
END$$

DELIMITER ;


-- ============================================================
-- INDEXES
-- InnoDB auto-indexes all FK columns; the following cover
-- additional query and reporting patterns.
-- ============================================================

-- AspNetUsers domain filters
CREATE INDEX idx_users_role_status    ON AspNetUsers(Role, Status);
CREATE INDEX idx_users_last_login     ON AspNetUsers(LastLoginAt);

-- menu_items availability lookup
CREATE INDEX idx_menu_items_cat_avail ON menu_items(category_id, is_available, status);

-- restaurant_tables floor status
CREATE INDEX idx_tables_status        ON restaurant_tables(status);

-- reservations scheduling queries
CREATE INDEX idx_res_table_start      ON reservations(table_id, start_time);
CREATE INDEX idx_res_guest            ON reservations(guest_id);
CREATE INDEX idx_res_date             ON reservations(reservation_date);

-- orders dashboard / reporting
CREATE INDEX idx_orders_status        ON orders(status, payment_status);
CREATE INDEX idx_orders_waiter        ON orders(waiter_user_id);
CREATE INDEX idx_orders_guest         ON orders(guest_id);
CREATE INDEX idx_orders_reservation   ON orders(reservation_id);
CREATE INDEX idx_orders_opened_at     ON orders(opened_at);

-- order_items menu item usage / reporting
CREATE INDEX idx_order_items_menu     ON order_items(menu_item_id);

-- payments timeline
CREATE INDEX idx_payments_paid_at     ON payments(paid_at);

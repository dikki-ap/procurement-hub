-- ============================================================
-- ProcureHub — Initial Seed Data
-- ============================================================
-- Run AFTER migrations have been applied.
-- Safe to run multiple times — all INSERTs use IGNORE.
-- If the C# startup seed has already run, this script is a no-op
-- (all unique keys already exist, so every INSERT is skipped).
--
-- Usage:
--   mysql -u root -p procurehub < scripts/seed.sql
--
-- Or inside MySQL/MariaDB shell:
--   SOURCE /path/to/scripts/seed.sql;
-- ============================================================

SET NAMES utf8mb4;
SET @now = UTC_TIMESTAMP(6);

-- ── Company ──────────────────────────────────────────────────────────────────
INSERT IGNORE INTO companies (id, name, code, type, is_active, created_at, updated_at)
VALUES (
    '11111111-1111-1111-1111-111111111111',
    'ProcureHub Manufacturing Co.', 'PRCH', 'Internal', 1,
    @now, @now
);

-- Capture the real company ID regardless of which seed ran first
SET @company_id = (SELECT id FROM companies WHERE code = 'PRCH' LIMIT 1);

-- ── Currencies ───────────────────────────────────────────────────────────────
INSERT IGNORE INTO currencies (id, code, name, symbol, exchange_rate, is_base, created_at, updated_at) VALUES
('cc000000-0000-0000-0000-000000000001', 'IDR', 'Indonesian Rupiah', 'Rp', 1.000000,     1, @now, @now),
('cc000000-0000-0000-0000-000000000002', 'USD', 'US Dollar',         '$',  16000.000000, 0, @now, @now),
('cc000000-0000-0000-0000-000000000003', 'EUR', 'Euro',              '€',  17500.000000, 0, @now, @now),
('cc000000-0000-0000-0000-000000000004', 'SGD', 'Singapore Dollar',  'S$', 12000.000000, 0, @now, @now);

-- ── Units of Measure ─────────────────────────────────────────────────────────
INSERT IGNORE INTO units_of_measure (id, company_id, code, name, created_at, updated_at) VALUES
('uu000000-0000-0000-0000-000000000001', @company_id, 'KG',   'Kilogram', @now, @now),
('uu000000-0000-0000-0000-000000000002', @company_id, 'TON',  'Ton',      @now, @now),
('uu000000-0000-0000-0000-000000000003', @company_id, 'PCS',  'Pieces',   @now, @now),
('uu000000-0000-0000-0000-000000000004', @company_id, 'LTR',  'Liter',    @now, @now),
('uu000000-0000-0000-0000-000000000005', @company_id, 'MTR',  'Meter',    @now, @now),
('uu000000-0000-0000-0000-000000000006', @company_id, 'BOX',  'Box',      @now, @now),
('uu000000-0000-0000-0000-000000000007', @company_id, 'SET',  'Set',      @now, @now),
('uu000000-0000-0000-0000-000000000008', @company_id, 'UNIT', 'Unit',     @now, @now);

-- ── Payment Terms ────────────────────────────────────────────────────────────
INSERT IGNORE INTO payment_terms (id, company_id, code, name, days, description, created_at, updated_at) VALUES
('pt000000-0000-0000-0000-000000000001', @company_id, 'COD',   'Cash on Delivery', 0,  'Payment upon delivery',              @now, @now),
('pt000000-0000-0000-0000-000000000002', @company_id, 'NET7',  'Net 7 Days',       7,  NULL,                                 @now, @now),
('pt000000-0000-0000-0000-000000000003', @company_id, 'NET14', 'Net 14 Days',      14, NULL,                                 @now, @now),
('pt000000-0000-0000-0000-000000000004', @company_id, 'NET30', 'Net 30 Days',      30, NULL,                                 @now, @now),
('pt000000-0000-0000-0000-000000000005', @company_id, 'NET60', 'Net 60 Days',      60, NULL,                                 @now, @now),
('pt000000-0000-0000-0000-000000000006', @company_id, 'DP50',  'Down Payment 50%', 30, '50% upfront, remainder on delivery', @now, @now),
('pt000000-0000-0000-0000-000000000007', @company_id, 'DP30',  'Down Payment 30%', 30, '30% upfront, remainder on delivery', @now, @now);

-- ── Material Categories ───────────────────────────────────────────────────────
INSERT IGNORE INTO material_categories (id, company_id, code, name, is_strategic, created_at, updated_at) VALUES
('mc000000-0000-0000-0000-000000000001', @company_id, 'RM',    'Raw Material',         1, @now, @now),
('mc000000-0000-0000-0000-000000000002', @company_id, 'SP',    'Spare Parts',          0, @now, @now),
('mc000000-0000-0000-0000-000000000003', @company_id, 'MRO',   'Maintenance & Repair', 0, @now, @now),
('mc000000-0000-0000-0000-000000000004', @company_id, 'CAPEX', 'Capital Expenditure',  1, @now, @now),
('mc000000-0000-0000-0000-000000000005', @company_id, 'PKG',   'Packaging',            0, @now, @now),
('mc000000-0000-0000-0000-000000000006', @company_id, 'CONS',  'Consumables',          0, @now, @now);

-- ── Locations ─────────────────────────────────────────────────────────────────
INSERT IGNORE INTO locations (id, company_id, name, type, city, country, created_at, updated_at) VALUES
('lo000000-0000-0000-0000-000000000001', @company_id, 'Main Warehouse',     'warehouse', 'Jakarta',      'Indonesia', @now, @now),
('lo000000-0000-0000-0000-000000000002', @company_id, 'Production Plant A', 'plant',     'Cikarang',     'Indonesia', @now, @now),
('lo000000-0000-0000-0000-000000000003', @company_id, 'Head Office',        'office',    'South Jakarta', 'Indonesia', @now, @now);

-- ── Approval Policies ─────────────────────────────────────────────────────────
-- is_strategic_override = 1  → Medium-value docs flagged as strategic item get +1 level
-- is_single_source_override = 1 → Medium-value docs flagged as single source get +1 level
--
-- PR/PO Low    (0 - 50M IDR)    → 1 approval level
-- PR/PO Medium (50M - 500M IDR) → 2 levels; strategic or single-source → 3 levels
-- PR/PO High   (> 500M IDR)     → 3 levels (already maximum)
INSERT IGNORE INTO approval_policies
    (id, company_id, reference_type, name, min_value, max_value,
     required_levels, is_strategic_override, is_single_source_override, is_active,
     created_at, updated_at)
VALUES
-- PR
('ap000000-0000-0000-0000-000000000001', @company_id, 'PR', 'PR Low Value (< 50M)',        0,         49999999.9999,  1, 0, 0, 1, @now, @now),
('ap000000-0000-0000-0000-000000000002', @company_id, 'PR', 'PR Medium Value (50M - 500M)',50000000,  499999999.9999, 2, 1, 1, 1, @now, @now),
('ap000000-0000-0000-0000-000000000003', @company_id, 'PR', 'PR High Value (> 500M)',      500000000, NULL,           3, 0, 0, 1, @now, @now),
-- PO
('ap000000-0000-0000-0000-000000000004', @company_id, 'PO', 'PO Low Value (< 50M)',        0,         49999999.9999,  1, 0, 0, 1, @now, @now),
('ap000000-0000-0000-0000-000000000005', @company_id, 'PO', 'PO Medium Value (50M - 500M)',50000000,  499999999.9999, 2, 1, 1, 1, @now, @now),
('ap000000-0000-0000-0000-000000000006', @company_id, 'PO', 'PO High Value (> 500M)',      500000000, NULL,           3, 0, 0, 1, @now, @now);

-- ── Done ──────────────────────────────────────────────────────────────────────
SELECT 'Seed completed.' AS status;

-- ============================================================
-- ProcureHub — Initial Seed Data
-- ============================================================
-- Run AFTER migrations have been applied.
-- Safe to run multiple times — all INSERTs use IGNORE.
-- If the C# startup seed has already run, this script is a no-op
-- (all unique keys already exist, so every INSERT is skipped).
--
-- "Duplicate entry" messages = warnings (data already existed, row skipped). OK.
-- No actual errors should appear after this fix.
--
-- Usage:
--   mysql -u root -p procurehub < scripts/seed.sql
--
-- Or inside MySQL/MariaDB shell:
--   SOURCE /path/to/scripts/seed.sql;
-- ============================================================

SET NAMES utf8mb4;
SET @now = UTC_TIMESTAMP(6);

-- ── Clean up rows with invalid (non-hex) UUID prefixes from previous seed ────
-- These rows cannot be read by EF Core (Guid.Parse fails on non-hex chars).
DELETE FROM document_types      WHERE id LIKE 'dt%';
DELETE FROM approval_policies   WHERE id LIKE 'ap%';
DELETE FROM locations           WHERE id LIKE 'lo%';
DELETE FROM material_categories WHERE id LIKE 'mc%';
DELETE FROM payment_terms       WHERE id LIKE 'pt%';
DELETE FROM unit_of_measures    WHERE id LIKE 'uu%';

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
INSERT IGNORE INTO currencies (id, code, name, symbol, exchange_rate, is_base, is_active, created_at, updated_at) VALUES
('cc000000-0000-0000-0000-000000000001', 'IDR', 'Indonesian Rupiah', 'Rp', 1.000000,     1, 1, @now, @now),
('cc000000-0000-0000-0000-000000000002', 'USD', 'US Dollar',         '$',  16000.000000, 0, 1, @now, @now),
('cc000000-0000-0000-0000-000000000003', 'EUR', 'Euro',              '€',  17500.000000, 0, 1, @now, @now),
('cc000000-0000-0000-0000-000000000004', 'SGD', 'Singapore Dollar',  'S$', 12000.000000, 0, 1, @now, @now);

-- ── Units of Measure ─────────────────────────────────────────────────────────
-- Prefix ee = valid hex (e=14, e=14)
INSERT IGNORE INTO unit_of_measures (id, company_id, code, name, is_active, created_at, updated_at) VALUES
('ee000000-0000-0000-0000-000000000001', @company_id, 'KG',   'Kilogram', 1, @now, @now),
('ee000000-0000-0000-0000-000000000002', @company_id, 'TON',  'Ton',      1, @now, @now),
('ee000000-0000-0000-0000-000000000003', @company_id, 'PCS',  'Pieces',   1, @now, @now),
('ee000000-0000-0000-0000-000000000004', @company_id, 'LTR',  'Liter',    1, @now, @now),
('ee000000-0000-0000-0000-000000000005', @company_id, 'MTR',  'Meter',    1, @now, @now),
('ee000000-0000-0000-0000-000000000006', @company_id, 'BOX',  'Box',      1, @now, @now),
('ee000000-0000-0000-0000-000000000007', @company_id, 'SET',  'Set',      1, @now, @now),
('ee000000-0000-0000-0000-000000000008', @company_id, 'UNIT', 'Unit',     1, @now, @now);

-- ── Payment Terms ────────────────────────────────────────────────────────────
-- Prefix b0 = valid hex (b=11, 0=0)
INSERT IGNORE INTO payment_terms (id, company_id, code, name, days, description, is_active, created_at, updated_at) VALUES
('b0000000-0000-0000-0000-000000000001', @company_id, 'COD',   'Cash on Delivery', 0,  'Payment upon delivery',              1, @now, @now),
('b0000000-0000-0000-0000-000000000002', @company_id, 'NET7',  'Net 7 Days',       7,  NULL,                                 1, @now, @now),
('b0000000-0000-0000-0000-000000000003', @company_id, 'NET14', 'Net 14 Days',      14, NULL,                                 1, @now, @now),
('b0000000-0000-0000-0000-000000000004', @company_id, 'NET30', 'Net 30 Days',      30, NULL,                                 1, @now, @now),
('b0000000-0000-0000-0000-000000000005', @company_id, 'NET60', 'Net 60 Days',      60, NULL,                                 1, @now, @now),
('b0000000-0000-0000-0000-000000000006', @company_id, 'DP50',  'Down Payment 50%', 30, '50% upfront, remainder on delivery', 1, @now, @now),
('b0000000-0000-0000-0000-000000000007', @company_id, 'DP30',  'Down Payment 30%', 30, '30% upfront, remainder on delivery', 1, @now, @now);

-- ── Material Categories ───────────────────────────────────────────────────────
-- Prefix bc = valid hex (b=11, c=12)
INSERT IGNORE INTO material_categories (id, company_id, code, name, is_strategic, is_active, created_at, updated_at) VALUES
('bc000000-0000-0000-0000-000000000001', @company_id, 'RM',    'Raw Material',         1, 1, @now, @now),
('bc000000-0000-0000-0000-000000000002', @company_id, 'SP',    'Spare Parts',          0, 1, @now, @now),
('bc000000-0000-0000-0000-000000000003', @company_id, 'MRO',   'Maintenance & Repair', 0, 1, @now, @now),
('bc000000-0000-0000-0000-000000000004', @company_id, 'CAPEX', 'Capital Expenditure',  1, 1, @now, @now),
('bc000000-0000-0000-0000-000000000005', @company_id, 'PKG',   'Packaging',            0, 1, @now, @now),
('bc000000-0000-0000-0000-000000000006', @company_id, 'CONS',  'Consumables',          0, 1, @now, @now);

-- ── Locations ─────────────────────────────────────────────────────────────────
-- Prefix f0 = valid hex (f=15, 0=0)
INSERT IGNORE INTO locations (id, company_id, name, type, city, country, is_active, created_at, updated_at) VALUES
('f0000000-0000-0000-0000-000000000001', @company_id, 'Main Warehouse',     'warehouse', 'Jakarta',       'Indonesia', 1, @now, @now),
('f0000000-0000-0000-0000-000000000002', @company_id, 'Production Plant A', 'plant',     'Cikarang',      'Indonesia', 1, @now, @now),
('f0000000-0000-0000-0000-000000000003', @company_id, 'Head Office',        'office',    'South Jakarta', 'Indonesia', 1, @now, @now);

-- ── Approval Policies ─────────────────────────────────────────────────────────
-- is_strategic_override = 1  → Medium-value docs flagged as strategic item get +1 level
-- is_single_source_override = 1 → Medium-value docs flagged as single source get +1 level
--
-- PR/PO Low    (0 - 50M IDR)    → 1 approval level
-- PR/PO Medium (50M - 500M IDR) → 2 levels; strategic or single-source → 3 levels
-- PR/PO High   (> 500M IDR)     → 3 levels (already maximum)
--
-- Prefix ab = valid hex (a=10, b=11)
INSERT IGNORE INTO approval_policies
    (id, company_id, reference_type, name, min_value, max_value,
     required_levels, is_strategic_override, is_single_source_override, is_active,
     created_at, updated_at)
VALUES
-- PR
('ab000000-0000-0000-0000-000000000001', @company_id, 'PR', 'PR Low Value (< 50M)',         0,         49999999.9999,  1, 0, 0, 1, @now, @now),
('ab000000-0000-0000-0000-000000000002', @company_id, 'PR', 'PR Medium Value (50M - 500M)', 50000000,  499999999.9999, 2, 1, 1, 1, @now, @now),
('ab000000-0000-0000-0000-000000000003', @company_id, 'PR', 'PR High Value (> 500M)',       500000000, NULL,           3, 0, 0, 1, @now, @now),
-- PO
('ab000000-0000-0000-0000-000000000004', @company_id, 'PO', 'PO Low Value (< 50M)',         0,         49999999.9999,  1, 0, 0, 1, @now, @now),
('ab000000-0000-0000-0000-000000000005', @company_id, 'PO', 'PO Medium Value (50M - 500M)', 50000000,  499999999.9999, 2, 1, 1, 1, @now, @now),
('ab000000-0000-0000-0000-000000000006', @company_id, 'PO', 'PO High Value (> 500M)',       500000000, NULL,           3, 0, 0, 1, @now, @now);

-- ── Document Types ───────────────────────────────────────────────────────────
-- allowed_extensions: comma-separated (null = all globally allowed types)
-- max_file_size_mb: integer 1-100 (default 10)
-- Prefix d0 = valid hex (d=13, 0=0)
INSERT IGNORE INTO document_types (id, name, is_active, allowed_extensions, max_file_size_mb, created_at, updated_at) VALUES
('d0000000-0000-0000-0000-000000000001', 'SIUP',    1, '.pdf,.jpg,.jpeg,.png', 10, @now, @now),
('d0000000-0000-0000-0000-000000000002', 'NPWP',    1, '.pdf,.jpg,.jpeg,.png', 5,  @now, @now),
('d0000000-0000-0000-0000-000000000003', 'NIB',     1, '.pdf,.jpg,.jpeg,.png', 5,  @now, @now),
('d0000000-0000-0000-0000-000000000004', 'ISO 9001',1, '.pdf',                10, @now, @now),
('d0000000-0000-0000-0000-000000000005', 'HALAL',   1, '.pdf',                10, @now, @now),
('d0000000-0000-0000-0000-000000000006', 'AKTA',    1, '.pdf,.jpg,.jpeg,.png', 10, @now, @now),
('d0000000-0000-0000-0000-000000000007', 'OTHER',   1, NULL,                  10, @now, @now);

-- ── Materials ─────────────────────────────────────────────────────────────────
-- Prefix aa = valid hex (a=10, a=10)
-- Covers: various categories, UoMs, currencies, strategic/non-strategic, active/inactive
DELETE FROM materials WHERE id LIKE 'aa%';
INSERT IGNORE INTO materials (id, category_id, code, name, description, uom_id, estimated_price, currency_id, is_strategic, is_active, is_deleted, created_at, updated_at) VALUES
-- Raw Materials (strategic)
('aa000000-0000-0000-0000-000000000001', 'bc000000-0000-0000-0000-000000000001', 'STL-001', 'Steel Sheet 2mm',         'Cold-rolled steel sheet thickness 2mm',        'ee000000-0000-0000-0000-000000000001', 18500.00,    'cc000000-0000-0000-0000-000000000001', 1, 1, 0, @now, @now),
('aa000000-0000-0000-0000-000000000002', 'bc000000-0000-0000-0000-000000000001', 'ALM-001', 'Aluminum Ingot 6061',     'Aluminum alloy 6061-T6 ingot',                 'ee000000-0000-0000-0000-000000000001', 2.85,        'cc000000-0000-0000-0000-000000000002', 1, 1, 0, @now, @now),
('aa000000-0000-0000-0000-000000000003', 'bc000000-0000-0000-0000-000000000001', 'CST-001', 'Carbon Steel Rod 20mm',  'Carbon steel round bar diameter 20mm',         'ee000000-0000-0000-0000-000000000005', 42000.00,    'cc000000-0000-0000-0000-000000000001', 1, 1, 0, @now, @now),
-- Spare Parts (non-strategic)
('aa000000-0000-0000-0000-000000000004', 'bc000000-0000-0000-0000-000000000002', 'BRN-001', 'Bearing SKF 6205-2RS',   'Deep groove ball bearing, sealed both sides',  'ee000000-0000-0000-0000-000000000003', 85000.00,    'cc000000-0000-0000-0000-000000000001', 0, 1, 0, @now, @now),
('aa000000-0000-0000-0000-000000000005', 'bc000000-0000-0000-0000-000000000002', 'VBL-001', 'V-Belt A-50',            'Classical V-belt section A length 50 inch',    'ee000000-0000-0000-0000-000000000003', 35000.00,    'cc000000-0000-0000-0000-000000000001', 0, 1, 0, @now, @now),
-- MRO (non-strategic)
('aa000000-0000-0000-0000-000000000006', 'bc000000-0000-0000-0000-000000000003', 'LUB-001', 'Lubricant Oil SAE 40',   'Engine lubricant oil SAE 40 grade',            'ee000000-0000-0000-0000-000000000004', 45000.00,    'cc000000-0000-0000-0000-000000000001', 0, 1, 0, @now, @now),
('aa000000-0000-0000-0000-000000000007', 'bc000000-0000-0000-0000-000000000003', 'WLD-001', 'Welding Electrode E6013', 'Mild steel welding electrode 3.2mm x 350mm',   'ee000000-0000-0000-0000-000000000001', 28000.00,    'cc000000-0000-0000-0000-000000000001', 0, 1, 0, @now, @now),
-- CAPEX (strategic)
('aa000000-0000-0000-0000-000000000008', 'bc000000-0000-0000-0000-000000000004', 'CNC-001', 'CNC Milling Machine',   '3-axis CNC vertical machining center',         'ee000000-0000-0000-0000-000000000008', 45000.00,    'cc000000-0000-0000-0000-000000000002', 1, 1, 0, @now, @now),
-- Packaging (non-strategic)
('aa000000-0000-0000-0000-000000000009', 'bc000000-0000-0000-0000-000000000005', 'BOX-001', 'Cardboard Box 40x30x20', 'Single-wall corrugated box 40×30×20 cm',       'ee000000-0000-0000-0000-000000000003', 3500.00,     'cc000000-0000-0000-0000-000000000001', 0, 1, 0, @now, @now),
-- Consumables (inactive — for testing inactive filter)
('aa000000-0000-0000-0000-000000000010', 'bc000000-0000-0000-0000-000000000006', 'HMR-001', 'Hammer 500g',            'Steel claw hammer 500g with fiberglass handle', 'ee000000-0000-0000-0000-000000000003', 75000.00,    'cc000000-0000-0000-0000-000000000001', 0, 0, 0, @now, @now);

-- ── Done ──────────────────────────────────────────────────────────────────────
SELECT 'Seed completed.' AS status;

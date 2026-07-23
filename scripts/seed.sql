-- ============================================================
-- ProcureHub — Initial Seed Data
-- ============================================================
-- Run AFTER migrations have been applied.
-- Safe to run multiple times — all INSERTs use IGNORE.
--
-- Usage:
--   mysql -u root -p procurehub < scripts/seed.sql
--
-- Or inside a MySQL/MariaDB shell:
--   SOURCE /path/to/scripts/seed.sql;
--
-- KEYCLOAK NOTE:
--   The keycloak_id values for users and vendor_users below are
--   placeholder UUIDs. After importing the Keycloak realm, update
--   them to match the real Keycloak user UUIDs:
--     UPDATE users SET keycloak_id = '<real-kc-uuid>' WHERE email = '<email>';
--     UPDATE vendor_users SET keycloak_id = '<real-kc-uuid>' WHERE email = '<email>';
-- ============================================================

SET NAMES utf8mb4;
SET @now = UTC_TIMESTAMP(6);

-- ── Clean up rows with invalid UUID prefixes from any older seed ──────────────
DELETE FROM document_types      WHERE id LIKE 'dt%';
DELETE FROM approval_policies   WHERE id LIKE 'ap%';
DELETE FROM locations           WHERE id LIKE 'lo%';
DELETE FROM material_categories WHERE id LIKE 'mc%';
DELETE FROM payment_terms       WHERE id LIKE 'pt%';
DELETE FROM unit_of_measures    WHERE id LIKE 'uu%';

-- ============================================================
-- 1. COMPANY
-- ============================================================
-- Update name/code/address here to match the client's actual company.
INSERT IGNORE INTO companies (id, name, code, type, address, phone, email, is_active, created_at, updated_at)
VALUES (
    '11111111-1111-1111-1111-111111111111',
    'PT. Nexcore Industries', 'NCI', 'Internal',
    'Jl. Industri Raya No. 88, Kawasan Industri Cikarang Barat, Bekasi 17520',
    '+62-21-8901-2345',
    'procurement@nexcore-industries.com',
    1, @now, @now
);

SET @company_id = (SELECT id FROM companies WHERE code = 'NCI' LIMIT 1);

-- ============================================================
-- 2. CURRENCIES
-- ============================================================
INSERT IGNORE INTO currencies (id, code, name, symbol, exchange_rate, is_base, is_active, created_at, updated_at) VALUES
('cc000000-0000-0000-0000-000000000001', 'IDR', 'Indonesian Rupiah', 'Rp',     1.000000,  1, 1, @now, @now),
('cc000000-0000-0000-0000-000000000002', 'USD', 'US Dollar',         '$',  16000.000000,  0, 1, @now, @now),
('cc000000-0000-0000-0000-000000000003', 'EUR', 'Euro',              '€',  17500.000000,  0, 1, @now, @now),
('cc000000-0000-0000-0000-000000000004', 'SGD', 'Singapore Dollar',  'S$', 12000.000000,  0, 1, @now, @now),
('cc000000-0000-0000-0000-000000000005', 'JPY', 'Japanese Yen',      '¥',    107.000000,  0, 1, @now, @now);

SET @cur_idr = (SELECT id FROM currencies WHERE code = 'IDR' LIMIT 1);
SET @cur_usd = (SELECT id FROM currencies WHERE code = 'USD' LIMIT 1);

-- ============================================================
-- 3. UNITS OF MEASURE
-- ============================================================
INSERT IGNORE INTO unit_of_measures (id, company_id, code, name, is_active, created_at, updated_at) VALUES
('ee000000-0000-0000-0000-000000000001', @company_id, 'KG',   'Kilogram', 1, @now, @now),
('ee000000-0000-0000-0000-000000000002', @company_id, 'TON',  'Ton',      1, @now, @now),
('ee000000-0000-0000-0000-000000000003', @company_id, 'PCS',  'Pieces',   1, @now, @now),
('ee000000-0000-0000-0000-000000000004', @company_id, 'LTR',  'Liter',    1, @now, @now),
('ee000000-0000-0000-0000-000000000005', @company_id, 'MTR',  'Meter',    1, @now, @now),
('ee000000-0000-0000-0000-000000000006', @company_id, 'BOX',  'Box',      1, @now, @now),
('ee000000-0000-0000-0000-000000000007', @company_id, 'SET',  'Set',      1, @now, @now),
('ee000000-0000-0000-0000-000000000008', @company_id, 'UNIT', 'Unit',     1, @now, @now),
('ee000000-0000-0000-0000-000000000009', @company_id, 'ROLL', 'Roll',     1, @now, @now),
('ee000000-0000-0000-0000-000000000010', @company_id, 'DRUM', 'Drum',     1, @now, @now);

SET @uom_kg   = (SELECT id FROM unit_of_measures WHERE code = 'KG'   AND company_id = @company_id LIMIT 1);
SET @uom_ton  = (SELECT id FROM unit_of_measures WHERE code = 'TON'  AND company_id = @company_id LIMIT 1);
SET @uom_pcs  = (SELECT id FROM unit_of_measures WHERE code = 'PCS'  AND company_id = @company_id LIMIT 1);
SET @uom_ltr  = (SELECT id FROM unit_of_measures WHERE code = 'LTR'  AND company_id = @company_id LIMIT 1);
SET @uom_mtr  = (SELECT id FROM unit_of_measures WHERE code = 'MTR'  AND company_id = @company_id LIMIT 1);
SET @uom_unit = (SELECT id FROM unit_of_measures WHERE code = 'UNIT' AND company_id = @company_id LIMIT 1);
SET @uom_drum = (SELECT id FROM unit_of_measures WHERE code = 'DRUM' AND company_id = @company_id LIMIT 1);
SET @uom_roll = (SELECT id FROM unit_of_measures WHERE code = 'ROLL' AND company_id = @company_id LIMIT 1);

-- ============================================================
-- 4. PAYMENT TERMS
-- ============================================================
INSERT IGNORE INTO payment_terms (id, company_id, code, name, days, description, is_active, created_at, updated_at) VALUES
('b0000000-0000-0000-0000-000000000001', @company_id, 'COD',   'Cash on Delivery',  0,  'Payment upon delivery',               1, @now, @now),
('b0000000-0000-0000-0000-000000000002', @company_id, 'NET7',  'Net 7 Days',        7,  NULL,                                  1, @now, @now),
('b0000000-0000-0000-0000-000000000003', @company_id, 'NET14', 'Net 14 Days',       14, NULL,                                  1, @now, @now),
('b0000000-0000-0000-0000-000000000004', @company_id, 'NET30', 'Net 30 Days',       30, NULL,                                  1, @now, @now),
('b0000000-0000-0000-0000-000000000005', @company_id, 'NET45', 'Net 45 Days',       45, NULL,                                  1, @now, @now),
('b0000000-0000-0000-0000-000000000006', @company_id, 'NET60', 'Net 60 Days',       60, NULL,                                  1, @now, @now),
('b0000000-0000-0000-0000-000000000007', @company_id, 'DP50',  'Down Payment 50%',  30, '50% upfront, remainder on delivery',  1, @now, @now),
('b0000000-0000-0000-0000-000000000008', @company_id, 'DP30',  'Down Payment 30%',  30, '30% upfront, remainder on delivery',  1, @now, @now);

SET @pt_net30 = (SELECT id FROM payment_terms WHERE code = 'NET30' AND company_id = @company_id LIMIT 1);
SET @pt_net14 = (SELECT id FROM payment_terms WHERE code = 'NET14' AND company_id = @company_id LIMIT 1);
SET @pt_dp50  = (SELECT id FROM payment_terms WHERE code = 'DP50'  AND company_id = @company_id LIMIT 1);
SET @pt_cod   = (SELECT id FROM payment_terms WHERE code = 'COD'   AND company_id = @company_id LIMIT 1);

-- ============================================================
-- 5. MATERIAL CATEGORIES
-- ============================================================
INSERT IGNORE INTO material_categories (id, company_id, code, name, is_strategic, is_active, created_at, updated_at) VALUES
('bc000000-0000-0000-0000-000000000001', @company_id, 'RM',    'Raw Material',            1, 1, @now, @now),
('bc000000-0000-0000-0000-000000000002', @company_id, 'SP',    'Spare Parts',             0, 1, @now, @now),
('bc000000-0000-0000-0000-000000000003', @company_id, 'MRO',   'Maintenance & Repair',    0, 1, @now, @now),
('bc000000-0000-0000-0000-000000000004', @company_id, 'CAPEX', 'Capital Expenditure',     1, 1, @now, @now),
('bc000000-0000-0000-0000-000000000005', @company_id, 'PKG',   'Packaging',               0, 1, @now, @now),
('bc000000-0000-0000-0000-000000000006', @company_id, 'CONS',  'Consumables',             0, 1, @now, @now),
('bc000000-0000-0000-0000-000000000007', @company_id, 'CHEM',  'Chemical & Lubricants',   0, 1, @now, @now),
('bc000000-0000-0000-0000-000000000008', @company_id, 'ELEC',  'Electrical & Instrument', 0, 1, @now, @now);

SET @cat_rm    = (SELECT id FROM material_categories WHERE code = 'RM'    AND company_id = @company_id LIMIT 1);
SET @cat_sp    = (SELECT id FROM material_categories WHERE code = 'SP'    AND company_id = @company_id LIMIT 1);
SET @cat_mro   = (SELECT id FROM material_categories WHERE code = 'MRO'   AND company_id = @company_id LIMIT 1);
SET @cat_capex = (SELECT id FROM material_categories WHERE code = 'CAPEX' AND company_id = @company_id LIMIT 1);
SET @cat_pkg   = (SELECT id FROM material_categories WHERE code = 'PKG'   AND company_id = @company_id LIMIT 1);
SET @cat_cons  = (SELECT id FROM material_categories WHERE code = 'CONS'  AND company_id = @company_id LIMIT 1);
SET @cat_chem  = (SELECT id FROM material_categories WHERE code = 'CHEM'  AND company_id = @company_id LIMIT 1);
SET @cat_elec  = (SELECT id FROM material_categories WHERE code = 'ELEC'  AND company_id = @company_id LIMIT 1);

-- ============================================================
-- 6. LOCATIONS
-- ============================================================
INSERT IGNORE INTO locations (id, company_id, name, type, city, country, is_active, created_at, updated_at) VALUES
('f0000000-0000-0000-0000-000000000001', @company_id, 'Main Warehouse Cikarang',  'warehouse', 'Cikarang',         'Indonesia', 1, @now, @now),
('f0000000-0000-0000-0000-000000000002', @company_id, 'Production Plant A',       'plant',     'Cikarang',         'Indonesia', 1, @now, @now),
('f0000000-0000-0000-0000-000000000003', @company_id, 'Production Plant B',       'plant',     'Karawang',         'Indonesia', 1, @now, @now),
('f0000000-0000-0000-0000-000000000004', @company_id, 'Head Office Jakarta',      'office',    'Jakarta Selatan',  'Indonesia', 1, @now, @now),
('f0000000-0000-0000-0000-000000000005', @company_id, 'Transit Warehouse Sby',   'warehouse', 'Surabaya',         'Indonesia', 1, @now, @now);

-- ============================================================
-- 7. APPROVAL POLICIES
-- ============================================================
-- Value tiers (in IDR):
--   Low    (< 50 M)    → 1 approval level
--   Medium (50M–500M)  → 2 levels; strategic or single-source items → 3 levels
--   High   (> 500M)    → 3 levels (always maximum)
INSERT IGNORE INTO approval_policies
    (id, company_id, reference_type, name, min_value, max_value,
     required_levels, is_strategic_override, is_single_source_override, is_active,
     created_at, updated_at)
VALUES
-- PR
('ab000000-0000-0000-0000-000000000001', @company_id, 'PR', 'PR Low Value (< IDR 50M)',        0,         49999999.9999,  1, 0, 0, 1, @now, @now),
('ab000000-0000-0000-0000-000000000002', @company_id, 'PR', 'PR Medium Value (IDR 50M–500M)',  50000000,  499999999.9999, 2, 1, 1, 1, @now, @now),
('ab000000-0000-0000-0000-000000000003', @company_id, 'PR', 'PR High Value (> IDR 500M)',      500000000, NULL,           3, 0, 0, 1, @now, @now),
-- PO
('ab000000-0000-0000-0000-000000000004', @company_id, 'PO', 'PO Low Value (< IDR 50M)',        0,         49999999.9999,  1, 0, 0, 1, @now, @now),
('ab000000-0000-0000-0000-000000000005', @company_id, 'PO', 'PO Medium Value (IDR 50M–500M)',  50000000,  499999999.9999, 2, 1, 1, 1, @now, @now),
('ab000000-0000-0000-0000-000000000006', @company_id, 'PO', 'PO High Value (> IDR 500M)',      500000000, NULL,           3, 0, 0, 1, @now, @now);

-- ============================================================
-- 8. DOCUMENT TYPES
-- ============================================================
INSERT IGNORE INTO document_types (id, name, is_active, allowed_extensions, max_file_size_mb, created_at, updated_at) VALUES
('d0000000-0000-0000-0000-000000000001', 'SIUP',              1, '.pdf,.jpg,.jpeg,.png', 10, @now, @now),
('d0000000-0000-0000-0000-000000000002', 'NPWP',              1, '.pdf,.jpg,.jpeg,.png',  5, @now, @now),
('d0000000-0000-0000-0000-000000000003', 'NIB',               1, '.pdf,.jpg,.jpeg,.png',  5, @now, @now),
('d0000000-0000-0000-0000-000000000004', 'ISO 9001',          1, '.pdf',                 10, @now, @now),
('d0000000-0000-0000-0000-000000000005', 'Halal Certificate', 1, '.pdf',                 10, @now, @now),
('d0000000-0000-0000-0000-000000000006', 'Deed of Establishment', 1, '.pdf,.jpg,.jpeg,.png', 10, @now, @now),
('d0000000-0000-0000-0000-000000000007', 'SPPKP (VAT Registration)', 1, '.pdf,.jpg,.jpeg,.png', 5, @now, @now),
('d0000000-0000-0000-0000-000000000008', 'Other',             1, NULL,                   10, @now, @now);

-- ============================================================
-- 9. DEPARTMENTS
-- ============================================================
INSERT IGNORE INTO departments (id, company_id, code, name, is_active, created_at, updated_at) VALUES
('de000000-0000-0000-0000-000000000001', @company_id, 'PROC', 'Procurement',              1, @now, @now),
('de000000-0000-0000-0000-000000000002', @company_id, 'PROD', 'Production',               1, @now, @now),
('de000000-0000-0000-0000-000000000003', @company_id, 'ENG',  'Engineering & Maintenance',1, @now, @now),
('de000000-0000-0000-0000-000000000004', @company_id, 'FIN',  'Finance & Accounting',     1, @now, @now),
('de000000-0000-0000-0000-000000000005', @company_id, 'MGMT', 'Management',               1, @now, @now),
('de000000-0000-0000-0000-000000000006', @company_id, 'IT',   'IT & Systems',             1, @now, @now);

SET @dept_proc = (SELECT id FROM departments WHERE code = 'PROC' AND company_id = @company_id LIMIT 1);
SET @dept_prod = (SELECT id FROM departments WHERE code = 'PROD' AND company_id = @company_id LIMIT 1);
SET @dept_eng  = (SELECT id FROM departments WHERE code = 'ENG'  AND company_id = @company_id LIMIT 1);
SET @dept_fin  = (SELECT id FROM departments WHERE code = 'FIN'  AND company_id = @company_id LIMIT 1);
SET @dept_mgmt = (SELECT id FROM departments WHERE code = 'MGMT' AND company_id = @company_id LIMIT 1);
SET @dept_it   = (SELECT id FROM departments WHERE code = 'IT'   AND company_id = @company_id LIMIT 1);

-- ============================================================
-- 10. INTERNAL USERS
-- ============================================================
-- keycloak_id must match the actual user UUID from Keycloak after realm setup.
-- Placeholder UUIDs are used here — update them post-Keycloak configuration.
INSERT IGNORE INTO users
    (id, company_id, keycloak_id, email, full_name, role, department_id, is_active, created_at, updated_at)
VALUES
-- super_admin
('fa000000-0000-0000-0000-000000000001', @company_id,
 'fa000000-kc00-0000-0000-000000000001',
 'admin@nexcore-industries.com', 'System Administrator', 'super_admin',
 @dept_it, 1, @now, @now),

-- purchasing team (2 buyers)
('fa000000-0000-0000-0000-000000000002', @company_id,
 'fa000000-kc00-0000-0000-000000000002',
 'james.carter@nexcore-industries.com', 'James Carter', 'purchasing',
 @dept_proc, 1, @now, @now),

('fa000000-0000-0000-0000-000000000003', @company_id,
 'fa000000-kc00-0000-0000-000000000003',
 'sarah.mitchell@nexcore-industries.com', 'Sarah Mitchell', 'purchasing',
 @dept_proc, 1, @now, @now),

-- requesters (production + engineering)
('fa000000-0000-0000-0000-000000000004', @company_id,
 'fa000000-kc00-0000-0000-000000000004',
 'michael.davis@nexcore-industries.com', 'Michael Davis', 'requester',
 @dept_prod, 1, @now, @now),

('fa000000-0000-0000-0000-000000000005', @company_id,
 'fa000000-kc00-0000-0000-000000000005',
 'emily.chen@nexcore-industries.com', 'Emily Chen', 'requester',
 @dept_eng, 1, @now, @now),

-- approvers (L1 manager, L2 VP, L3 director)
('fa000000-0000-0000-0000-000000000006', @company_id,
 'fa000000-kc00-0000-0000-000000000006',
 'robert.hayes@nexcore-industries.com', 'Robert Hayes', 'approver',
 @dept_proc, 1, @now, @now),

('fa000000-0000-0000-0000-000000000007', @company_id,
 'fa000000-kc00-0000-0000-000000000007',
 'william.foster@nexcore-industries.com', 'William Foster', 'approver',
 @dept_mgmt, 1, @now, @now),

('fa000000-0000-0000-0000-000000000008', @company_id,
 'fa000000-kc00-0000-0000-000000000008',
 'director@nexcore-industries.com', 'David Harrison', 'approver',
 @dept_mgmt, 1, @now, @now),

-- finance
('fa000000-0000-0000-0000-000000000009', @company_id,
 'fa000000-kc00-0000-0000-000000000009',
 'jennifer.walsh@nexcore-industries.com', 'Jennifer Walsh', 'finance',
 @dept_fin, 1, @now, @now),

-- management (read-only analytics)
('fa000000-0000-0000-0000-000000000010', @company_id,
 'fa000000-kc00-0000-0000-000000000010',
 'cfo@nexcore-industries.com', 'Thomas Reynolds', 'management',
 @dept_mgmt, 1, @now, @now);

-- ============================================================
-- 11. APPROVER MATRIX ENTRIES
-- ============================================================
-- Maps each approval level to an actual person.
-- L1 = Procurement Manager, L2 = VP Operations, L3 = Director
INSERT IGNORE INTO approver_matrix_entries
    (id, company_id, reference_type, level, name, position, email, created_at, updated_at)
VALUES
-- PR approvers
('a1000000-0000-0000-0000-000000000001', @company_id, 'PR', 1,
 'Robert Hayes',    'Procurement Manager', 'robert.hayes@nexcore-industries.com',    @now, @now),
('a1000000-0000-0000-0000-000000000002', @company_id, 'PR', 2,
 'William Foster',  'VP Operations',       'william.foster@nexcore-industries.com',  @now, @now),
('a1000000-0000-0000-0000-000000000003', @company_id, 'PR', 3,
 'David Harrison',  'President Director',  'director@nexcore-industries.com',        @now, @now),
-- PO approvers
('a1000000-0000-0000-0000-000000000004', @company_id, 'PO', 1,
 'Robert Hayes',    'Procurement Manager', 'robert.hayes@nexcore-industries.com',    @now, @now),
('a1000000-0000-0000-0000-000000000005', @company_id, 'PO', 2,
 'William Foster',  'VP Operations',       'william.foster@nexcore-industries.com',  @now, @now),
('a1000000-0000-0000-0000-000000000006', @company_id, 'PO', 3,
 'David Harrison',  'President Director',  'director@nexcore-industries.com',        @now, @now);

-- ============================================================
-- 12. VENDORS
-- ============================================================
-- Mix of cases to cover all possible system states:
--   VND-001  Active · Gold      · Manufacturer  · PKP + PPh 2%   (steel supplier, ISO certified)
--   VND-002  Active · Silver    · Distributor   · PKP, no PPh    (spare parts distributor)
--   VND-003  Active · Bronze    · Manufacturer  · non-PKP        (MRO/consumables, small local)
--   VND-004  Active · Gold      · Manufacturer  · PKP + PPh 4%   (capital equipment, import dealer)
--   VND-005  Active · Probation · Trader        · non-PKP        (new vendor, low score, on watch)
--   VND-006  Suspended · Silver · Distributor   · PKP            (failed internal quality audit)
--   VND-007  Active · Bronze    · Manufacturer  · non-PKP        (local packaging manufacturer)
--   VND-008  Blacklisted · Bronze · Trader      · non-PKP        (proven invoice fraud)

INSERT IGNORE INTO vendors
    (id, company_id, vendor_code, legal_name, trade_name, npwp, siup, nib,
     address, city, province, postal_code, country,
     vendor_type, status, tier, score, is_blacklisted, blacklist_reason,
     is_pkp, pph_rate,
     default_payment_term_id, default_currency_id,
     is_deleted, created_at, updated_at)
VALUES

-- VND-001: Active, Gold, steel manufacturer, PKP + PPh 2%, ISO 9001
('e1000000-0000-0000-0000-000000000001', @company_id,
 'VND-2026-000001', 'PT. Baja Nusantara Sejahtera', 'Baja Nusantara',
 '01.234.567.8-901.000', 'SIUP-12345/2020', '1234567890123456',
 'Jl. Baja Raya No. 12, Kawasan Industri MM2100', 'Cikarang', 'West Java', '17520', 'Indonesia',
 'Manufacturer', 'Active', 'Gold', 88.50, 0, NULL,
 1, 2.00, @pt_net30, @cur_idr,
 0, @now, @now),

-- VND-002: Active, Silver, spare parts distributor, PKP (no PPh)
('e1000000-0000-0000-0000-000000000002', @company_id,
 'VND-2026-000002', 'PT. Maju Bersama Teknik', 'MBT Teknik',
 '02.345.678.9-012.000', 'SIUP-23456/2019', '2345678901234567',
 'Jl. Raya Bekasi KM 28 No. 45', 'Bekasi', 'West Java', '17142', 'Indonesia',
 'Distributor', 'Active', 'Silver', 74.20, 0, NULL,
 1, NULL, @pt_net14, @cur_idr,
 0, @now, @now),

-- VND-003: Active, Bronze, MRO/consumables manufacturer, non-PKP, small local business
('e1000000-0000-0000-0000-000000000003', @company_id,
 'VND-2026-000003', 'CV. Karya Mandiri Sejahtera', 'KMS',
 '03.456.789.0-123.000', 'SIUP-34567/2021', '3456789012345678',
 'Jl. Industri Selatan No. 7, Blok C-12', 'Tangerang', 'Banten', '15810', 'Indonesia',
 'Manufacturer', 'Active', 'Bronze', 61.00, 0, NULL,
 0, NULL, @pt_cod, @cur_idr,
 0, @now, @now),

-- VND-004: Active, Gold, capital equipment (imported machinery), PKP + PPh 4%, USD transactions
('e1000000-0000-0000-0000-000000000004', @company_id,
 'VND-2026-000004', 'PT. Teknologi Mesin Canggih', 'TMC Machinery',
 '04.567.890.1-234.000', 'SIUP-45678/2018', '4567890123456789',
 'Jl. Gatot Subroto Kav. 51-53', 'South Jakarta', 'DKI Jakarta', '12950', 'Indonesia',
 'Manufacturer', 'Active', 'Gold', 91.80, 0, NULL,
 1, 4.00, @pt_dp50, @cur_usd,
 0, @now, @now),

-- VND-005: Active, Probation (new vendor, score < 60, under watch period)
('e1000000-0000-0000-0000-000000000005', @company_id,
 'VND-2026-000005', 'CV. Alam Plastik Persada', 'Alam Plastik',
 '05.678.901.2-345.000', 'SIUP-56789/2025', '5678901234567890',
 'Jl. Raya Serpong No. 99, Blok B5', 'South Tangerang', 'Banten', '15310', 'Indonesia',
 'Trader', 'Active', 'Probation', 52.00, 0, NULL,
 0, NULL, @pt_net14, @cur_idr,
 0, @now, @now),

-- VND-006: Suspended (failed internal quality audit — deliveries consistently late)
('e1000000-0000-0000-0000-000000000006', @company_id,
 'VND-2026-000006', 'PT. Indo Kimia Utama', 'IKU Chemical',
 '06.789.012.3-456.000', 'SIUP-67890/2017', '6789012345678901',
 'Jl. Panjang No. 5, Kebon Jeruk', 'West Jakarta', 'DKI Jakarta', '11530', 'Indonesia',
 'Distributor', 'Suspended', 'Silver', 58.30, 0, NULL,
 1, NULL, @pt_net30, @cur_idr,
 0, @now, @now),

-- VND-007: Active, Bronze, local packaging manufacturer
('e1000000-0000-0000-0000-000000000007', @company_id,
 'VND-2026-000007', 'CV. Sarana Kemasan Prima', 'SKP Packaging',
 '07.890.123.4-567.000', 'SIUP-78901/2022', '7890123456789012',
 'Jl. Industri Kemasan No. 33, Cibitung', 'Bekasi', 'West Java', '17520', 'Indonesia',
 'Manufacturer', 'Active', 'Bronze', 65.40, 0, NULL,
 0, NULL, @pt_net14, @cur_idr,
 0, @now, @now),

-- VND-008: Blacklisted (proven invoice manipulation fraud — management decision March 2025)
('e1000000-0000-0000-0000-000000000008', @company_id,
 'VND-2026-000008', 'PT. Bumi Logistik Andalan', 'BLA Logistics',
 '08.901.234.5-678.000', 'SIUP-89012/2016', '8901234567890123',
 'Jl. Raya Pos No. 17, Cakung', 'East Jakarta', 'DKI Jakarta', '13960', 'Indonesia',
 'Trader', 'Blacklisted', 'Bronze', 28.00, 1,
 'Confirmed invoice and delivery document fraud on transaction PO-2025-000412. Blacklisted by management decision dated 10 March 2025.',
 0, NULL, @pt_cod, @cur_idr,
 0, @now, @now);

-- ============================================================
-- 13. VENDOR USERS
-- ============================================================
-- 2 active users per active vendor (admin + staff).
-- Suspended and blacklisted vendors have their users deactivated (is_active = 0).
INSERT IGNORE INTO vendor_users
    (id, vendor_id, keycloak_id, email, full_name, role, is_active, created_at, updated_at)
VALUES
-- VND-001 Baja Nusantara Sejahtera
('e2000000-0000-0000-0000-000000000001', 'e1000000-0000-0000-0000-000000000001',
 'e2000000-kc00-0000-0000-000000000001',
 'admin@baja-nusantara.co.id', 'Antonius Wibowo', 'vendor_admin', 1, @now, @now),
('e2000000-0000-0000-0000-000000000002', 'e1000000-0000-0000-0000-000000000001',
 'e2000000-kc00-0000-0000-000000000002',
 'sales@baja-nusantara.co.id', 'Mega Pertiwi', 'vendor_staff', 1, @now, @now),

-- VND-002 MBT Teknik
('e2000000-0000-0000-0000-000000000003', 'e1000000-0000-0000-0000-000000000002',
 'e2000000-kc00-0000-0000-000000000003',
 'admin@mbt-teknik.com', 'Rudy Hartono', 'vendor_admin', 1, @now, @now),
('e2000000-0000-0000-0000-000000000004', 'e1000000-0000-0000-0000-000000000002',
 'e2000000-kc00-0000-0000-000000000004',
 'bid@mbt-teknik.com', 'Lina Suhartini', 'vendor_staff', 1, @now, @now),

-- VND-003 KMS
('e2000000-0000-0000-0000-000000000005', 'e1000000-0000-0000-0000-000000000003',
 'e2000000-kc00-0000-0000-000000000005',
 'owner@kms-mandiri.co.id', 'Haryanto', 'vendor_admin', 1, @now, @now),
('e2000000-0000-0000-0000-000000000006', 'e1000000-0000-0000-0000-000000000003',
 'e2000000-kc00-0000-0000-000000000006',
 'staff@kms-mandiri.co.id', 'Sulastri', 'vendor_staff', 1, @now, @now),

-- VND-004 TMC Machinery
('e2000000-0000-0000-0000-000000000007', 'e1000000-0000-0000-0000-000000000004',
 'e2000000-kc00-0000-0000-000000000007',
 'admin@tmc-machinery.co.id', 'Felix Kurniawan', 'vendor_admin', 1, @now, @now),
('e2000000-0000-0000-0000-000000000008', 'e1000000-0000-0000-0000-000000000004',
 'e2000000-kc00-0000-0000-000000000008',
 'tender@tmc-machinery.co.id', 'Jessica Tanujaya', 'vendor_staff', 1, @now, @now),

-- VND-005 Alam Plastik (Probation — 1 user only)
('e2000000-0000-0000-0000-000000000009', 'e1000000-0000-0000-0000-000000000005',
 'e2000000-kc00-0000-0000-000000000009',
 'owner@alam-plastik.co.id', 'Wahyudi Saputra', 'vendor_admin', 1, @now, @now),

-- VND-006 IKU Chemical (Suspended — user deactivated)
('e2000000-0000-0000-0000-000000000010', 'e1000000-0000-0000-0000-000000000006',
 'e2000000-kc00-0000-0000-000000000010',
 'admin@iku-chemical.co.id', 'Bambang Irawan', 'vendor_admin', 0, @now, @now),

-- VND-007 SKP Packaging
('e2000000-0000-0000-0000-000000000011', 'e1000000-0000-0000-0000-000000000007',
 'e2000000-kc00-0000-0000-000000000011',
 'admin@skp-packaging.co.id', 'Dini Rahayu', 'vendor_admin', 1, @now, @now),
('e2000000-0000-0000-0000-000000000012', 'e1000000-0000-0000-0000-000000000007',
 'e2000000-kc00-0000-0000-000000000012',
 'sales@skp-packaging.co.id', 'Tono Pratama', 'vendor_staff', 1, @now, @now),

-- VND-008 BLA Logistics (Blacklisted — user deactivated)
('e2000000-0000-0000-0000-000000000013', 'e1000000-0000-0000-0000-000000000008',
 'e2000000-kc00-0000-0000-000000000013',
 'admin@bla-logistics.co.id', 'Slamet Riyadi', 'vendor_admin', 0, @now, @now);

-- ============================================================
-- 14. VENDOR CONTACTS (PIC per vendor)
-- ============================================================
INSERT IGNORE INTO vendor_contacts
    (id, vendor_id, name, position, email, phone, is_primary, created_at, updated_at)
VALUES
('e3000000-0000-0000-0000-000000000001', 'e1000000-0000-0000-0000-000000000001',
 'Antonius Wibowo',  'Sales Manager',        'antonius@baja-nusantara.co.id', '+62-812-1001-0001', 1, @now, @now),
('e3000000-0000-0000-0000-000000000002', 'e1000000-0000-0000-0000-000000000001',
 'Mega Pertiwi',     'Sales Representative', 'mega@baja-nusantara.co.id',     '+62-813-1002-0002', 0, @now, @now),

('e3000000-0000-0000-0000-000000000003', 'e1000000-0000-0000-0000-000000000002',
 'Rudy Hartono',     'Director',             'rudy@mbt-teknik.com',           '+62-821-2001-0003', 1, @now, @now),
('e3000000-0000-0000-0000-000000000004', 'e1000000-0000-0000-0000-000000000002',
 'Lina Suhartini',   'Account Manager',      'lina@mbt-teknik.com',           '+62-822-2002-0004', 0, @now, @now),

('e3000000-0000-0000-0000-000000000005', 'e1000000-0000-0000-0000-000000000003',
 'Haryanto',         'Owner',                'haryanto@kms-mandiri.co.id',    '+62-831-3001-0005', 1, @now, @now),

('e3000000-0000-0000-0000-000000000006', 'e1000000-0000-0000-0000-000000000004',
 'Felix Kurniawan',  'Business Dev Manager', 'felix@tmc-machinery.co.id',     '+62-811-4001-0006', 1, @now, @now),
('e3000000-0000-0000-0000-000000000007', 'e1000000-0000-0000-0000-000000000004',
 'Jessica Tanujaya', 'Tender Coordinator',   'jessica@tmc-machinery.co.id',   '+62-818-4002-0007', 0, @now, @now),

('e3000000-0000-0000-0000-000000000008', 'e1000000-0000-0000-0000-000000000005',
 'Wahyudi Saputra',  'Owner',                'wahyudi@alam-plastik.co.id',    '+62-851-5001-0008', 1, @now, @now),

('e3000000-0000-0000-0000-000000000009', 'e1000000-0000-0000-0000-000000000007',
 'Dini Rahayu',      'Sales Admin',          'dini@skp-packaging.co.id',      '+62-857-7001-0009', 1, @now, @now);

-- ============================================================
-- 15. VENDOR BANK ACCOUNTS
-- ============================================================
INSERT IGNORE INTO vendor_bank_accounts
    (id, vendor_id, bank_name, account_number, account_name, branch_name, currency, is_default, created_at, updated_at)
VALUES
-- Baja Nusantara — IDR (default) + USD for export transactions
('e4000000-0000-0000-0000-000000000001', 'e1000000-0000-0000-0000-000000000001',
 'Bank BCA', '123-456-7890', 'PT. Baja Nusantara Sejahtera', 'KCP Cikarang', 'IDR', 1, @now, @now),
('e4000000-0000-0000-0000-000000000002', 'e1000000-0000-0000-0000-000000000001',
 'Bank Mandiri', '140-000-1234567', 'PT. Baja Nusantara Sejahtera', 'Branch Bekasi', 'USD', 0, @now, @now),

-- MBT Teknik
('e4000000-0000-0000-0000-000000000003', 'e1000000-0000-0000-0000-000000000002',
 'Bank BRI', '0123-01-001234-56-7', 'PT. Maju Bersama Teknik', 'KCP Bekasi Kota', 'IDR', 1, @now, @now),

-- KMS
('e4000000-0000-0000-0000-000000000004', 'e1000000-0000-0000-0000-000000000003',
 'Bank BNI', '0123456789', 'CV. Karya Mandiri Sejahtera', 'Branch Tangerang', 'IDR', 1, @now, @now),

-- TMC Machinery — IDR (default) + USD for imported machinery payments
('e4000000-0000-0000-0000-000000000005', 'e1000000-0000-0000-0000-000000000004',
 'Bank Mandiri', '103-000-9876543', 'PT. Teknologi Mesin Canggih', 'Branch South Jakarta', 'IDR', 1, @now, @now),
('e4000000-0000-0000-0000-000000000006', 'e1000000-0000-0000-0000-000000000004',
 'Bank BCA', '456-789-0123', 'PT. Teknologi Mesin Canggih', 'KCP Gatot Subroto', 'USD', 0, @now, @now),

-- Alam Plastik (Probation)
('e4000000-0000-0000-0000-000000000007', 'e1000000-0000-0000-0000-000000000005',
 'Bank BCA', '789-012-3456', 'CV. Alam Plastik Persada', 'KCP BSD City', 'IDR', 1, @now, @now),

-- SKP Packaging
('e4000000-0000-0000-0000-000000000008', 'e1000000-0000-0000-0000-000000000007',
 'Bank BRI', '0456-01-004567-89-0', 'CV. Sarana Kemasan Prima', 'KCP Cibitung', 'IDR', 1, @now, @now);

-- ============================================================
-- 16. VENDOR CAPABILITIES
-- ============================================================
-- Each active vendor has 1–3 supply category capabilities.
-- One capability is intentionally set as expired to demonstrate the expiry feature.
INSERT IGNORE INTO vendor_capabilities
    (id, vendor_id, material_category_id,
     min_order_qty, max_order_qty, uom, lead_time_days,
     effective_date, expiry_date, is_expired, notes,
     created_at, updated_at)
VALUES
-- Baja Nusantara: Raw Material + Spare Parts (ISO 9001 certified)
('e5000000-0000-0000-0000-000000000001', 'e1000000-0000-0000-0000-000000000001', @cat_rm,
 500.0000, 50000.0000, 'KG', 7,
 '2025-01-01', '2027-12-31', 0, 'Steel sheet, plate, coil. Minimum order 500 kg.',
 @now, @now),
('e5000000-0000-0000-0000-000000000002', 'e1000000-0000-0000-0000-000000000001', @cat_sp,
 10.0000, 5000.0000, 'PCS', 14,
 '2025-01-01', '2027-12-31', 0, 'Steel fabricated spare parts, custom order accepted.',
 @now, @now),

-- MBT Teknik: Spare Parts + MRO
('e5000000-0000-0000-0000-000000000003', 'e1000000-0000-0000-0000-000000000002', @cat_sp,
 1.0000, 10000.0000, 'PCS', 3,
 '2024-06-01', '2026-05-31', 0, 'Bearings, v-belts, seals, gaskets — all major brands stocked.',
 @now, @now),
('e5000000-0000-0000-0000-000000000004', 'e1000000-0000-0000-0000-000000000002', @cat_mro,
 1.0000, 5000.0000, 'PCS', 5,
 '2024-06-01', '2026-05-31', 0, 'Hand tools, maintenance supplies.',
 @now, @now),

-- KMS: MRO + Consumables
('e5000000-0000-0000-0000-000000000005', 'e1000000-0000-0000-0000-000000000003', @cat_mro,
 10.0000, 2000.0000, 'KG', 7,
 '2025-03-01', '2026-02-28', 0, 'Welding electrodes, grinding wheels, PPE.',
 @now, @now),
('e5000000-0000-0000-0000-000000000006', 'e1000000-0000-0000-0000-000000000003', @cat_cons,
 5.0000, 500.0000, 'UNIT', 3,
 '2025-03-01', '2026-02-28', 0, 'Consumable tools and workshop supplies.',
 @now, @now),

-- TMC Machinery: CAPEX (expired — renewal in progress) + Electrical
('e5000000-0000-0000-0000-000000000007', 'e1000000-0000-0000-0000-000000000004', @cat_capex,
 1.0000, 50.0000, 'UNIT', 90,
 '2023-01-01', '2025-12-31', 1, 'CNC machining centers, lathes, press machines. [EXPIRED — renewal in progress]',
 @now, @now),
('e5000000-0000-0000-0000-000000000008', 'e1000000-0000-0000-0000-000000000004', @cat_elec,
 1.0000, 200.0000, 'UNIT', 30,
 '2026-01-01', '2028-12-31', 0, 'Industrial control panels, PLCs, inverters.',
 @now, @now),

-- Alam Plastik (Probation): Packaging only
('e5000000-0000-0000-0000-000000000009', 'e1000000-0000-0000-0000-000000000005', @cat_pkg,
 500.0000, 50000.0000, 'PCS', 14,
 '2026-06-01', '2027-05-31', 0, 'Plastic wrapping, polybag, stretch film.',
 @now, @now),

-- SKP Packaging: Packaging
('e5000000-0000-0000-0000-000000000010', 'e1000000-0000-0000-0000-000000000007', @cat_pkg,
 100.0000, 100000.0000, 'PCS', 7,
 '2025-01-01', '2026-12-31', 0, 'Corrugated boxes, cardboard, inner packaging.',
 @now, @now);

-- ============================================================
-- 17. MATERIALS
-- ============================================================
SET @now        = UTC_TIMESTAMP(6);
SET @company_id = (SELECT id FROM companies WHERE code = 'NCI' LIMIT 1);

DELETE FROM materials WHERE id LIKE 'aa%';
INSERT IGNORE INTO materials
    (id, category_id, code, name, description, uom_id,
     estimated_price, currency_id, is_strategic, is_active, is_deleted,
     created_at, updated_at)
VALUES
-- Raw Materials (strategic)
('aa000000-0000-0000-0000-000000000001', @cat_rm,    'STL-001', 'Cold Rolled Steel Sheet 2mm',
 'Cold-rolled steel sheet, thickness 2mm, grade SPCC, width 1200mm',
 @uom_kg,    18500.00,   @cur_idr, 1, 1, 0, @now, @now),

('aa000000-0000-0000-0000-000000000002', @cat_rm,    'STL-002', 'Seamless Steel Pipe Sch-40',
 'Seamless steel pipe schedule 40, 2 inch diameter, length 6m per piece',
 @uom_mtr,  185000.00,   @cur_idr, 1, 1, 0, @now, @now),

('aa000000-0000-0000-0000-000000000003', @cat_rm,    'ALM-001', 'Aluminum Ingot 6061-T6',
 'Aluminum alloy 6061-T6 ingot, purity 99.5%',
 @uom_kg,        2.85,   @cur_usd, 1, 1, 0, @now, @now),

-- Spare Parts
('aa000000-0000-0000-0000-000000000004', @cat_sp,    'BRN-001', 'Bearing SKF 6205-2RS',
 'Deep groove ball bearing SKF 6205-2RS, sealed both sides, ID=25mm OD=52mm',
 @uom_pcs,   85000.00,   @cur_idr, 0, 1, 0, @now, @now),

('aa000000-0000-0000-0000-000000000005', @cat_sp,    'BRN-002', 'Bearing NSK 6210',
 'Deep groove ball bearing NSK 6210, open type, ID=50mm OD=90mm',
 @uom_pcs,  145000.00,   @cur_idr, 0, 1, 0, @now, @now),

('aa000000-0000-0000-0000-000000000006', @cat_sp,    'VBL-001', 'V-Belt Gates A-50',
 'Classical V-belt Gates A-50, section A, 50 inch length',
 @uom_pcs,   48000.00,   @cur_idr, 0, 1, 0, @now, @now),

-- MRO / Maintenance
('aa000000-0000-0000-0000-000000000007', @cat_mro,   'LUB-001', 'Engine Oil SAE 40 (Drum)',
 'Engine lubricant oil SAE 40, 200-liter drum',
 @uom_drum, 3200000.00,  @cur_idr, 0, 1, 0, @now, @now),

('aa000000-0000-0000-0000-000000000008', @cat_mro,   'WLD-001', 'Welding Electrode E6013 3.2mm',
 'Mild steel welding electrode E6013, 3.2mm x 350mm, price per kg',
 @uom_kg,    28000.00,   @cur_idr, 0, 1, 0, @now, @now),

('aa000000-0000-0000-0000-000000000009', @cat_mro,   'GRD-001', 'Cutting Disc 4" x 1mm',
 'Cutting disc 4 inch x 1mm, for steel and stainless steel',
 @uom_pcs,   8500.00,    @cur_idr, 0, 1, 0, @now, @now),

-- CAPEX (strategic)
('aa000000-0000-0000-0000-000000000010', @cat_capex, 'CNC-001', 'CNC Milling Machine VMC-650',
 '3-axis CNC vertical machining center, table 650x400mm, spindle 8000rpm',
 @uom_unit,  48000.00,   @cur_usd, 1, 1, 0, @now, @now),

('aa000000-0000-0000-0000-000000000011', @cat_capex, 'HYD-001', 'Hydraulic Press 100 Ton',
 'H-frame hydraulic press 100 ton, stroke 200mm, working pressure 350 bar',
 @uom_unit,85000000.00,  @cur_idr, 1, 1, 0, @now, @now),

-- Packaging
('aa000000-0000-0000-0000-000000000012', @cat_pkg,   'BOX-001', 'Corrugated Box 40x30x20cm',
 'Single-wall corrugated box 40x30x20cm, BC-flute, natural kraft',
 @uom_pcs,   3500.00,    @cur_idr, 0, 1, 0, @now, @now),

('aa000000-0000-0000-0000-000000000013', @cat_pkg,   'PLY-001', 'Stretch Film 50cm x 300m',
 'LLDPE stretch film, width 50cm, length 300m, thickness 20 micron',
 @uom_roll,  85000.00,   @cur_idr, 0, 1, 0, @now, @now),

-- Chemical & Lubricants
('aa000000-0000-0000-0000-000000000014', @cat_chem,  'SOL-001', 'NC Thinner 1 Liter',
 'Nitrocellulose thinner for metal paint, 1-liter container',
 @uom_ltr,   25000.00,   @cur_idr, 0, 1, 0, @now, @now),

-- Electrical & Instrument
('aa000000-0000-0000-0000-000000000015', @cat_elec,  'PLC-001', 'PLC Omron CP1E-N30DR-A',
 'Programmable logic controller Omron CP1E, 30 I/O, relay output, AC power',
 @uom_unit, 4500000.00,  @cur_idr, 0, 1, 0, @now, @now),

-- Consumables (inactive — for testing inactive filter behaviour)
('aa000000-0000-0000-0000-000000000016', @cat_cons,  'HMR-001', 'Steel Claw Hammer 500g',
 'Steel claw hammer 500g, fiberglass handle [DISCONTINUED]',
 @uom_pcs,   75000.00,   @cur_idr, 0, 0, 0, @now, @now);

-- ============================================================
-- Done
-- ============================================================
SELECT CONCAT(
    'Seed complete. ',
    (SELECT COUNT(*) FROM vendors   WHERE company_id = @company_id), ' vendors, ',
    (SELECT COUNT(*) FROM users     WHERE company_id = @company_id), ' internal users, ',
    (SELECT COUNT(*) FROM materials WHERE is_deleted = 0), ' materials.'
) AS status;

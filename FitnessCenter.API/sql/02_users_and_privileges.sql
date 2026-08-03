-- ============================================================
-- FitnessCenter – Database Users & Privileges
-- 4 users:
--   1. fitness_app   – application user (min. privileges)
--   2. fitness_admin – full admin
--   3. fitness_readonly – read-only on all tables
--   4. fitness_restricted – read-only but CANNOT see Payment or audit_log
-- ============================================================

USE fitnesscenter;

-- ─────────────────────────────────────────────────────────────
-- 1. Application user – minimum privileges for CRUD
-- ─────────────────────────────────────────────────────────────
DROP USER IF EXISTS 'fitness_app'@'%';
CREATE USER 'fitness_app'@'%' IDENTIFIED BY 'AppPass123!';

GRANT SELECT, INSERT, UPDATE, DELETE
    ON fitnesscenter.*
    TO 'fitness_app'@'%';

-- Cannot DROP, ALTER, CREATE – only what the app needs
REVOKE DELETE ON fitnesscenter.audit_log FROM 'fitness_app'@'%';

-- ─────────────────────────────────────────────────────────────
-- 2. Admin user – full privileges
-- ─────────────────────────────────────────────────────────────
DROP USER IF EXISTS 'fitness_admin'@'%';
CREATE USER 'fitness_admin'@'%' IDENTIFIED BY 'AdminPass123!';

GRANT ALL PRIVILEGES
    ON fitnesscenter.*
    TO 'fitness_admin'@'%'
    WITH GRANT OPTION;

-- ─────────────────────────────────────────────────────────────
-- 3. Read-only user – can SELECT everything
-- ─────────────────────────────────────────────────────────────
DROP USER IF EXISTS 'fitness_readonly'@'%';
CREATE USER 'fitness_readonly'@'%' IDENTIFIED BY 'ReadOnly123!';

GRANT SELECT
    ON fitnesscenter.*
    TO 'fitness_readonly'@'%';

-- ─────────────────────────────────────────────────────────────
-- 4. Restricted user – read-only but NO access to Payment or audit_log
--    (e.g. a front-desk employee who can see members but not financial data)
-- ─────────────────────────────────────────────────────────────
DROP USER IF EXISTS 'fitness_restricted'@'%';
CREATE USER 'fitness_restricted'@'%' IDENTIFIED BY 'Restricted123!';

-- Grant SELECT on each safe table individually
GRANT SELECT ON fitnesscenter.Location        TO 'fitness_restricted'@'%';
GRANT SELECT ON fitnesscenter.trainer         TO 'fitness_restricted'@'%';
GRANT SELECT ON fitnesscenter.member          TO 'fitness_restricted'@'%';
GRANT SELECT ON fitnesscenter.subscription    TO 'fitness_restricted'@'%';
GRANT SELECT ON fitnesscenter.membership      TO 'fitness_restricted'@'%';
GRANT SELECT ON fitnesscenter.Center          TO 'fitness_restricted'@'%';
GRANT SELECT ON fitnesscenter.Hall            TO 'fitness_restricted'@'%';
GRANT SELECT ON fitnesscenter.class           TO 'fitness_restricted'@'%';
GRANT SELECT ON fitnesscenter.classbooking    TO 'fitness_restricted'@'%';
GRANT SELECT ON fitnesscenter.Equipment       TO 'fitness_restricted'@'%';
GRANT SELECT ON fitnesscenter.VendingMachine  TO 'fitness_restricted'@'%';
GRANT SELECT ON fitnesscenter.VendingMachineStock TO 'fitness_restricted'@'%';
GRANT SELECT ON fitnesscenter.Staff           TO 'fitness_restricted'@'%';
-- NO grant on Payment, audit_log, or app_user

FLUSH PRIVILEGES;

-- ─────────────────────────────────────────────────────────────
-- Verify
-- ─────────────────────────────────────────────────────────────
SHOW GRANTS FOR 'fitness_app'@'%';
SHOW GRANTS FOR 'fitness_admin'@'%';
SHOW GRANTS FOR 'fitness_readonly'@'%';
SHOW GRANTS FOR 'fitness_restricted'@'%';

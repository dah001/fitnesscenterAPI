-- ============================================================
-- FitnessCenter Database – Full Schema Script
-- Includes: tables, keys, indexes, constraints, stored
--           procedures, triggers, views, events
-- ============================================================

CREATE DATABASE IF NOT EXISTS fitnesscenter
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_0900_ai_ci;

USE fitnesscenter;

-- ─────────────────────────────────────────────────────────────
-- TABLES
-- ─────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS Location (
    LocationID INT NOT NULL AUTO_INCREMENT,
    City       VARCHAR(100) NOT NULL,
    PRIMARY KEY (LocationID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS trainer (
    TrainerID          INT NOT NULL AUTO_INCREMENT,
    Name               VARCHAR(100) NOT NULL,
    AiBio              TEXT DEFAULT NULL,
    AiBioGeneratedAt   DATETIME DEFAULT NULL,
    PRIMARY KEY (TrainerID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS member (
    MemberID   INT NOT NULL AUTO_INCREMENT,
    Name       VARCHAR(100) NOT NULL,
    Email      VARCHAR(100) DEFAULT NULL,
    TrainerID  INT DEFAULT NULL,
    BirthDate  DATE DEFAULT NULL,
    UserID     INT DEFAULT NULL,
    PRIMARY KEY (MemberID),
    UNIQUE KEY uk_member_email (Email),
    KEY idx_member_email   (Email),
    KEY fk_member_trainer  (TrainerID),
    CONSTRAINT fk_member_trainer FOREIGN KEY (TrainerID)
        REFERENCES trainer (TrainerID) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS subscription (
    SubscriptionID INT NOT NULL AUTO_INCREMENT,
    Type           VARCHAR(50) NOT NULL,
    Price          DECIMAL(8,2) NOT NULL,
    PRIMARY KEY (SubscriptionID),
    CONSTRAINT chk_subscription_price CHECK (Price > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS membership (
    MembershipID   INT NOT NULL AUTO_INCREMENT,
    MemberID       INT NOT NULL,
    SubscriptionID INT NOT NULL,
    StartDate      DATE NOT NULL,
    PRIMARY KEY (MembershipID),
    KEY fk_membership_member (MemberID),
    KEY fk_membership_sub    (SubscriptionID),
    CONSTRAINT fk_membership_member FOREIGN KEY (MemberID)
        REFERENCES member (MemberID) ON DELETE CASCADE,
    CONSTRAINT fk_membership_sub    FOREIGN KEY (SubscriptionID)
        REFERENCES subscription (SubscriptionID) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS Center (
    CenterID   INT NOT NULL AUTO_INCREMENT,
    LocationID INT NOT NULL,
    PRIMARY KEY (CenterID),
    KEY fk_center_location (LocationID),
    CONSTRAINT fk_center_location FOREIGN KEY (LocationID)
        REFERENCES Location (LocationID) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS Hall (
    HallID   INT NOT NULL AUTO_INCREMENT,
    CenterID INT NOT NULL,
    Name     VARCHAR(100) DEFAULT NULL,
    PRIMARY KEY (HallID),
    KEY fk_hall_center (CenterID),
    CONSTRAINT fk_hall_center FOREIGN KEY (CenterID)
        REFERENCES Center (CenterID) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS class (
    ClassID                    INT NOT NULL AUTO_INCREMENT,
    Name                       VARCHAR(100) NOT NULL,
    TrainerID                  INT NOT NULL,
    ClassDate                  DATETIME NOT NULL,
    HallID                     INT DEFAULT NULL,
    LocationID                 INT DEFAULT NULL,
    AiDescription              TEXT DEFAULT NULL,
    AiDescriptionGeneratedAt   DATETIME DEFAULT NULL,
    PRIMARY KEY (ClassID),
    KEY fk_class_trainer  (TrainerID),
    KEY fk_class_hall     (HallID),
    KEY idx_class_date    (ClassDate),
    CONSTRAINT fk_class_trainer FOREIGN KEY (TrainerID)
        REFERENCES trainer (TrainerID) ON DELETE RESTRICT,
    CONSTRAINT fk_class_hall    FOREIGN KEY (HallID)
        REFERENCES Hall (HallID) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS classbooking (
    BookingID INT NOT NULL AUTO_INCREMENT,
    MemberID  INT NOT NULL,
    ClassID   INT NOT NULL,
    PRIMARY KEY (BookingID),
    UNIQUE KEY uk_classbooking (MemberID, ClassID),
    KEY fk_booking_class (ClassID),
    CONSTRAINT fk_booking_member FOREIGN KEY (MemberID)
        REFERENCES member (MemberID) ON DELETE CASCADE,
    CONSTRAINT fk_booking_class  FOREIGN KEY (ClassID)
        REFERENCES class (ClassID) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS Payment (
    PaymentID   INT NOT NULL AUTO_INCREMENT,
    MemberID    INT NOT NULL,
    Amount      DECIMAL(8,2) DEFAULT NULL,
    PaymentDate DATETIME DEFAULT NULL,
    PaymentType VARCHAR(50) DEFAULT NULL,
    PRIMARY KEY (PaymentID),
    KEY fk_payment_member (MemberID),
    KEY idx_payment_date  (PaymentDate),
    CONSTRAINT fk_payment_member FOREIGN KEY (MemberID)
        REFERENCES member (MemberID) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS Equipment (
    EquipmentID INT NOT NULL AUTO_INCREMENT,
    Name        VARCHAR(100) DEFAULT NULL,
    CenterID    INT DEFAULT NULL,
    PRIMARY KEY (EquipmentID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS VendingMachine (
    VendingMachineID INT NOT NULL AUTO_INCREMENT,
    Name             VARCHAR(100) DEFAULT NULL,
    Location         VARCHAR(100) DEFAULT NULL,
    CenterID         INT DEFAULT NULL,
    PRIMARY KEY (VendingMachineID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS VendingMachineStock (
    StockID          INT NOT NULL AUTO_INCREMENT,
    VendingMachineID INT NOT NULL,
    ProductName      VARCHAR(100) DEFAULT NULL,
    Quantity         INT DEFAULT NULL,
    Price            DECIMAL(8,2) DEFAULT NULL,
    PRIMARY KEY (StockID),
    KEY fk_stock_vending (VendingMachineID),
    CONSTRAINT fk_stock_vending FOREIGN KEY (VendingMachineID)
        REFERENCES VendingMachine (VendingMachineID) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS Staff (
    StaffID INT NOT NULL AUTO_INCREMENT,
    Name    VARCHAR(100) NOT NULL,
    Role    VARCHAR(50) DEFAULT NULL,
    UserID  INT DEFAULT NULL,
    PRIMARY KEY (StaffID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS app_user (
    UserID       INT NOT NULL AUTO_INCREMENT,
    Username     VARCHAR(50) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Enabled      TINYINT(1) NOT NULL DEFAULT 1,
    MemberID     INT DEFAULT NULL,
    TrainerID    INT DEFAULT NULL,
    Role         VARCHAR(50) NOT NULL DEFAULT 'User',
    PRIMARY KEY (UserID),
    UNIQUE KEY uk_user_username (Username),
    KEY fk_user_member  (MemberID),
    KEY fk_user_trainer (TrainerID),
    CONSTRAINT fk_user_member  FOREIGN KEY (MemberID)  REFERENCES member  (MemberID),
    CONSTRAINT fk_user_trainer FOREIGN KEY (TrainerID) REFERENCES trainer (TrainerID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ─────────────────────────────────────────────────────────────
-- AUDIT TABLE
-- ─────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS audit_log (
    AuditID    INT NOT NULL AUTO_INCREMENT,
    TableName  VARCHAR(50) NOT NULL,
    Action     VARCHAR(10) NOT NULL,   -- INSERT / UPDATE / DELETE
    RecordID   INT DEFAULT NULL,
    ChangedAt  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ChangedBy  VARCHAR(100) DEFAULT NULL,
    OldData    TEXT DEFAULT NULL,
    NewData    TEXT DEFAULT NULL,
    PRIMARY KEY (AuditID),
    KEY idx_audit_table (TableName),
    KEY idx_audit_time  (ChangedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ─────────────────────────────────────────────────────────────
-- VIEWS
-- ─────────────────────────────────────────────────────────────

CREATE OR REPLACE VIEW v_member_overview AS
SELECT
    m.MemberID,
    m.Name        AS MemberName,
    m.Email,
    m.BirthDate,
    t.Name        AS TrainerName,
    s.Type        AS SubscriptionType,
    s.Price       AS SubscriptionPrice,
    ms.StartDate  AS MemberSince,
    COUNT(DISTINCT cb.BookingID) AS TotalBookings,
    SUM(p.Amount)                AS TotalPaid
FROM member m
LEFT JOIN trainer     t  ON t.TrainerID     = m.TrainerID
LEFT JOIN membership  ms ON ms.MemberID     = m.MemberID
LEFT JOIN subscription s ON s.SubscriptionID = ms.SubscriptionID
LEFT JOIN classbooking cb ON cb.MemberID    = m.MemberID
LEFT JOIN Payment     p  ON p.MemberID      = m.MemberID
GROUP BY m.MemberID, m.Name, m.Email, m.BirthDate,
         t.Name, s.Type, s.Price, ms.StartDate;

CREATE OR REPLACE VIEW v_class_popularity AS
SELECT
    c.ClassID,
    c.Name      AS ClassName,
    c.ClassDate,
    t.Name      AS TrainerName,
    h.Name      AS HallName,
    l.City,
    COUNT(cb.BookingID) AS BookingCount
FROM class c
LEFT JOIN trainer     t  ON t.TrainerID  = c.TrainerID
LEFT JOIN Hall        h  ON h.HallID     = c.HallID
LEFT JOIN Location    l  ON l.LocationID = c.LocationID
LEFT JOIN classbooking cb ON cb.ClassID  = c.ClassID
GROUP BY c.ClassID, c.Name, c.ClassDate, t.Name, h.Name, l.City
ORDER BY BookingCount DESC;

CREATE OR REPLACE VIEW v_trainer_stats AS
SELECT
    t.TrainerID,
    t.Name        AS TrainerName,
    COUNT(DISTINCT m.MemberID)  AS MemberCount,
    COUNT(DISTINCT c.ClassID)   AS ClassCount,
    SUM(p.Amount)               AS TotalRevenueFromMembers
FROM trainer t
LEFT JOIN member       m  ON m.TrainerID  = t.TrainerID
LEFT JOIN class        c  ON c.TrainerID  = t.TrainerID
LEFT JOIN Payment      p  ON p.MemberID   = m.MemberID
GROUP BY t.TrainerID, t.Name;

CREATE OR REPLACE VIEW v_revenue_by_month AS
SELECT
    DATE_FORMAT(PaymentDate, '%Y-%m') AS Month,
    COUNT(*)                          AS PaymentCount,
    SUM(Amount)                       AS TotalRevenue,
    AVG(Amount)                       AS AvgPayment
FROM Payment
GROUP BY DATE_FORMAT(PaymentDate, '%Y-%m')
ORDER BY Month DESC;

-- ─────────────────────────────────────────────────────────────
-- STORED PROCEDURES
-- ─────────────────────────────────────────────────────────────

DELIMITER $$

-- Get member full profile
CREATE PROCEDURE IF NOT EXISTS sp_get_member_profile(IN p_memberID INT)
BEGIN
    SELECT
        m.MemberID, m.Name, m.Email, m.BirthDate,
        t.Name      AS TrainerName,
        s.Type      AS SubscriptionType,
        s.Price     AS SubscriptionPrice,
        ms.StartDate AS MemberSince
    FROM member m
    LEFT JOIN trainer    t  ON t.TrainerID      = m.TrainerID
    LEFT JOIN membership ms ON ms.MemberID       = m.MemberID
    LEFT JOIN subscription s ON s.SubscriptionID = ms.SubscriptionID
    WHERE m.MemberID = p_memberID;

    SELECT cb.BookingID, c.Name AS ClassName, c.ClassDate
    FROM classbooking cb
    JOIN class c ON c.ClassID = cb.ClassID
    WHERE cb.MemberID = p_memberID
    ORDER BY c.ClassDate DESC
    LIMIT 10;

    SELECT PaymentID, Amount, PaymentDate, PaymentType
    FROM Payment
    WHERE MemberID = p_memberID
    ORDER BY PaymentDate DESC
    LIMIT 10;
END$$

-- Book a class with duplicate check
CREATE PROCEDURE IF NOT EXISTS sp_book_class(
    IN  p_memberID INT,
    IN  p_classID  INT,
    OUT p_result   VARCHAR(100)
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM member WHERE MemberID = p_memberID) THEN
        SET p_result = 'ERROR: Member not found';
    ELSEIF NOT EXISTS (SELECT 1 FROM class WHERE ClassID = p_classID) THEN
        SET p_result = 'ERROR: Class not found';
    ELSEIF EXISTS (SELECT 1 FROM classbooking WHERE MemberID = p_memberID AND ClassID = p_classID) THEN
        SET p_result = 'ERROR: Already booked';
    ELSE
        INSERT INTO classbooking (MemberID, ClassID) VALUES (p_memberID, p_classID);
        SET p_result = CONCAT('OK: BookingID=', LAST_INSERT_ID());
    END IF;
END$$

-- Monthly revenue report
CREATE PROCEDURE IF NOT EXISTS sp_revenue_report(
    IN p_year  INT,
    IN p_month INT
)
BEGIN
    SELECT
        m.Name AS MemberName,
        p.Amount,
        p.PaymentDate,
        p.PaymentType
    FROM Payment p
    JOIN member m ON m.MemberID = p.MemberID
    WHERE YEAR(p.PaymentDate) = p_year
      AND MONTH(p.PaymentDate) = p_month
    ORDER BY p.PaymentDate;
END$$

-- Paginated member search
CREATE PROCEDURE IF NOT EXISTS sp_search_members(
    IN p_search   VARCHAR(100),
    IN p_page     INT,
    IN p_pageSize INT
)
BEGIN
    DECLARE v_offset INT DEFAULT (p_page - 1) * p_pageSize;
    SELECT m.MemberID, m.Name, m.Email, t.Name AS TrainerName
    FROM member m
    LEFT JOIN trainer t ON t.TrainerID = m.TrainerID
    WHERE (p_search IS NULL OR m.Name LIKE CONCAT('%', p_search, '%')
                            OR m.Email LIKE CONCAT('%', p_search, '%'))
    ORDER BY m.Name
    LIMIT p_pageSize OFFSET v_offset;
END$$

DELIMITER ;

-- ─────────────────────────────────────────────────────────────
-- TRIGGERS – Audit log
-- ─────────────────────────────────────────────────────────────

DELIMITER $$

-- Member INSERT
CREATE TRIGGER IF NOT EXISTS trg_member_insert
AFTER INSERT ON member
FOR EACH ROW
BEGIN
    INSERT INTO audit_log (TableName, Action, RecordID, NewData)
    VALUES ('member', 'INSERT', NEW.MemberID,
            CONCAT('Name=', NEW.Name, ', Email=', IFNULL(NEW.Email, 'NULL')));
END$$

-- Member UPDATE
CREATE TRIGGER IF NOT EXISTS trg_member_update
AFTER UPDATE ON member
FOR EACH ROW
BEGIN
    INSERT INTO audit_log (TableName, Action, RecordID, OldData, NewData)
    VALUES ('member', 'UPDATE', NEW.MemberID,
            CONCAT('Name=', OLD.Name, ', Email=', IFNULL(OLD.Email, 'NULL')),
            CONCAT('Name=', NEW.Name, ', Email=', IFNULL(NEW.Email, 'NULL')));
END$$

-- Member DELETE
CREATE TRIGGER IF NOT EXISTS trg_member_delete
AFTER DELETE ON member
FOR EACH ROW
BEGIN
    INSERT INTO audit_log (TableName, Action, RecordID, OldData)
    VALUES ('member', 'DELETE', OLD.MemberID,
            CONCAT('Name=', OLD.Name, ', Email=', IFNULL(OLD.Email, 'NULL')));
END$$

-- Payment INSERT
CREATE TRIGGER IF NOT EXISTS trg_payment_insert
AFTER INSERT ON Payment
FOR EACH ROW
BEGIN
    INSERT INTO audit_log (TableName, Action, RecordID, NewData)
    VALUES ('Payment', 'INSERT', NEW.PaymentID,
            CONCAT('MemberID=', NEW.MemberID, ', Amount=', IFNULL(NEW.Amount, 'NULL')));
END$$

-- Class booking INSERT
CREATE TRIGGER IF NOT EXISTS trg_booking_insert
AFTER INSERT ON classbooking
FOR EACH ROW
BEGIN
    INSERT INTO audit_log (TableName, Action, RecordID, NewData)
    VALUES ('classbooking', 'INSERT', NEW.BookingID,
            CONCAT('MemberID=', NEW.MemberID, ', ClassID=', NEW.ClassID));
END$$

DELIMITER ;

-- ─────────────────────────────────────────────────────────────
-- EVENTS
-- ─────────────────────────────────────────────────────────────

SET GLOBAL event_scheduler = ON;

DELIMITER $$

-- Clean audit log entries older than 1 year (runs monthly)
CREATE EVENT IF NOT EXISTS evt_cleanup_audit_log
ON SCHEDULE EVERY 1 MONTH
STARTS CURRENT_TIMESTAMP
DO
BEGIN
    DELETE FROM audit_log
    WHERE ChangedAt < DATE_SUB(NOW(), INTERVAL 1 YEAR);
END$$

-- Archive old payments summary (runs yearly on Jan 1)
CREATE EVENT IF NOT EXISTS evt_yearly_summary
ON SCHEDULE EVERY 1 YEAR
STARTS '2025-01-01 02:00:00'
DO
BEGIN
    -- This event logs a yearly summary marker to audit_log for reporting
    INSERT INTO audit_log (TableName, Action, OldData)
    VALUES ('Payment', 'YEARLY_SUMMARY',
            CONCAT('Year=', YEAR(NOW()) - 1,
                   ', Total=', (SELECT IFNULL(SUM(Amount), 0) FROM Payment
                                WHERE YEAR(PaymentDate) = YEAR(NOW()) - 1)));
END$$

DELIMITER ;

-- ═══════════════════════════════════════════════════════════════
--  AttendanceMS  –  Full Migration Script (v2.0)
--  Run this against your existing AttendanceMSDb
--  Safe to re-run: uses IF NOT EXISTS / IF COL_LENGTH guards
-- ═══════════════════════════════════════════════════════════════

USE AttendanceMSDb;
GO

-- ───────────────────────────────────────────────────────────────
-- STEP 1 : Ensure Role column exists on Users (old schema had free-text)
-- ───────────────────────────────────────────────────────────────
IF COL_LENGTH('Users','Role') IS NULL
BEGIN
    ALTER TABLE Users ADD Role NVARCHAR(20) NOT NULL DEFAULT 'faculty';
    PRINT 'Added Role column to Users';
END
GO

-- ───────────────────────────────────────────────────────────────
-- STEP 2 : Add UserId FK to Students  (for student login)
-- ───────────────────────────────────────────────────────────────
IF COL_LENGTH('Students','UserId') IS NULL
BEGIN
    ALTER TABLE Students ADD UserId INT NULL;
    PRINT 'Added UserId column to Students';
END
GO

-- ───────────────────────────────────────────────────────────────
-- STEP 3 : Add UserId FK to Faculty  (link Faculty ↔ Users)
-- ───────────────────────────────────────────────────────────────
IF COL_LENGTH('Faculty','UserId') IS NULL
BEGIN
    ALTER TABLE Faculty ADD UserId INT NULL;
    PRINT 'Added UserId column to Faculty';
END
GO

-- ───────────────────────────────────────────────────────────────
-- STEP 4 : ThresholdSettings
-- ───────────────────────────────────────────────────────────────
IF OBJECT_ID('ThresholdSettings','U') IS NULL
BEGIN
    CREATE TABLE ThresholdSettings (
        ThresholdId        INT           IDENTITY(1,1) PRIMARY KEY,
        FacultyId          INT           NULL,   -- NULL = global default
        SemesterId         INT           NULL,   -- NULL = all semesters
        ClassId            INT           NULL,   -- NULL = all classes
        BlacklistThreshold DECIMAL(5,2)  NOT NULL DEFAULT 50.00,
        WarningThreshold   DECIMAL(5,2)  NOT NULL DEFAULT 75.00,
        IsGlobal           BIT           NOT NULL DEFAULT 0,
        CreatedAt          DATETIME2     NOT NULL DEFAULT GETDATE(),
        UpdatedAt          DATETIME2     NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Threshold_Faculty   FOREIGN KEY (FacultyId)  REFERENCES Faculty(FacultyId),
        CONSTRAINT FK_Threshold_Semester  FOREIGN KEY (SemesterId) REFERENCES Semesters(SemesterId),
        CONSTRAINT FK_Threshold_Class     FOREIGN KEY (ClassId)    REFERENCES Classes(ClassId)
    );
    PRINT 'Created ThresholdSettings table';

    -- Global default threshold
    INSERT INTO ThresholdSettings (FacultyId, SemesterId, ClassId, BlacklistThreshold, WarningThreshold, IsGlobal)
    VALUES (NULL, NULL, NULL, 50.00, 75.00, 1);
    PRINT 'Inserted global default threshold (Blacklist<50%, Warning<75%)';
END
GO

-- ───────────────────────────────────────────────────────────────
-- STEP 5 : EmailNotificationLog  (audit trail for emails sent)
-- ───────────────────────────────────────────────────────────────
IF OBJECT_ID('EmailNotificationLog','U') IS NULL
BEGIN
    CREATE TABLE EmailNotificationLog (
        LogId      INT           IDENTITY(1,1) PRIMARY KEY,
        StudentId  INT           NOT NULL,
        EmailType  NVARCHAR(50)  NOT NULL,   -- 'Warning' | 'Blacklisted'
        SentAt     DATETIME2     NOT NULL DEFAULT GETDATE(),
        IsSuccess  BIT           NOT NULL DEFAULT 1,
        ErrorMsg   NVARCHAR(500) NULL,
        CONSTRAINT FK_EmailLog_Students FOREIGN KEY (StudentId) REFERENCES Students(StudentId)
    );
    PRINT 'Created EmailNotificationLog table';
END
GO

-- ───────────────────────────────────────────────────────────────
-- STEP 6 : Create student Users accounts (password = RollNo)
--          BCrypt hash for 'BCA001' = used as placeholder, each student
--          should reset their password. This seeds 10 sample students.
-- ───────────────────────────────────────────────────────────────
-- Default password hash for 'Student@123' (BCrypt cost=11)
DECLARE @defaultStudentPwd NVARCHAR(300) =
    '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi';

-- For each student with no UserId, create a Users record
INSERT INTO Users (Name, Email, Role, PasswordHash, IsActive)
SELECT s.Name, s.Email, 'student', @defaultStudentPwd, 1
FROM Students s
WHERE s.Email NOT IN (SELECT Email FROM Users)
  AND s.UserId IS NULL;

-- Link the Students.UserId
UPDATE s
SET s.UserId = u.Id
FROM Students s
JOIN Users u ON u.Email = s.Email AND u.Role = 'student'
WHERE s.UserId IS NULL;

PRINT 'Student Users accounts created and linked';
GO

-- ───────────────────────────────────────────────────────────────
-- STEP 7 : Link Faculty.UserId
-- ───────────────────────────────────────────────────────────────
UPDATE f
SET f.UserId = u.Id
FROM Faculty f
JOIN Users u ON u.Email = f.Email AND u.Role = 'faculty'
WHERE f.UserId IS NULL;

PRINT 'Faculty UserId links updated';
GO

-- ───────────────────────────────────────────────────────────────
-- STEP 8 : Useful stored procedures
-- ───────────────────────────────────────────────────────────────

-- Get attendance report for a student with filters
IF OBJECT_ID('sp_GetStudentAttendanceReport','P') IS NOT NULL
    DROP PROCEDURE sp_GetStudentAttendanceReport;
GO
CREATE PROCEDURE sp_GetStudentAttendanceReport
    @StudentId  INT,
    @Subject    NVARCHAR(100) = NULL,
    @FromDate   DATE          = NULL,
    @ToDate     DATE          = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        a.AttendanceId,
        a.StudentId,
        a.Subject,
        a.Date,
        a.Status,
        a.MarkedAt,
        c.ClassName,
        sem.SemesterNumber
    FROM Attendance a
    JOIN Classes   c   ON c.ClassId    = a.ClassId
    JOIN Semesters sem ON sem.SemesterId = a.SemesterId
    WHERE a.StudentId = @StudentId
      AND (@Subject  IS NULL OR a.Subject = @Subject)
      AND (@FromDate IS NULL OR a.Date  >= @FromDate)
      AND (@ToDate   IS NULL OR a.Date  <= @ToDate)
    ORDER BY a.Date DESC;
END
GO

-- Get class attendance report for faculty
IF OBJECT_ID('sp_GetClassAttendanceReport','P') IS NOT NULL
    DROP PROCEDURE sp_GetClassAttendanceReport;
GO
CREATE PROCEDURE sp_GetClassAttendanceReport
    @FacultyId  INT           = NULL,
    @SemesterId INT           = NULL,
    @ClassId    INT           = NULL,
    @Subject    NVARCHAR(100) = NULL,
    @FromDate   DATE          = NULL,
    @ToDate     DATE          = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        s.StudentId,
        s.Name           AS StudentName,
        s.RollNo,
        a.Subject,
        COUNT(a.AttendanceId)                                   AS TotalClasses,
        SUM(CASE WHEN a.Status = 'present' THEN 1 ELSE 0 END)  AS Present,
        SUM(CASE WHEN a.Status = 'absent'  THEN 1 ELSE 0 END)  AS Absent,
        SUM(CASE WHEN a.Status = 'late'    THEN 1 ELSE 0 END)  AS Late,
        CAST(
            CAST(SUM(CASE WHEN a.Status IN('present','late') THEN 1 ELSE 0 END) AS DECIMAL(10,2))
            / NULLIF(COUNT(a.AttendanceId),0) * 100
        AS DECIMAL(5,2))                                        AS AttendancePercent
    FROM Students s
    JOIN Attendance a ON a.StudentId  = s.StudentId
    WHERE (@SemesterId IS NULL OR a.SemesterId = @SemesterId)
      AND (@ClassId    IS NULL OR a.ClassId    = @ClassId)
      AND (@Subject    IS NULL OR a.Subject    = @Subject)
      AND (@FromDate   IS NULL OR a.Date      >= @FromDate)
      AND (@ToDate     IS NULL OR a.Date      <= @ToDate)
    GROUP BY s.StudentId, s.Name, s.RollNo, a.Subject
    ORDER BY s.RollNo;
END
GO

-- Recalculate and update student status based on threshold
IF OBJECT_ID('sp_UpdateStudentStatus','P') IS NOT NULL
    DROP PROCEDURE sp_UpdateStudentStatus;
GO
CREATE PROCEDURE sp_UpdateStudentStatus
    @StudentId         INT,
    @BlacklistThreshold DECIMAL(5,2) = 50.00,
    @WarningThreshold   DECIMAL(5,2) = 75.00
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalClasses INT, @PresentLate INT, @Percent DECIMAL(5,2);
    DECLARE @NewStatus NVARCHAR(20);

    SELECT
        @TotalClasses = COUNT(*),
        @PresentLate  = SUM(CASE WHEN Status IN('present','late') THEN 1 ELSE 0 END)
    FROM Attendance
    WHERE StudentId = @StudentId;

    IF @TotalClasses = 0
        SET @Percent = 0
    ELSE
        SET @Percent = CAST(@PresentLate AS DECIMAL(10,2)) / @TotalClasses * 100;

    SET @NewStatus = CASE
        WHEN @Percent < @BlacklistThreshold THEN 'blacklisted'
        WHEN @Percent < @WarningThreshold   THEN 'warning'
        ELSE 'active'
    END;

    UPDATE Students
    SET AttendancePercent = @Percent,
        Status = @NewStatus
    WHERE StudentId = @StudentId;

    SELECT @NewStatus AS NewStatus, @Percent AS NewPercent;
END
GO

PRINT '═══════════════════════════════════════════';
PRINT 'Migration v2.0 complete!';
PRINT 'New: ThresholdSettings, EmailNotificationLog';
PRINT 'New: Students.UserId, Faculty.UserId';
PRINT 'New: Student login accounts (password: Student@123)';
PRINT '═══════════════════════════════════════════';
GO

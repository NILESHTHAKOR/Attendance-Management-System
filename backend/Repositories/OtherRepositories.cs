using AttendanceMS.Data;
using AttendanceMS.DTOs;
using AttendanceMS.Models;
using AttendanceMS.Repositories.Interfaces;

namespace AttendanceMS.Repositories;

// ════════════════════════════════════════════════════════
//  ThresholdRepository
// ════════════════════════════════════════════════════════
public sealed class ThresholdRepository : IThresholdRepository
{
    private readonly DbHelper _db;
    public ThresholdRepository(DbHelper db) => _db = db;

    public ThresholdSetting? GetGlobal()
    {
        const string sql = """
            SELECT TOP 1 ThresholdId, BlacklistThreshold, WarningThreshold, IsGlobal, UpdatedAt
            FROM   ThresholdSettings
            WHERE  IsGlobal = 1
            ORDER BY UpdatedAt DESC
            """;
        var dt = _db.ExecuteQuery(sql);
        if (dt.Rows.Count == 0) return null;
        return MapTs(dt.Rows[0]);
    }

    public ThresholdSetting? GetForFaculty(int facultyId, int? semesterId, int? classId)
    {
        const string sql = """
            SELECT TOP 1 ThresholdId, BlacklistThreshold, WarningThreshold, IsGlobal, UpdatedAt
            FROM   ThresholdSettings
            WHERE  FacultyId = @FacultyId
              AND  (SemesterId = @SemesterId OR (@SemesterId IS NULL AND SemesterId IS NULL))
              AND  (ClassId    = @ClassId    OR (@ClassId    IS NULL AND ClassId    IS NULL))
            ORDER BY UpdatedAt DESC
            """;
        var dt = _db.ExecuteQuery(sql,
            DbHelper.Param("@FacultyId",  facultyId),
            DbHelper.ParamNullable("@SemesterId", semesterId),
            DbHelper.ParamNullable("@ClassId",    classId));
        return dt.Rows.Count == 0 ? null : MapTs(dt.Rows[0]);
    }

    public void Upsert(ThresholdDto dto)
    {
        // Try update first
        const string updateSql = """
            UPDATE ThresholdSettings
            SET    BlacklistThreshold = @BL, WarningThreshold = @WL, UpdatedAt = GETDATE()
            WHERE  (FacultyId  = @FacultyId  OR (FacultyId  IS NULL AND @FacultyId  IS NULL))
              AND  (SemesterId = @SemesterId OR (SemesterId IS NULL AND @SemesterId IS NULL))
              AND  (ClassId    = @ClassId    OR (ClassId    IS NULL AND @ClassId    IS NULL))
              AND  IsGlobal    = @IsGlobal
            """;
        int rows = _db.ExecuteNonQuery(updateSql,
            DbHelper.Param("@BL",          dto.BlacklistThreshold),
            DbHelper.Param("@WL",          dto.WarningThreshold),
            DbHelper.ParamNullable("@FacultyId",  dto.FacultyId),
            DbHelper.ParamNullable("@SemesterId", dto.SemesterId),
            DbHelper.ParamNullable("@ClassId",    dto.ClassId),
            DbHelper.Param("@IsGlobal",    dto.IsGlobal));

        if (rows == 0)
        {
            const string insertSql = """
                INSERT INTO ThresholdSettings
                    (FacultyId, SemesterId, ClassId, BlacklistThreshold, WarningThreshold, IsGlobal)
                VALUES (@FacultyId, @SemesterId, @ClassId, @BL, @WL, @IsGlobal)
                """;
            _db.ExecuteNonQuery(insertSql,
                DbHelper.ParamNullable("@FacultyId",  dto.FacultyId),
                DbHelper.ParamNullable("@SemesterId", dto.SemesterId),
                DbHelper.ParamNullable("@ClassId",    dto.ClassId),
                DbHelper.Param("@BL",                 dto.BlacklistThreshold),
                DbHelper.Param("@WL",                 dto.WarningThreshold),
                DbHelper.Param("@IsGlobal",            dto.IsGlobal));
        }
    }

    public List<ThresholdSetting> GetAllByFaculty(int facultyId)
    {
        const string sql = """
            SELECT ts.ThresholdId, ts.BlacklistThreshold, ts.WarningThreshold,
                   ts.IsGlobal, ts.UpdatedAt,
                   COALESCE('Sem ' + CAST(sem.SemesterNumber AS NVARCHAR) + ' / Class ' + c.ClassName,
                             'All Classes') AS Scope
            FROM   ThresholdSettings ts
            LEFT   JOIN Semesters sem ON sem.SemesterId = ts.SemesterId
            LEFT   JOIN Classes   c   ON c.ClassId      = ts.ClassId
            WHERE  ts.FacultyId = @FacultyId OR ts.IsGlobal = 1
            ORDER BY ts.IsGlobal DESC, ts.UpdatedAt DESC
            """;
        var dt = _db.ExecuteQuery(sql, DbHelper.Param("@FacultyId", facultyId));
        return dt.Rows.Cast<System.Data.DataRow>().Select(r => new ThresholdSetting
        {
            ThresholdId = (int)r["ThresholdId"],
            BlacklistThreshold = (decimal)r["BlacklistThreshold"],
            WarningThreshold = (decimal)r["WarningThreshold"],
            IsGlobal = (bool)r["IsGlobal"],
            UpdatedAt = (DateTime)r["UpdatedAt"]
        }).ToList();
    }

    private static ThresholdSetting MapTs(System.Data.DataRow r) => new()
    {
        ThresholdId        = (int)    r["ThresholdId"],
        BlacklistThreshold = (decimal)r["BlacklistThreshold"],
        WarningThreshold   = (decimal)r["WarningThreshold"],
        IsGlobal           = (bool)   r["IsGlobal"],
        UpdatedAt          = (DateTime)r["UpdatedAt"]
    };
}

// ════════════════════════════════════════════════════════
//  FacultyRepository
// ════════════════════════════════════════════════════════
public sealed class FacultyRepository : IFacultyRepository
{
    private readonly DbHelper _db;
    public FacultyRepository(DbHelper db) => _db = db;

    public Faculty? GetById(int facultyId)
    {
        const string sql = "SELECT * FROM Faculty WHERE FacultyId = @Id";
        var dt = _db.ExecuteQuery(sql, DbHelper.Param("@Id", facultyId));
        return dt.Rows.Count == 0 ? null : MapFaculty(dt.Rows[0]);
    }

    public Faculty? GetByUserId(int userId)
    {
        const string sql = "SELECT * FROM Faculty WHERE UserId = @UserId";
        var dt = _db.ExecuteQuery(sql, DbHelper.Param("@UserId", userId));
        return dt.Rows.Count == 0 ? null : MapFaculty(dt.Rows[0]);
    }

    public List<TimetableSlot> GetTodaySlots(int facultyId)
    {
        string today = DateTime.Today.DayOfWeek.ToString();
        const string sql = """
            SELECT t.TimetableId, t.Subject, t.ClassId, c.ClassName,
                   t.SemesterId, sem.SemesterNumber,
                   CONVERT(NVARCHAR, t.StartTime, 108) AS StartTime,
                   CONVERT(NVARCHAR, t.EndTime,   108) AS EndTime,
                   t.DayOfWeek
            FROM   Timetables t
            JOIN   Classes   c   ON c.ClassId     = t.ClassId
            JOIN   Semesters sem ON sem.SemesterId = t.SemesterId
            WHERE  t.FacultyId  = @FacultyId
              AND  t.DayOfWeek  = @Day
            ORDER BY t.StartTime
            """;
        var dt = _db.ExecuteQuery(sql,
            DbHelper.Param("@FacultyId", facultyId),
            DbHelper.Param("@Day",       today));
        return MapSlots(dt);
    }

    public List<TimetableSlot> GetAllSlots(int facultyId)
    {
        const string sql = """
            SELECT t.TimetableId, t.Subject, t.ClassId, c.ClassName,
                   t.SemesterId, sem.SemesterNumber,
                   CONVERT(NVARCHAR, t.StartTime, 108) AS StartTime,
                   CONVERT(NVARCHAR, t.EndTime,   108) AS EndTime,
                   t.DayOfWeek
            FROM   Timetables t
            JOIN   Classes   c   ON c.ClassId     = t.ClassId
            JOIN   Semesters sem ON sem.SemesterId = t.SemesterId
            WHERE  t.FacultyId = @FacultyId
            ORDER BY t.DayOfWeek, t.StartTime
            """;
        var dt = _db.ExecuteQuery(sql, DbHelper.Param("@FacultyId", facultyId));
        return MapSlots(dt);
    }

    private static List<TimetableSlot> MapSlots(System.Data.DataTable dt) =>
        dt.Rows.Cast<System.Data.DataRow>().Select(r => new TimetableSlot(
            (int)    r["TimetableId"],
            (string) r["Subject"],
            (int)    r["ClassId"],
            (string) r["ClassName"],
            (int)    r["SemesterId"],
            (int)    r["SemesterNumber"],
            (string) r["StartTime"],
            (string) r["EndTime"],
            (string) r["DayOfWeek"]
        )).ToList();

    private static Faculty MapFaculty(System.Data.DataRow r) => new()
    {
        FacultyId    = (int)    r["FacultyId"],
        UserId       = r["UserId"] == DBNull.Value ? null : (int?)r["UserId"],
        Name         = (string) r["Name"],
        Email        = (string) r["Email"],
        Subject      = (string) r["Subject"],
        PasswordHash = (string) r["PasswordHash"],
        CreatedAt    = (DateTime)r["CreatedAt"]
    };
}

// ════════════════════════════════════════════════════════
//  ReportRepository
// ════════════════════════════════════════════════════════
public sealed class ReportRepository : IReportRepository
{
    private readonly DbHelper _db;
    public ReportRepository(DbHelper db) => _db = db;

    public List<AttendanceSummaryDto> GetStudentSubjectSummary(int studentId, string? subject, DateOnly? from, DateOnly? to)
    {
        const string sql = """
            SELECT
                s.StudentId, s.Name AS StudentName, s.RollNo, s.Email AS StudentEmail,
                a.Subject,
                COUNT(a.AttendanceId)                                    AS TotalClasses,
                SUM(CASE WHEN a.Status='present' THEN 1 ELSE 0 END)     AS Present,
                SUM(CASE WHEN a.Status='absent'  THEN 1 ELSE 0 END)     AS Absent,
                SUM(CASE WHEN a.Status='late'    THEN 1 ELSE 0 END)     AS Late,
                CAST(
                    CAST(SUM(CASE WHEN a.Status IN('present','late') THEN 1 ELSE 0 END) AS DECIMAL(10,2))
                    / NULLIF(COUNT(a.AttendanceId),0) * 100
                AS DECIMAL(5,2))                                         AS AttendancePercent,
                s.Status
            FROM   Students   s
            JOIN   Attendance a ON a.StudentId = s.StudentId
            WHERE  s.StudentId = @StudentId
              AND  (@Subject  IS NULL OR a.Subject = @Subject)
              AND  (@FromDate IS NULL OR a.Date   >= @FromDate)
              AND  (@ToDate   IS NULL OR a.Date   <= @ToDate)
            GROUP BY s.StudentId, s.Name, s.RollNo, s.Email, a.Subject, s.Status
            ORDER BY a.Subject
            """;
        var dt = _db.ExecuteQuery(sql,
            DbHelper.Param("@StudentId",            studentId),
            DbHelper.ParamNullable("@Subject",  subject),
            DbHelper.ParamNullable("@FromDate", from.HasValue ? from.Value.ToDateTime(TimeOnly.MinValue) : null),
            DbHelper.ParamNullable("@ToDate",   to.HasValue   ? to.Value.ToDateTime(TimeOnly.MinValue)   : null));

        return dt.Rows.Cast<System.Data.DataRow>().Select(r => new AttendanceSummaryDto
        {
            StudentId         = (int)    r["StudentId"],
            StudentName       = (string) r["StudentName"],
            RollNo            = (string) r["RollNo"],
            StudentEmail      = (string) r["StudentEmail"],
            Subject           = (string) r["Subject"],
            TotalClasses      = (int)    r["TotalClasses"],
            Present           = (int)    r["Present"],
            Absent            = (int)    r["Absent"],
            Late              = (int)    r["Late"],
            AttendancePercent = r["AttendancePercent"] == DBNull.Value ? 0m : (decimal)r["AttendancePercent"],
            Status            = (string) r["Status"]
        }).ToList();
    }

    public List<AttendanceRecordDto> GetStudentDetail(int studentId, string? subject, DateOnly? from, DateOnly? to)
    {
        const string sql = """
            SELECT a.AttendanceId, a.StudentId, s.Name AS StudentName, s.RollNo,
                   a.Subject, a.Date, a.Status, c.ClassName, sem.SemesterNumber
            FROM   Attendance  a
            JOIN   Students    s   ON s.StudentId   = a.StudentId
            JOIN   Classes     c   ON c.ClassId     = a.ClassId
            JOIN   Semesters   sem ON sem.SemesterId = a.SemesterId
            WHERE  a.StudentId = @StudentId
              AND  (@Subject  IS NULL OR a.Subject = @Subject)
              AND  (@FromDate IS NULL OR a.Date   >= @FromDate)
              AND  (@ToDate   IS NULL OR a.Date   <= @ToDate)
            ORDER BY a.Date DESC
            """;
        var dt = _db.ExecuteQuery(sql,
            DbHelper.Param("@StudentId",            studentId),
            DbHelper.ParamNullable("@Subject",  subject),
            DbHelper.ParamNullable("@FromDate", from.HasValue ? from.Value.ToDateTime(TimeOnly.MinValue) : null),
            DbHelper.ParamNullable("@ToDate",   to.HasValue   ? to.Value.ToDateTime(TimeOnly.MinValue)   : null));

        return dt.Rows.Cast<System.Data.DataRow>().Select(r => new AttendanceRecordDto
        {
            AttendanceId   = (int)    r["AttendanceId"],
            StudentId      = (int)    r["StudentId"],
            StudentName    = (string) r["StudentName"],
            RollNo         = (string) r["RollNo"],
            Subject        = (string) r["Subject"],
            Date           = DateOnly.FromDateTime((DateTime)r["Date"]),
            Status         = (string) r["Status"],
            ClassName      = (string) r["ClassName"],
            SemesterNumber = (int)    r["SemesterNumber"]
        }).ToList();
    }
}

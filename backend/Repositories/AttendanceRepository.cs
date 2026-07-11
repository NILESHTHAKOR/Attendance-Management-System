using AttendanceMS.Data;
using AttendanceMS.DTOs;
using AttendanceMS.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace AttendanceMS.Repositories;

public sealed class AttendanceRepository : IAttendanceRepository
{
    private readonly DbHelper _db;
    public AttendanceRepository(DbHelper db) => _db = db;

    public bool HasBeenMarked(int studentId, int classId, string subject, DateOnly date)
    {
        const string sql = """
            SELECT COUNT(1) FROM Attendance
            WHERE StudentId = @StudentId AND ClassId = @ClassId
              AND Subject = @Subject AND Date = @Date
            """;
        return _db.ExecuteScalar<int>(sql,
            DbHelper.Param("@StudentId", studentId),
            DbHelper.Param("@ClassId",   classId),
            DbHelper.Param("@Subject",   subject),
            DbHelper.Param("@Date",      date.ToDateTime(TimeOnly.MinValue))) > 0;
    }

    public bool AnyMarkedForClassSubjectDate(int classId, string subject, DateOnly date)
    {
        const string sql = """
            SELECT COUNT(1) FROM Attendance
            WHERE ClassId = @ClassId AND Subject = @Subject AND Date = @Date
            """;
        return _db.ExecuteScalar<int>(sql,
            DbHelper.Param("@ClassId", classId),
            DbHelper.Param("@Subject", subject),
            DbHelper.Param("@Date",    date.ToDateTime(TimeOnly.MinValue))) > 0;
    }

    public void MarkBulk(IEnumerable<MarkAttendanceDto> records)
    {
        const string sql = """
            IF NOT EXISTS (
                SELECT 1 FROM Attendance
                WHERE StudentId=@StudentId AND ClassId=@ClassId
                  AND Subject=@Subject AND Date=@Date)
            BEGIN
                INSERT INTO Attendance (StudentId, ClassId, SemesterId, Subject, Date, Status)
                VALUES (@StudentId, @ClassId, @SemesterId, @Subject, @Date, @Status)
            END
            ELSE
            BEGIN
                UPDATE Attendance
                SET    Status   = @Status,
                       MarkedAt = GETDATE()
                WHERE  StudentId=@StudentId AND ClassId=@ClassId
                  AND  Subject=@Subject AND Date=@Date
            END
            """;

        foreach (var r in records)
        {
            _db.ExecuteNonQuery(sql,
                DbHelper.Param("@StudentId",  r.StudentId),
                DbHelper.Param("@ClassId",    r.ClassId),
                DbHelper.Param("@SemesterId", r.SemesterId),
                DbHelper.Param("@Subject",    r.Subject),
                DbHelper.Param("@Date",       r.Date.ToDateTime(TimeOnly.MinValue)),
                DbHelper.Param("@Status",     r.Status));
        }
    }

    public List<AttendanceRecordDto> GetForStudent(int studentId, string? subject, DateOnly? from, DateOnly? to)
    {
        const string sql = """
            SELECT a.AttendanceId, a.StudentId, s.Name AS StudentName, s.RollNo,
                   a.Subject, a.Date, a.Status,
                   c.ClassName, sem.SemesterNumber
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
            DbHelper.Param("@StudentId", studentId),
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

    public List<AttendanceSummaryDto> GetClassSummary(int? semesterId, int? classId, string? subject, DateOnly? from, DateOnly? to)
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
            JOIN   Attendance a ON a.StudentId  = s.StudentId
            WHERE  (@SemesterId IS NULL OR a.SemesterId = @SemesterId)
              AND  (@ClassId    IS NULL OR a.ClassId    = @ClassId)
              AND  (@Subject    IS NULL OR a.Subject    = @Subject)
              AND  (@FromDate   IS NULL OR a.Date      >= @FromDate)
              AND  (@ToDate     IS NULL OR a.Date      <= @ToDate)
            GROUP BY s.StudentId, s.Name, s.RollNo, s.Email, a.Subject, s.Status
            ORDER BY s.RollNo, a.Subject
            """;

        var dt = _db.ExecuteQuery(sql,
            DbHelper.ParamNullable("@SemesterId", semesterId),
            DbHelper.ParamNullable("@ClassId",    classId),
            DbHelper.ParamNullable("@Subject",    subject),
            DbHelper.ParamNullable("@FromDate",   from.HasValue ? from.Value.ToDateTime(TimeOnly.MinValue) : null),
            DbHelper.ParamNullable("@ToDate",     to.HasValue   ? to.Value.ToDateTime(TimeOnly.MinValue)   : null));

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

    public List<string> GetDistinctSubjectsByFaculty(int facultyId)
    {
        const string sql = """
            SELECT DISTINCT Subject FROM Timetables
            WHERE FacultyId = @FacultyId ORDER BY Subject
            """;
        var dt = _db.ExecuteQuery(sql, DbHelper.Param("@FacultyId", facultyId));
        return dt.Rows.Cast<System.Data.DataRow>().Select(r => r["Subject"].ToString()!).ToList();
    }

    public List<string> GetDistinctSubjectsByStudent(int studentId)
    {
        const string sql = """
            SELECT DISTINCT Subject FROM Attendance
            WHERE StudentId = @StudentId ORDER BY Subject
            """;
        var dt = _db.ExecuteQuery(sql, DbHelper.Param("@StudentId", studentId));
        return dt.Rows.Cast<System.Data.DataRow>().Select(r => r["Subject"].ToString()!).ToList();
    }
}

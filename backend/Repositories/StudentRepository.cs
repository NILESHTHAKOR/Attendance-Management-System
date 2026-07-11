using AttendanceMS.Data;
using AttendanceMS.Models;
using AttendanceMS.Repositories.Interfaces;

namespace AttendanceMS.Repositories;

public sealed class StudentRepository : IStudentRepository
{
    private readonly DbHelper _db;
    public StudentRepository(DbHelper db) => _db = db;

    private static Student Map(System.Data.DataRow r) => new()
    {
        StudentId         = (int)    r["StudentId"],
        UserId            = r["UserId"] == DBNull.Value ? null : (int?)r["UserId"],
        Name              = (string) r["Name"],
        Email             = (string) r["Email"],
        Phone             = (string) r["Phone"],
        RollNo            = (string) r["RollNo"],
        SemesterId        = (int)    r["SemesterId"],
        ClassId           = (int)    r["ClassId"],
        AttendancePercent = (decimal)r["AttendancePercent"],
        Status            = (string) r["Status"],
        CreatedAt         = (DateTime)r["CreatedAt"],
        SemesterNumber    = r.Table.Columns.Contains("SemesterNumber")
                                ? r["SemesterNumber"].ToString()! : string.Empty,
        ClassName         = r.Table.Columns.Contains("ClassName")
                                ? r["ClassName"].ToString()! : string.Empty
    };

    public Student? GetById(int studentId)
    {
        const string sql = """
            SELECT s.*, sem.SemesterNumber, c.ClassName
            FROM   Students s
            JOIN   Semesters sem ON sem.SemesterId = s.SemesterId
            JOIN   Classes   c   ON c.ClassId      = s.ClassId
            WHERE  s.StudentId = @StudentId
            """;
        var dt = _db.ExecuteQuery(sql, DbHelper.Param("@StudentId", studentId));
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    public Student? GetByUserId(int userId)
    {
        const string sql = """
            SELECT s.*, sem.SemesterNumber, c.ClassName
            FROM   Students s
            JOIN   Semesters sem ON sem.SemesterId = s.SemesterId
            JOIN   Classes   c   ON c.ClassId      = s.ClassId
            WHERE  s.UserId = @UserId
            """;
        var dt = _db.ExecuteQuery(sql, DbHelper.Param("@UserId", userId));
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    public List<Student> GetByClass(int classId, int semesterId)
    {
        const string sql = """
            SELECT s.*, sem.SemesterNumber, c.ClassName
            FROM   Students s
            JOIN   Semesters sem ON sem.SemesterId = s.SemesterId
            JOIN   Classes   c   ON c.ClassId      = s.ClassId
            WHERE  s.ClassId    = @ClassId
              AND  s.SemesterId = @SemesterId
            ORDER BY s.RollNo
            """;
        var dt = _db.ExecuteQuery(sql,
            DbHelper.Param("@ClassId",    classId),
            DbHelper.Param("@SemesterId", semesterId));
        return dt.Rows.Cast<System.Data.DataRow>().Select(Map).ToList();
    }

    public List<Student> GetAll()
    {
        const string sql = """
            SELECT s.*, sem.SemesterNumber, c.ClassName
            FROM   Students s
            JOIN   Semesters sem ON sem.SemesterId = s.SemesterId
            JOIN   Classes   c   ON c.ClassId      = s.ClassId
            ORDER BY s.SemesterId, s.ClassId, s.RollNo
            """;
        var dt = _db.ExecuteQuery(sql);
        return dt.Rows.Cast<System.Data.DataRow>().Select(Map).ToList();
    }

    public void UpdateStatus(int studentId, string status, decimal percent)
    {
        const string sql = """
            UPDATE Students
            SET    Status = @Status, AttendancePercent = @Percent
            WHERE  StudentId = @StudentId
            """;
        _db.ExecuteNonQuery(sql,
            DbHelper.Param("@Status",    status),
            DbHelper.Param("@Percent",   percent),
            DbHelper.Param("@StudentId", studentId));
    }
}

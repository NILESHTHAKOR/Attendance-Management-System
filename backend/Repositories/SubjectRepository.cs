using AttendanceMS.Data;
using AttendanceMS.Models;
using AttendanceMS.Repositories.Interfaces;

namespace AttendanceMS.Repositories;

public sealed class SubjectRepository : ISubjectRepository
{
    private readonly DbHelper _db;
    public SubjectRepository(DbHelper db) => _db = db;

    public List<Subject> GetAll()
    {
        const string sql = """
            SELECT SubjectId, SubjectName, SemesterId, ClassId, IsActive, CreatedAt
            FROM Subjects WHERE IsActive=1 ORDER BY SubjectName
            """;
        var dt = _db.ExecuteQuery(sql);
        return dt.Rows.Cast<System.Data.DataRow>().Select(Map).ToList();
    }

    public List<string> GetNames(int? semesterId = null, int? classId = null)
    {
        const string sql = """
            SELECT SubjectName FROM Subjects
            WHERE IsActive=1
              AND (SemesterId IS NULL OR SemesterId=@SemesterId OR @SemesterId IS NULL)
              AND (ClassId    IS NULL OR ClassId   =@ClassId    OR @ClassId    IS NULL)
            ORDER BY SubjectName
            """;
        var dt = _db.ExecuteQuery(sql,
            DbHelper.ParamNullable("@SemesterId", semesterId),
            DbHelper.ParamNullable("@ClassId",    classId));
        return dt.Rows.Cast<System.Data.DataRow>().Select(r => (string)r["SubjectName"]).ToList();
    }

    public void Create(Subject s)
    {
        const string sql = """
            INSERT INTO Subjects (SubjectName, SemesterId, ClassId, IsActive)
            VALUES (@Name, @SemId, @ClsId, 1)
            """;
        _db.ExecuteNonQuery(sql,
            DbHelper.Param("@Name",  s.SubjectName),
            DbHelper.ParamNullable("@SemId", s.SemesterId),
            DbHelper.ParamNullable("@ClsId", s.ClassId));
    }

    public void Update(Subject s)
    {
        const string sql = """
            UPDATE Subjects SET SubjectName=@Name, SemesterId=@SemId, ClassId=@ClsId
            WHERE SubjectId=@Id
            """;
        _db.ExecuteNonQuery(sql,
            DbHelper.Param("@Name",  s.SubjectName),
            DbHelper.ParamNullable("@SemId", s.SemesterId),
            DbHelper.ParamNullable("@ClsId", s.ClassId),
            DbHelper.Param("@Id",    s.SubjectId));
    }

    public void Delete(int subjectId)
    {
        const string sql = "UPDATE Subjects SET IsActive=0 WHERE SubjectId=@Id";
        _db.ExecuteNonQuery(sql, DbHelper.Param("@Id", subjectId));
    }

    private static Subject Map(System.Data.DataRow r) => new()
    {
        SubjectId   = (int)    r["SubjectId"],
        SubjectName = (string) r["SubjectName"],
        SemesterId  = r["SemesterId"] == DBNull.Value ? null : (int?)r["SemesterId"],
        ClassId     = r["ClassId"]    == DBNull.Value ? null : (int?)r["ClassId"],
        IsActive    = (bool)   r["IsActive"],
        CreatedAt   = (DateTime)r["CreatedAt"]
    };
}

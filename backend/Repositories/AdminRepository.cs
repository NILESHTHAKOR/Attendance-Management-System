using AttendanceMS.Data;
using AttendanceMS.Models;
using AttendanceMS.Repositories.Interfaces;

namespace AttendanceMS.Repositories;

public sealed class AdminRepository : IAdminRepository
{
    private readonly DbHelper _db;
    public AdminRepository(DbHelper db) => _db = db;

    public List<Faculty> GetAllFaculty()
    {
        const string sql = """
            SELECT f.FacultyId, f.UserId, f.Name, f.Email, f.Subject, f.PasswordHash, f.CreatedAt,
                   u.Password AS PlainPassword
            FROM Faculty f LEFT JOIN Users u ON u.Id = f.UserId
            ORDER BY f.Name
            """;
        var dt = _db.ExecuteQuery(sql);
        return dt.Rows.Cast<System.Data.DataRow>().Select(r => new Faculty
        {
            FacultyId = (int)r["FacultyId"],
            UserId = r["UserId"] == DBNull.Value ? null : (int?)r["UserId"],
            Name = (string)r["Name"],
            Email = (string)r["Email"],
            Subject = (string)r["Subject"],
            PasswordHash = r["PlainPassword"] == DBNull.Value ? "" : (string)r["PlainPassword"],
            CreatedAt = (DateTime)r["CreatedAt"]
        }).ToList();
    }

    public void CreateFaculty(Faculty f, string password)
    {
        // Create user first
        const string userSql = """
            INSERT INTO Users (Name, Email, Role, Password, IsActive)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Email, 'faculty', @Password, 1)
            """;
        var dt = _db.ExecuteQuery(userSql,
            DbHelper.Param("@Name", f.Name),
            DbHelper.Param("@Email", f.Email),
            DbHelper.Param("@Password", password));
        int userId = (int)dt.Rows[0][0];

        const string facSql = """
            INSERT INTO Faculty (UserId, Name, Email, Subject, PasswordHash)
            VALUES (@UserId, @Name, @Email, @Subject, @PwdHash)
            """;
        _db.ExecuteNonQuery(facSql,
            DbHelper.Param("@UserId", userId),
            DbHelper.Param("@Name", f.Name),
            DbHelper.Param("@Email", f.Email),
            DbHelper.Param("@Subject", f.Subject),
            DbHelper.Param("@PwdHash", password));
    }

    public void UpdateFaculty(Faculty f)
    {
        const string facSql = """
            UPDATE Faculty SET Name=@Name, Email=@Email, Subject=@Subject WHERE FacultyId=@Id
            """;
        _db.ExecuteNonQuery(facSql,
            DbHelper.Param("@Name", f.Name),
            DbHelper.Param("@Email", f.Email),
            DbHelper.Param("@Subject", f.Subject),
            DbHelper.Param("@Id", f.FacultyId));

        if (f.UserId.HasValue)
        {
            const string userSql = "UPDATE Users SET Name=@Name, Email=@Email WHERE Id=@Id";
            _db.ExecuteNonQuery(userSql,
                DbHelper.Param("@Name", f.Name),
                DbHelper.Param("@Email", f.Email),
                DbHelper.Param("@Id", f.UserId.Value));
        }
    }

    public void DeleteFaculty(int facultyId)
    {
        // Soft-delete: deactivate user
        const string sql = """
            UPDATE Users SET IsActive=0
            WHERE Id = (SELECT UserId FROM Faculty WHERE FacultyId=@Id)
            """;
        _db.ExecuteNonQuery(sql, DbHelper.Param("@Id", facultyId));
    }

    public void CreateStudent(Student s, string password)
    {
        const string userSql = """
            INSERT INTO Users (Name, Email, Role, Password, IsActive)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Email, 'student', @Password, 1)
            """;
        var dt = _db.ExecuteQuery(userSql,
            DbHelper.Param("@Name", s.Name),
            DbHelper.Param("@Email", s.Email),
            DbHelper.Param("@Password", password));
        int userId = (int)dt.Rows[0][0];

        const string stuSql = """
            INSERT INTO Students (UserId, Name, Email, Phone, RollNo, SemesterId, ClassId, AttendancePercent, Status)
            VALUES (@UserId, @Name, @Email, @Phone, @RollNo, @SemId, @ClsId, 0, 'active')
            """;
        _db.ExecuteNonQuery(stuSql,
            DbHelper.Param("@UserId", userId),
            DbHelper.Param("@Name", s.Name),
            DbHelper.Param("@Email", s.Email),
            DbHelper.Param("@Phone", s.Phone),
            DbHelper.Param("@RollNo", s.RollNo),
            DbHelper.Param("@SemId", s.SemesterId),
            DbHelper.Param("@ClsId", s.ClassId));
    }

    public void BulkCreateStudents(List<(Student student, string password)> students)
    {
        foreach (var (s, pwd) in students)
        {
            // Skip if email already exists
            var check = _db.ExecuteQuery("SELECT COUNT(1) AS Cnt FROM Users WHERE Email=@E",
                DbHelper.Param("@E", s.Email));
            if ((int)check.Rows[0]["Cnt"] > 0) continue;
            CreateStudent(s, pwd);
        }
    }
}

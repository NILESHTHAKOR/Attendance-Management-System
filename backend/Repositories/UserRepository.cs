using AttendanceMS.Data;
using AttendanceMS.Models;
using AttendanceMS.Repositories.Interfaces;

namespace AttendanceMS.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly DbHelper _db;
    public UserRepository(DbHelper db) => _db = db;

    public User? GetByEmail(string email)
    {
        const string sql = """
            SELECT Id, Name, Email, Role, Password, IsActive, CreatedAt
            FROM   Users
            WHERE  Email = @Email AND IsActive = 1
            """;

        var dt = _db.ExecuteQuery(sql, DbHelper.Param("@Email", email));
        if (dt.Rows.Count == 0) return null;
        return MapUser(dt.Rows[0]);
    }

    public User? GetById(int id)
    {
        const string sql = """
            SELECT Id, Name, Email, Role, Password, IsActive, CreatedAt
            FROM   Users
            WHERE  Id = @Id
            """;

        var dt = _db.ExecuteQuery(sql, DbHelper.Param("@Id", id));
        if (dt.Rows.Count == 0) return null;
        return MapUser(dt.Rows[0]);
    }

    public List<User> GetAll()
    {
        const string sql = """
            SELECT Id, Name, Email, Role, Password, IsActive, CreatedAt
            FROM   Users
            ORDER BY Role, Name
            """;
        var dt = _db.ExecuteQuery(sql);
        return dt.Rows.Cast<System.Data.DataRow>().Select(MapUser).ToList();
    }

    public void Create(User user)
    {
        const string sql = """
            INSERT INTO Users (Name, Email, Role, Password, IsActive)
            VALUES (@Name, @Email, @Role, @Password, 1)
            """;
        _db.ExecuteNonQuery(sql,
            DbHelper.Param("@Name", user.Name),
            DbHelper.Param("@Email", user.Email),
            DbHelper.Param("@Role", user.Role),
            DbHelper.Param("@Password", user.Password));
    }

    public void Update(User user)
    {
        const string sql = """
            UPDATE Users SET Name=@Name, Email=@Email, Role=@Role,
                             Password=@Password, IsActive=@IsActive
            WHERE  Id=@Id
            """;
        _db.ExecuteNonQuery(sql,
            DbHelper.Param("@Name", user.Name),
            DbHelper.Param("@Email", user.Email),
            DbHelper.Param("@Role", user.Role),
            DbHelper.Param("@Password", user.Password),
            DbHelper.Param("@IsActive", user.IsActive),
            DbHelper.Param("@Id", user.Id));
    }

    public void Delete(int id)
    {
        const string sql = "UPDATE Users SET IsActive=0 WHERE Id=@Id";
        _db.ExecuteNonQuery(sql, DbHelper.Param("@Id", id));
    }

    private static User MapUser(System.Data.DataRow r) => new()
    {
        Id = (int)r["Id"],
        Name = (string)r["Name"],
        Email = (string)r["Email"],
        Role = (string)r["Role"],
        Password = r["Password"] == DBNull.Value ? "" : (string)r["Password"],
        IsActive = (bool)r["IsActive"],
        CreatedAt = (DateTime)r["CreatedAt"]
    };
}

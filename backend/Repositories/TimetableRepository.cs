using AttendanceMS.Data;
using AttendanceMS.Models;
using AttendanceMS.Repositories.Interfaces;

namespace AttendanceMS.Repositories;

public sealed class TimetableRepository : ITimetableRepository
{
    private readonly DbHelper _db;
    public TimetableRepository(DbHelper db) => _db = db;

    public List<TimetableSlot> GetByFaculty(int facultyId)
    {
        const string sql = """
            SELECT t.TimetableId, t.Subject, t.ClassId, c.ClassName,
                   t.SemesterId, sem.SemesterNumber, t.StartTime, t.EndTime, t.DayOfWeek
            FROM Timetable t
            JOIN Classes   c   ON c.ClassId    = t.ClassId
            JOIN Semesters sem ON sem.SemesterId = t.SemesterId
            WHERE t.FacultyId=@FId AND t.IsActive=1
            ORDER BY
              CASE t.DayOfWeek WHEN 'Monday' THEN 1 WHEN 'Tuesday' THEN 2
                WHEN 'Wednesday' THEN 3 WHEN 'Thursday' THEN 4
                WHEN 'Friday' THEN 5 WHEN 'Saturday' THEN 6 ELSE 7 END,
              t.StartTime
            """;
        var dt = _db.ExecuteQuery(sql, DbHelper.Param("@FId", facultyId));
        return dt.Rows.Cast<System.Data.DataRow>().Select(Map).ToList();
    }

    public List<TimetableSlot> GetTodayByFaculty(int facultyId)
    {
        string today = DateTime.Today.DayOfWeek.ToString();
        const string sql = """
            SELECT t.TimetableId, t.Subject, t.ClassId, c.ClassName,
                   t.SemesterId, sem.SemesterNumber, t.StartTime, t.EndTime, t.DayOfWeek
            FROM Timetable t
            JOIN Classes   c   ON c.ClassId    = t.ClassId
            JOIN Semesters sem ON sem.SemesterId = t.SemesterId
            WHERE t.FacultyId=@FId AND t.DayOfWeek=@Day AND t.IsActive=1
            ORDER BY t.StartTime
            """;
        var dt = _db.ExecuteQuery(sql,
            DbHelper.Param("@FId", facultyId),
            DbHelper.Param("@Day", today));
        return dt.Rows.Cast<System.Data.DataRow>().Select(Map).ToList();
    }

    public void Create(TimetableEntry e)
    {
        const string sql = """
            INSERT INTO Timetable (FacultyId, SemesterId, ClassId, Subject, DayOfWeek, StartTime, EndTime, IsActive)
            VALUES (@FId, @SemId, @ClsId, @Sub, @Day, @Start, @End, 1)
            """;
        _db.ExecuteNonQuery(sql,
            DbHelper.Param("@FId",   e.FacultyId),
            DbHelper.Param("@SemId", e.SemesterId),
            DbHelper.Param("@ClsId", e.ClassId),
            DbHelper.Param("@Sub",   e.Subject),
            DbHelper.Param("@Day",   e.DayOfWeek),
            DbHelper.Param("@Start", e.StartTime),
            DbHelper.Param("@End",   e.EndTime));
    }

    public void Delete(int timetableId)
    {
        const string sql = "UPDATE Timetable SET IsActive=0 WHERE TimetableId=@Id";
        _db.ExecuteNonQuery(sql, DbHelper.Param("@Id", timetableId));
    }

    private static TimetableSlot Map(System.Data.DataRow r) => new(
        TimetableId:    (int)   r["TimetableId"],
        Subject:        (string)r["Subject"],
        ClassId:        (int)   r["ClassId"],
        ClassName:      (string)r["ClassName"],
        SemesterId:     (int)   r["SemesterId"],
        SemesterNumber: (int)   r["SemesterNumber"],
        StartTime:      (string)r["StartTime"],
        EndTime:        (string)r["EndTime"],
        DayOfWeek:      (string)r["DayOfWeek"]
    );
}

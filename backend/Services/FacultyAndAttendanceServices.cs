using AttendanceMS.Data;
using AttendanceMS.DTOs;
using AttendanceMS.Models;
using AttendanceMS.Repositories.Interfaces;
using AttendanceMS.Services.Interfaces;
using AttendanceMS.ViewModels;

namespace AttendanceMS.Services;

// ════════════════════════════════════════════════════════
//  FacultyService
// ════════════════════════════════════════════════════════
public sealed class FacultyService : IFacultyService
{
    private readonly IFacultyRepository _faculty;
    private readonly IStudentRepository _students;
    private readonly IAttendanceRepository _attendance;
    private readonly IThresholdRepository _thresholds;
    private readonly DbHelper _db;

    public FacultyService(
        IFacultyRepository faculty,
        IStudentRepository students,
        IAttendanceRepository attendance,
        IThresholdRepository thresholds,
        DbHelper db)
    {
        _faculty = faculty;
        _students = students;
        _attendance = attendance;
        _thresholds = thresholds;
        _db = db;
    }

    public Faculty? GetByUserId(int userId) => _faculty.GetByUserId(userId);

    public FacultyDashboardViewModel GetDashboard(int userId)
    {
        var fac = _faculty.GetByUserId(userId)
            ?? throw new InvalidOperationException("Faculty not found");

        var threshold = _thresholds.GetForFaculty(fac.FacultyId, null, null)
                     ?? _thresholds.GetGlobal();
        decimal bl = threshold?.BlacklistThreshold ?? 50m;
        decimal warn = threshold?.WarningThreshold ?? 75m;

        var allStudents = _students.GetAll();
        var todaySlots = _faculty.GetTodaySlots(fac.FacultyId);

        // Build "today slots" with already-marked flag
        var todaySlotVms = todaySlots.Select(ts => new TodaySlotViewModel
        {
            TimetableId = ts.TimetableId,
            Subject = ts.Subject,
            ClassId = ts.ClassId,
            ClassName = ts.ClassName,
            SemesterId = ts.SemesterId,
            SemesterNumber = ts.SemesterNumber,
            StartTime = ts.StartTime,
            EndTime = ts.EndTime,
            AlreadyMarked = _attendance.AnyMarkedForClassSubjectDate(
                                ts.ClassId, ts.Subject, DateOnly.FromDateTime(DateTime.Today))
        }).ToList();

        // Recent 5 attendance marks
        var recentMarked = GetRecentMarked(fac.FacultyId);

        return new FacultyDashboardViewModel
        {
            FacultyId = fac.FacultyId,
            FacultyName = fac.Name,
            Subject = fac.Subject,
            TotalStudents = allStudents.Count,
            ActiveStudents = allStudents.Count(s => s.Status == "active"),
            WarningStudents = allStudents.Count(s => s.Status == "warning"),
            BlacklistedStudents = allStudents.Count(s => s.Status == "blacklisted"),
            TodaySlots = todaySlotVms,
            RecentMarked = recentMarked,
            BlacklistThreshold = bl,
            WarningThreshold = warn
        };
    }

    public List<SemesterSelectItem> GetSemesters()
    {
        const string sql = "SELECT SemesterId, SemesterNumber FROM Semesters ORDER BY SemesterNumber";
        var dt = _db.ExecuteQuery(sql);
        return dt.Rows.Cast<System.Data.DataRow>().Select(r => new SemesterSelectItem
        {
            SemesterId = (int)r["SemesterId"],
            SemesterNumber = (int)r["SemesterNumber"]
        }).ToList();
    }

    public List<ClassSelectItem> GetClasses(int? semesterId = null)
    {
        string sql = semesterId.HasValue
            ? "SELECT ClassId, ClassName, SemesterId FROM Classes WHERE SemesterId = @SemesterId ORDER BY ClassName"
            : "SELECT ClassId, ClassName, SemesterId FROM Classes ORDER BY SemesterId, ClassName";

        var dt = semesterId.HasValue
            ? _db.ExecuteQuery(sql, DbHelper.Param("@SemesterId", semesterId.Value))
            : _db.ExecuteQuery(sql);

        return dt.Rows.Cast<System.Data.DataRow>().Select(r => new ClassSelectItem
        {
            ClassId = (int)r["ClassId"],
            ClassName = (string)r["ClassName"],
            SemesterId = (int)r["SemesterId"]
        }).ToList();
    }

    public List<string> GetSubjectsByFaculty(int facultyId)
        => _attendance.GetDistinctSubjectsByFaculty(facultyId);

    public List<TimetableSlot> GetAllSlots(int facultyId)
        => _faculty.GetAllSlots(facultyId);

    private List<RecentMarkViewModel> GetRecentMarked(int facultyId)
    {
        const string sql = """
            SELECT TOP 5
                a.Date, a.Subject, c.ClassName,
                SUM(CASE WHEN a.Status='present' THEN 1 ELSE 0 END) AS PresentCount,
                SUM(CASE WHEN a.Status='absent'  THEN 1 ELSE 0 END) AS AbsentCount,
                SUM(CASE WHEN a.Status='late'    THEN 1 ELSE 0 END) AS LateCount
            FROM   Attendance a
            JOIN   Students   s ON s.StudentId = a.StudentId
            JOIN   Classes    c ON c.ClassId   = a.ClassId
            JOIN   Timetables t ON t.ClassId   = a.ClassId AND t.Subject = a.Subject
            WHERE  t.FacultyId = @FacultyId
            GROUP BY a.Date, a.Subject, c.ClassName
            ORDER BY a.Date DESC
            """;
        var dt = _db.ExecuteQuery(sql, DbHelper.Param("@FacultyId", facultyId));
        return dt.Rows.Cast<System.Data.DataRow>().Select(r => new RecentMarkViewModel
        {
            Date = DateOnly.FromDateTime((DateTime)r["Date"]),
            Subject = (string)r["Subject"],
            ClassName = (string)r["ClassName"],
            PresentCount = (int)r["PresentCount"],
            AbsentCount = (int)r["AbsentCount"],
            LateCount = (int)r["LateCount"]
        }).ToList();
    }
}

// ════════════════════════════════════════════════════════
//  AttendanceService
// ════════════════════════════════════════════════════════
public sealed class AttendanceService : IAttendanceService
{
    private readonly IStudentRepository _students;
    private readonly IAttendanceRepository _attendance;
    private readonly IThresholdRepository _thresholds;
    private readonly IFacultyService _facultySvc;
    private readonly IEmailService _email;
    private readonly IStudentRepository _studentRepo;

    public AttendanceService(
        IStudentRepository students,
        IAttendanceRepository attendance,
        IThresholdRepository thresholds,
        IFacultyService facultySvc,
        IEmailService email)
    {
        _students = students;
        _attendance = attendance;
        _thresholds = thresholds;
        _facultySvc = facultySvc;
        _email = email;
        _studentRepo = students;
    }

    public MarkAttendanceViewModel BuildMarkForm(int facultyId, int? semesterId, int? classId, string? subject)
    {
        var vm = new MarkAttendanceViewModel
        {
            FacultyId = facultyId,
            SemesterId = semesterId ?? 0,
            ClassId = classId ?? 0,
            Subject = subject ?? string.Empty,
            Date = DateOnly.FromDateTime(DateTime.Today),
            Semesters = _facultySvc.GetSemesters(),
            Classes = _facultySvc.GetClasses(semesterId),
            Subjects = _facultySvc.GetSubjectsByFaculty(facultyId)
        };

        if (semesterId.HasValue && classId.HasValue && !string.IsNullOrEmpty(subject))
        {
            var studentList = _students.GetByClass(classId.Value, semesterId.Value);
            vm.Students = studentList.Select(s => new AttendanceEntryRow
            {
                StudentId = s.StudentId,
                Name = s.Name,
                RollNo = s.RollNo,
                Status = "present",
                AlreadyMarked = _attendance.HasBeenMarked(s.StudentId, classId.Value, subject, DateOnly.FromDateTime(DateTime.Today))
            }).ToList();
        }

        return vm;
    }

    public (bool success, string message) SaveAttendance(int facultyId, MarkAttendanceViewModel form)
    {
        if (form.Date != DateOnly.FromDateTime(DateTime.Today))
            return (false, "Attendance can only be marked for today.");

        if (form.Students.Count == 0)
            return (false, "No students found for the selected class.");

        var records = form.Students.Select(s => new MarkAttendanceDto
        {
            StudentId = s.StudentId,
            ClassId = form.ClassId,
            SemesterId = form.SemesterId,
            Subject = form.Subject,
            Date = form.Date,
            Status = s.Status
        }).ToList();

        _attendance.MarkBulk(records);

        // Recalculate status for each student and trigger emails if needed
        var threshold = _thresholds.GetForFaculty(facultyId, form.SemesterId, form.ClassId)
                     ?? _thresholds.GetGlobal();
        decimal bl = threshold?.BlacklistThreshold ?? 50m;
        decimal warn = threshold?.WarningThreshold ?? 75m;

        foreach (var row in form.Students)
        {
            var student = _studentRepo.GetById(row.StudentId);
            if (student is null) continue;

            // Recalculate from DB
            var summaries = GetStudentPercent(row.StudentId, form.Subject);
            if (summaries == null) continue;

            string newStatus = CalcStatus(summaries.Value, bl, warn);
            string oldStatus = student.Status;

            _studentRepo.UpdateStatus(row.StudentId, newStatus, summaries.Value);

            // Fire email notifications (don't await — fire and forget for now)
            if (newStatus == "blacklisted" && oldStatus != "blacklisted")
                _ = _email.SendAttendanceWarningAsync(student.Email, student.Name, summaries.Value, "blacklisted");
            else if (newStatus == "warning" && oldStatus == "active")
                _ = _email.SendAttendanceWarningAsync(student.Email, student.Name, summaries.Value, "warning");
        }

        return (true, $"Attendance saved for {form.Students.Count} students.");
    }

    public ClassReportViewModel GetClassReport(int facultyId, int? semesterId, int? classId,
                                                string? subject, DateOnly? from, DateOnly? to)
    {
        var threshold = _thresholds.GetForFaculty(facultyId, semesterId, classId)
                     ?? _thresholds.GetGlobal();
        decimal bl = threshold?.BlacklistThreshold ?? 50m;
        decimal warn = threshold?.WarningThreshold ?? 75m;

        var summary = _attendance.GetClassSummary(semesterId, classId, subject, from, to);

        // Overlay dynamic status based on threshold
        foreach (var s in summary)
            s.Status = CalcStatus(s.AttendancePercent, bl, warn);

        return new ClassReportViewModel
        {
            FilterSemesterId = semesterId,
            FilterClassId = classId,
            FilterSubject = subject,
            FilterFromDate = from,
            FilterToDate = to,
            Semesters = _facultySvc.GetSemesters(),
            Classes = _facultySvc.GetClasses(semesterId),
            Subjects = _facultySvc.GetSubjectsByFaculty(facultyId),
            Summary = summary,
            BlacklistThreshold = bl,
            WarningThreshold = warn
        };
    }

    private decimal? GetStudentPercent(int studentId, string? subject)
    {
        // Quick recalculation inline
        var records = _attendance.GetForStudent(studentId, subject, null, null);
        if (records.Count == 0) return null;
        int presentLate = records.Count(r => r.Status is "present" or "late");
        return Math.Round((decimal)presentLate / records.Count * 100, 2);
    }

    private static string CalcStatus(decimal pct, decimal bl, decimal warn) =>
        pct < bl ? "blacklisted" : pct < warn ? "warning" : "active";
}

// ════════════════════════════════════════════════════════
//  ThresholdService
// ════════════════════════════════════════════════════════
public sealed class ThresholdService : IThresholdService
{
    private readonly IThresholdRepository _repo;
    private readonly IFacultyService _facultySvc;

    public ThresholdService(IThresholdRepository repo, IFacultyService facultySvc)
    {
        _repo = repo;
        _facultySvc = facultySvc;
    }

    public ThresholdViewModel GetForm(int facultyId)
    {
        var current = _repo.GetForFaculty(facultyId, null, null)
                   ?? _repo.GetGlobal();

        var all = _repo.GetAllByFaculty(facultyId);

        return new ThresholdViewModel
        {
            FacultyId = facultyId,
            BlacklistThreshold = current?.BlacklistThreshold ?? 50m,
            WarningThreshold = current?.WarningThreshold ?? 75m,
            IsGlobal = false,
            Semesters = _facultySvc.GetSemesters(),
            Classes = _facultySvc.GetClasses(),
            All = all   // ✅ DIRECT USE (no mapping)
        };
    }

    public void SaveThreshold(int facultyId, ThresholdViewModel vm)
    {
        if (vm.BlacklistThreshold >= vm.WarningThreshold)
            throw new ArgumentException("Blacklist threshold must be less than Warning threshold.");

        _repo.Upsert(new ThresholdDto
        {
            FacultyId = facultyId,
            SemesterId = vm.SemesterId,
            ClassId = vm.ClassId,
            BlacklistThreshold = vm.BlacklistThreshold,
            WarningThreshold = vm.WarningThreshold,
            IsGlobal = false
        });
    }

    public (decimal blacklist, decimal warning) GetEffectiveThreshold(int facultyId, int? semesterId, int? classId)
    {
        var t = _repo.GetForFaculty(facultyId, semesterId, classId)
             ?? _repo.GetGlobal();
        return (t?.BlacklistThreshold ?? 50m, t?.WarningThreshold ?? 75m);
    }
}

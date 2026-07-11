using AttendanceMS.DTOs;
using AttendanceMS.Models;
using AttendanceMS.Repositories.Interfaces;
using AttendanceMS.Services.Interfaces;
using AttendanceMS.ViewModels;

namespace AttendanceMS.Services;

// ════════════════════════════════════════════════════════
//  AuthService
// ════════════════════════════════════════════════════════
public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    public AuthService(IUserRepository users) => _users = users;

    public User? Authenticate(string email, string password)
    {
        var user = _users.GetByEmail(email);
        if (user is null) return null;
        return user.Password == password ? user : null;
    }
}

// ════════════════════════════════════════════════════════
//  StudentService
// ════════════════════════════════════════════════════════
public sealed class StudentService : IStudentService
{
    private readonly IStudentRepository _students;
    private readonly IReportRepository _reports;
    private readonly IAttendanceRepository _attendance;
    private readonly IThresholdRepository _thresholds;

    public StudentService(
        IStudentRepository students,
        IReportRepository reports,
        IAttendanceRepository attendance,
        IThresholdRepository thresholds)
    {
        _students = students;
        _reports = reports;
        _attendance = attendance;
        _thresholds = thresholds;
    }

    public Student? GetByUserId(int userId) => _students.GetByUserId(userId);

    public StudentDashboardViewModel GetDashboard(int userId)
    {
        var student = _students.GetByUserId(userId)
            ?? throw new InvalidOperationException("Student not found");

        var threshold = GetThreshold(null, null);

        var summaries = _reports.GetStudentSubjectSummary(student.StudentId, null, null, null);
        var recent = _reports.GetStudentDetail(student.StudentId, null, null, null).Take(10).ToList();

        return new StudentDashboardViewModel
        {
            StudentId = student.StudentId,
            Name = student.Name,
            RollNo = student.RollNo,
            Email = student.Email,
            ClassName = student.ClassName,
            SemesterNumber = int.TryParse(student.SemesterNumber, out var sem) ? sem : 0,
            AttendancePercent = student.AttendancePercent,
            Status = student.Status,
            SubjectSummaries = summaries.Select(s => MapSummary(s, threshold.bl, threshold.warn)).ToList(),
            RecentRecords = recent
        };
    }

    public StudentReportViewModel GetReport(int userId, string? subject, DateOnly? from, DateOnly? to)
    {
        var student = _students.GetByUserId(userId)
            ?? throw new InvalidOperationException("Student not found");

        var threshold = GetThreshold(null, null);
        var summaries = _reports.GetStudentSubjectSummary(student.StudentId, subject, from, to);
        var details = _reports.GetStudentDetail(student.StudentId, subject, from, to);
        var subjects = _attendance.GetDistinctSubjectsByStudent(student.StudentId);

        decimal overallPercent = summaries.Count == 0 ? 0m
            : summaries.Average(s => s.AttendancePercent);

        return new StudentReportViewModel
        {
            StudentId = student.StudentId,
            StudentName = student.Name,
            RollNo = student.RollNo,
            ClassName = student.ClassName,
            SemesterNumber = int.TryParse(student.SemesterNumber, out var semN) ? semN : 0,
            OverallPercent = Math.Round(overallPercent, 2),
            OverallStatus = CalcStatus(overallPercent, threshold.bl, threshold.warn),
            FilterSubject = subject,
            FilterFromDate = from,
            FilterToDate = to,
            Subjects = subjects,
            DetailRecords = details,
            SubjectSummaries = summaries.Select(s => MapSummary(s, threshold.bl, threshold.warn)).ToList()
        };
    }

    // ── Private helpers ────────────────────────────────────────────
    private (decimal bl, decimal warn) GetThreshold(int? semesterId, int? classId)
    {
        var global = _thresholds.GetGlobal();
        return (global?.BlacklistThreshold ?? 50m, global?.WarningThreshold ?? 75m);
    }

    private static SubjectAttendanceSummary MapSummary(AttendanceSummaryDto s, decimal bl, decimal warn) => new()
    {
        Subject = s.Subject,
        TotalClasses = s.TotalClasses,
        Present = s.Present,
        Absent = s.Absent,
        Late = s.Late,
        AttendancePercent = s.AttendancePercent,
        Status = CalcStatus(s.AttendancePercent, bl, warn)
    };

    private static string CalcStatus(decimal pct, decimal bl, decimal warn) =>
        pct < bl ? "blacklisted" : pct < warn ? "warning" : "active";
}
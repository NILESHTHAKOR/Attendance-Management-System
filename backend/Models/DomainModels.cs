namespace AttendanceMS.Models;

// ════════════════════════════════════════════════════════
//  Domain entities — mirror the DB tables exactly
// ════════════════════════════════════════════════════════

public sealed class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;   // student | faculty | admin
    public string Password { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class Student
{
    public int StudentId { get; set; }
    public int? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string RollNo { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public int ClassId { get; set; }
    public decimal AttendancePercent { get; set; }
    public string Status { get; set; } = "active";   // active|warning|blacklisted
    public DateTime CreatedAt { get; set; }

    // Populated via JOIN
    public string SemesterNumber { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
}

public sealed class Faculty
{
    public int FacultyId { get; set; }
    public int? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class Subject
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int? SemesterId { get; set; }
    public int? ClassId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public sealed class Semester
{
    public int SemesterId { get; set; }
    public int SemesterNumber { get; set; }
}

public sealed class Class
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
}

public sealed class Timetable
{
    public int TimetableId { get; set; }
    public int FacultyId { get; set; }
    public int SemesterId { get; set; }
    public int ClassId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;

    // Populated via JOIN
    public string FacultyName { get; set; } = string.Empty;
    public int SemesterNumber { get; set; }
    public string ClassName { get; set; } = string.Empty;
}

public sealed class Attendance
{
    public int AttendanceId { get; set; }
    public int StudentId { get; set; }
    public int ClassId { get; set; }
    public int SemesterId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string Status { get; set; } = "present";   // present|absent|late
    public DateTime MarkedAt { get; set; }

    // Populated via JOIN
    public string StudentName { get; set; } = string.Empty;
    public string RollNo { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int SemesterNumber { get; set; }
}

public sealed class ThresholdSetting
{
    public int ThresholdId { get; set; }
    public int? FacultyId { get; set; }
    public int? SemesterId { get; set; }
    public int? ClassId { get; set; }
    public decimal BlacklistThreshold { get; set; } = 50m;
    public decimal WarningThreshold { get; set; } = 75m;
    public bool IsGlobal { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class EmailNotificationLog
{
    public int LogId { get; set; }
    public int StudentId { get; set; }
    public string EmailType { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMsg { get; set; }
}
using System.ComponentModel.DataAnnotations;
using AttendanceMS.DTOs;
using AttendanceMS.Models;
namespace AttendanceMS.ViewModels;

// ════════════════════════════════════════════════════════
//  Authentication ViewModels
// ════════════════════════════════════════════════════════

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}

// ════════════════════════════════════════════════════════
//  Student ViewModels
// ════════════════════════════════════════════════════════

public sealed class StudentDashboardViewModel
{
    public int StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RollNo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int SemesterNumber { get; set; }
    public decimal AttendancePercent { get; set; }
    public string Status { get; set; } = string.Empty;

    // Subject-wise summary
    public List<SubjectAttendanceSummary> SubjectSummaries { get; set; } = new();
    // Recent 10 records
    public List<AttendanceRecordDto> RecentRecords { get; set; } = new();
}

public sealed class SubjectAttendanceSummary
{
    public string Subject { get; set; } = string.Empty;
    public int TotalClasses { get; set; }
    public int Present { get; set; }
    public int Absent { get; set; }
    public int Late { get; set; }
    public decimal AttendancePercent { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class StudentReportViewModel
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RollNo { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int SemesterNumber { get; set; }
    public decimal OverallPercent { get; set; }
    public string OverallStatus { get; set; } = string.Empty;

    // Filters applied
    public string? FilterSubject { get; set; }
    public DateOnly? FilterFromDate { get; set; }
    public DateOnly? FilterToDate { get; set; }

    // Available subjects for dropdown
    public List<string> Subjects { get; set; } = new();

    // Detailed records
    public List<AttendanceRecordDto> DetailRecords { get; set; } = new();
    public List<SubjectAttendanceSummary> SubjectSummaries { get; set; } = new();
}

// ════════════════════════════════════════════════════════
//  Faculty ViewModels
// ════════════════════════════════════════════════════════

public sealed class FacultyDashboardViewModel
{
    public int FacultyId { get; set; }
    public string FacultyName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;

    // Summary cards
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int WarningStudents { get; set; }
    public int BlacklistedStudents { get; set; }

    // Timetable for today
    public List<TodaySlotViewModel> TodaySlots { get; set; } = new();

    // Recent attendance marked
    public List<RecentMarkViewModel> RecentMarked { get; set; } = new();

    // Current threshold
    public decimal BlacklistThreshold { get; set; } = 50m;
    public decimal WarningThreshold { get; set; } = 75m;
}

public sealed class TodaySlotViewModel
{
    public int TimetableId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public int SemesterNumber { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public bool AlreadyMarked { get; set; }
}

public sealed class RecentMarkViewModel
{
    public DateOnly Date { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
}

// ════════════════════════════════════════════════════════
//  Attendance Mark ViewModels
// ════════════════════════════════════════════════════════

public sealed class MarkAttendanceViewModel
{
    [Required] public int FacultyId { get; set; }
    [Required] public int SemesterId { get; set; }
    [Required] public int ClassId { get; set; }
    [Required] public string Subject { get; set; } = string.Empty;
    [Required] public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    // Populated for form rendering
    public List<AttendanceEntryRow> Students { get; set; } = new();
    public List<SemesterSelectItem> Semesters { get; set; } = new();
    public List<ClassSelectItem> Classes { get; set; } = new();
    public List<string> Subjects { get; set; } = new();
    public string? Message { get; set; }
    public bool Success { get; set; }
}

public sealed class AttendanceEntryRow
{
    public int StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RollNo { get; set; } = string.Empty;
    public string Status { get; set; } = "present";    // present|absent|late
    public bool AlreadyMarked { get; set; }
}

public sealed class SemesterSelectItem
{
    public int SemesterId { get; set; }
    public int SemesterNumber { get; set; }
}

public sealed class ClassSelectItem
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
}

// ════════════════════════════════════════════════════════
//  Reporting ViewModels (Faculty)
// ════════════════════════════════════════════════════════

public sealed class ClassReportViewModel
{
    // Filters
    public int? FilterSemesterId { get; set; }
    public int? FilterClassId { get; set; }
    public string? FilterSubject { get; set; }
    public DateOnly? FilterFromDate { get; set; }
    public DateOnly? FilterToDate { get; set; }

    // Lookup data
    public List<SemesterSelectItem> Semesters { get; set; } = new();
    public List<ClassSelectItem> Classes { get; set; } = new();
    public List<string> Subjects { get; set; } = new();

    // Results
    public List<AttendanceSummaryDto> Summary { get; set; } = new();

    // Thresholds applied
    public decimal BlacklistThreshold { get; set; } = 50m;
    public decimal WarningThreshold { get; set; } = 75m;
}

// ════════════════════════════════════════════════════════
//  Threshold Settings ViewModel
// ════════════════════════════════════════════════════════

public sealed class ThresholdViewModel
{
    public int ThresholdId { get; set; }
    public int? FacultyId { get; set; }
    public int? SemesterId { get; set; }
    public int? ClassId { get; set; }

    [Required]
    [Range(1, 99, ErrorMessage = "Blacklist threshold must be between 1 and 99")]
    [Display(Name = "Blacklist Below (%)")]
    public decimal BlacklistThreshold { get; set; } = 50m;

    [Required]
    [Range(1, 99, ErrorMessage = "Warning threshold must be between 1 and 99")]
    [Display(Name = "Warning Below (%)")]
    public decimal WarningThreshold { get; set; } = 75m;

    public bool IsGlobal { get; set; }

    public List<SemesterSelectItem> Semesters { get; set; } = new();
    public List<ClassSelectItem> Classes { get; set; } = new();
    public List<ThresholdSetting> All { get; set; } = new();
    public string? Message { get; set; }
    public bool Success { get; set; }
}

// Used as strong-typed display item
//public sealed class ThresholdSetting
//{
//    public int      ThresholdId        { get; set; }
//    public string   Scope              { get; set; } = string.Empty;
//    public decimal  BlacklistThreshold { get; set; }
//    public decimal  WarningThreshold   { get; set; }
//    public bool     IsGlobal           { get; set; }
//    public DateTime UpdatedAt          { get; set; }
//}

// ════════════════════════════════════════════════════════
//  Admin ViewModels
// ════════════════════════════════════════════════════════

public sealed class AdminFacultyViewModel
{
    public int FacultyId { get; set; }
    public string Name { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Password { get; set; } = "Faculty@123";
    public List<string> AllSubjects { get; set; } = new();
}

public sealed class AdminStudentViewModel
{
    public int StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string RollNo { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public int ClassId { get; set; }
    public string Password { get; set; } = "Student@123";
    public List<SemesterSelectItem> Semesters { get; set; } = new();
    public List<ClassSelectItem> Classes { get; set; } = new();
}

public sealed class AdminDashboardViewModel
{
    public int TotalStudents { get; set; }
    public int TotalFaculty { get; set; }
    public int TotalSubjects { get; set; }
    public int TotalTimetable { get; set; }
    public List<AttendanceMS.Models.Faculty> RecentFaculty { get; set; } = new();
    public List<AttendanceMS.Models.Student> RecentStudents { get; set; } = new();
}

public sealed class TimetableFormViewModel
{
    public int FacultyId { get; set; }
    public int SemesterId { get; set; }
    public int ClassId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;

    public List<AttendanceMS.Models.Faculty> AllFaculty { get; set; } = new();
    public List<SemesterSelectItem> Semesters { get; set; } = new();
    public List<ClassSelectItem> Classes { get; set; } = new();
    public List<string> Subjects { get; set; } = new();
    public List<AttendanceMS.Repositories.Interfaces.TimetableSlot> ExistingSlots { get; set; } = new();
}

public sealed class BulkUploadResultViewModel
{
    public int Created { get; set; }
    public int Skipped { get; set; }
    public string Message { get; set; } = string.Empty;
}

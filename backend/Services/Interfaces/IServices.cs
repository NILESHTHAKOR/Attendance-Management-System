using AttendanceMS.DTOs;
using AttendanceMS.Models;
using AttendanceMS.Repositories.Interfaces;
using AttendanceMS.ViewModels;

namespace AttendanceMS.Services.Interfaces;

public interface IAuthService
{
    /// <summary>Validates credentials; returns User on success, null on failure.</summary>
    User? Authenticate(string email, string password);
}

public interface IStudentService
{
    StudentDashboardViewModel GetDashboard(int userId);
    StudentReportViewModel GetReport(int userId, string? subject, DateOnly? from, DateOnly? to);
    Student? GetByUserId(int userId);
}

public interface IFacultyService
{
    FacultyDashboardViewModel GetDashboard(int userId);
    Faculty? GetByUserId(int userId);
    List<SemesterSelectItem> GetSemesters();
    List<ClassSelectItem> GetClasses(int? semesterId = null);
    List<string> GetSubjectsByFaculty(int facultyId);
    List<TimetableSlot> GetAllSlots(int facultyId);
}

public interface IAttendanceService
{
    MarkAttendanceViewModel BuildMarkForm(int facultyId, int? semesterId, int? classId, string? subject);
    (bool success, string message) SaveAttendance(int facultyId, MarkAttendanceViewModel form);
    ClassReportViewModel GetClassReport(int facultyId, int? semesterId, int? classId,
                                           string? subject, DateOnly? from, DateOnly? to);
}

public interface IThresholdService
{
    ThresholdViewModel GetForm(int facultyId);
    void SaveThreshold(int facultyId, ThresholdViewModel vm);
    (decimal blacklist, decimal warning) GetEffectiveThreshold(int facultyId, int? semesterId, int? classId);
}

public interface IEmailService
{
    Task SendAttendanceWarningAsync(string toEmail, string studentName, decimal percent, string thresholdType);
}

public interface IReportService
{
    byte[] ExportStudentReportExcel(int studentId, string? subject, DateOnly? from, DateOnly? to);
    byte[] ExportClassReportExcel(int? semesterId, int? classId, string? subject,
                                   DateOnly? from, DateOnly? to,
                                   decimal blacklist, decimal warning);
}

public interface IAdminService
{
    // Faculty
    List<Faculty> GetAllFaculty();
    void CreateFaculty(AdminFacultyViewModel vm);
    void UpdateFaculty(AdminFacultyViewModel vm);
    void DeleteFaculty(int facultyId);

    // Students
    List<Student> GetAllStudents();
    void CreateStudent(AdminStudentViewModel vm);
    (int created, int skipped) BulkImportStudents(System.IO.Stream excelStream, int semesterId, int classId);

    // Subjects
    List<Subject> GetAllSubjects();
    void CreateSubject(string name, int? semesterId, int? classId);
    void DeleteSubject(int subjectId);

    // Timetable
    List<TimetableSlot> GetTimetable(int facultyId);
    void AddTimetableSlot(TimetableFormViewModel vm);
    void DeleteTimetableSlot(int timetableId);

    // Lookup
    List<SemesterSelectItem> GetSemesters();
    List<ClassSelectItem> GetClasses(int? semesterId = null);
}

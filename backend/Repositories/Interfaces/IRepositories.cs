using AttendanceMS.DTOs;
using AttendanceMS.Models;

namespace AttendanceMS.Repositories.Interfaces;

public interface IUserRepository
{
    User? GetByEmail(string email);
    User? GetById(int id);
    List<User> GetAll();
    void Create(User user);
    void Update(User user);
    void Delete(int id);
}

public interface IStudentRepository
{
    Student? GetById(int studentId);
    Student? GetByUserId(int userId);
    List<Student> GetByClass(int classId, int semesterId);
    List<Student> GetAll();
    void UpdateStatus(int studentId, string status, decimal percent);
}

public interface IAttendanceRepository
{
    bool HasBeenMarked(int studentId, int classId, string subject, DateOnly date);
    void MarkBulk(IEnumerable<MarkAttendanceDto> records);
    List<AttendanceRecordDto> GetForStudent(int studentId, string? subject, DateOnly? from, DateOnly? to);
    List<AttendanceSummaryDto> GetClassSummary(int? semesterId, int? classId, string? subject, DateOnly? from, DateOnly? to);
    List<string> GetDistinctSubjectsByFaculty(int facultyId);
    List<string> GetDistinctSubjectsByStudent(int studentId);
    bool AnyMarkedForClassSubjectDate(int classId, string subject, DateOnly date);
}

public interface IThresholdRepository
{
    ThresholdSetting? GetGlobal();
    ThresholdSetting? GetForFaculty(int facultyId, int? semesterId, int? classId);
    void Upsert(ThresholdDto dto);
    List<ThresholdSetting> GetAllByFaculty(int facultyId);
}

public interface IReportRepository
{
    List<AttendanceSummaryDto> GetStudentSubjectSummary(int studentId, string? subject, DateOnly? from, DateOnly? to);
    List<AttendanceRecordDto> GetStudentDetail(int studentId, string? subject, DateOnly? from, DateOnly? to);
}

public interface IFacultyRepository
{
    Faculty? GetById(int facultyId);
    Faculty? GetByUserId(int userId);
    List<TimetableSlot> GetTodaySlots(int facultyId);
    List<TimetableSlot> GetAllSlots(int facultyId);
}

// Mini DTO used only by Faculty repo (to avoid a circular reference)
public sealed record TimetableSlot(
    int TimetableId,
    string Subject,
    int ClassId,
    string ClassName,
    int SemesterId,
    int SemesterNumber,
    string StartTime,
    string EndTime,
    string DayOfWeek
);

public interface ISubjectRepository
{
    List<Subject> GetAll();
    List<string> GetNames(int? semesterId = null, int? classId = null);
    void Create(Subject subject);
    void Update(Subject subject);
    void Delete(int subjectId);
}

public interface ITimetableRepository
{
    List<TimetableSlot> GetByFaculty(int facultyId);
    List<TimetableSlot> GetTodayByFaculty(int facultyId);
    void Create(TimetableEntry entry);
    void Delete(int timetableId);
}

public sealed record TimetableEntry(
    int FacultyId,
    int SemesterId,
    int ClassId,
    string Subject,
    string DayOfWeek,
    string StartTime,
    string EndTime
);

public interface IAdminRepository
{
    List<Faculty> GetAllFaculty();
    void CreateFaculty(Faculty faculty, string password);
    void UpdateFaculty(Faculty faculty);
    void DeleteFaculty(int facultyId);
    void CreateStudent(Student student, string password);
    void BulkCreateStudents(List<(Student student, string password)> students);
}

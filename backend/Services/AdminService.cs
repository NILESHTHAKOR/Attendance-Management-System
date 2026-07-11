using AttendanceMS.Models;
using AttendanceMS.Repositories.Interfaces;
using AttendanceMS.Services.Interfaces;
using AttendanceMS.ViewModels;
using ClosedXML.Excel;

namespace AttendanceMS.Services;

public sealed class AdminService : IAdminService
{
    private readonly IAdminRepository      _admin;
    private readonly IStudentRepository    _students;
    private readonly ISubjectRepository    _subjects;
    private readonly ITimetableRepository  _timetable;
    private readonly IFacultyRepository    _faculty;

    // Lookup helpers (reuse Faculty service logic)
    private readonly IFacultyService _facultySvc;

    public AdminService(
        IAdminRepository     admin,
        IStudentRepository   students,
        ISubjectRepository   subjects,
        ITimetableRepository timetable,
        IFacultyRepository   faculty,
        IFacultyService      facultySvc)
    {
        _admin      = admin;
        _students   = students;
        _subjects   = subjects;
        _timetable  = timetable;
        _faculty    = faculty;
        _facultySvc = facultySvc;
    }

    // ── Faculty ──────────────────────────────────────────────────────
    public List<Faculty> GetAllFaculty() => _admin.GetAllFaculty();

    public void CreateFaculty(AdminFacultyViewModel vm)
    {
        var f = new Faculty { Name = vm.Name, Email = vm.Email, Subject = vm.Subject };
        _admin.CreateFaculty(f, vm.Password);
    }

    public void UpdateFaculty(AdminFacultyViewModel vm)
    {
        var existing = _faculty.GetById(vm.FacultyId);
        if (existing is null) return;
        existing.Name    = vm.Name;
        existing.Email   = vm.Email;
        existing.Subject = vm.Subject;
        _admin.UpdateFaculty(existing);
    }

    public void DeleteFaculty(int facultyId) => _admin.DeleteFaculty(facultyId);

    // ── Students ─────────────────────────────────────────────────────
    public List<Student> GetAllStudents() => _students.GetAll();

    public void CreateStudent(AdminStudentViewModel vm)
    {
        var s = new Student
        {
            Name       = vm.Name,
            Email      = vm.Email,
            Phone      = vm.Phone,
            RollNo     = vm.RollNo,
            SemesterId = vm.SemesterId,
            ClassId    = vm.ClassId
        };
        _admin.CreateStudent(s, vm.Password);
    }

    public (int created, int skipped) BulkImportStudents(Stream excelStream, int semesterId, int classId)
    {
        using var wb = new XLWorkbook(excelStream);
        var ws = wb.Worksheets.First();

        var students = new List<(Student, string)>();
        // Expected columns: Name, Email, Phone, RollNo, Password(optional)
        foreach (var row in ws.RowsUsed().Skip(1)) // skip header
        {
            string name   = row.Cell(1).GetValue<string>().Trim();
            string email  = row.Cell(2).GetValue<string>().Trim();
            string phone  = row.Cell(3).GetValue<string>().Trim();
            string rollNo = row.Cell(4).GetValue<string>().Trim();
            string pwd    = ws.Column(5).CellsUsed().Count() > 0
                ? row.Cell(5).GetValue<string>().Trim() : "";
            if (string.IsNullOrEmpty(pwd)) pwd = "Student@123";

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email)) continue;

            students.Add((new Student
            {
                Name       = name,
                Email      = email,
                Phone      = phone,
                RollNo     = rollNo,
                SemesterId = semesterId,
                ClassId    = classId
            }, pwd));
        }

        int before = _students.GetAll().Count;
        _admin.BulkCreateStudents(students);
        int after = _students.GetAll().Count;

        int created = after - before;
        int skipped = students.Count - created;
        return (created, skipped);
    }

    // ── Subjects ─────────────────────────────────────────────────────
    public List<Subject> GetAllSubjects() => _subjects.GetAll();
    public void CreateSubject(string name, int? semesterId, int? classId)
        => _subjects.Create(new Subject { SubjectName = name, SemesterId = semesterId, ClassId = classId });
    public void DeleteSubject(int subjectId) => _subjects.Delete(subjectId);

    // ── Timetable ────────────────────────────────────────────────────
    public List<TimetableSlot> GetTimetable(int facultyId)
        => _timetable.GetByFaculty(facultyId);

    public void AddTimetableSlot(TimetableFormViewModel vm)
        => _timetable.Create(new TimetableEntry(
            vm.FacultyId, vm.SemesterId, vm.ClassId,
            vm.Subject, vm.DayOfWeek, vm.StartTime, vm.EndTime));

    public void DeleteTimetableSlot(int timetableId)
        => _timetable.Delete(timetableId);

    // ── Lookups ──────────────────────────────────────────────────────
    public List<SemesterSelectItem> GetSemesters() => _facultySvc.GetSemesters();
    public List<ClassSelectItem>    GetClasses(int? semesterId = null)
        => _facultySvc.GetClasses(semesterId);
}

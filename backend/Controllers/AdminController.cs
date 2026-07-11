using AttendanceMS.Services.Interfaces;
using AttendanceMS.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceMS.Controllers;

public sealed class AdminController : BaseController
{
    private readonly IAdminService   _admin;
    private readonly IFacultyService _faculty;

    public AdminController(IAdminService admin, IFacultyService faculty)
    {
        _admin   = admin;
        _faculty = faculty;
    }

    // ── Dashboard ────────────────────────────────────────────────────
    public IActionResult Index()
    {
        var guard = RequireRole("admin");
        if (guard is not null) return guard;

        var allFaculty  = _admin.GetAllFaculty();
        var allStudents = _admin.GetAllStudents();
        var vm = new AdminDashboardViewModel
        {
            TotalStudents  = allStudents.Count,
            TotalFaculty   = allFaculty.Count,
            TotalSubjects  = _admin.GetAllSubjects().Count,
            TotalTimetable = 0,
            RecentFaculty  = allFaculty.Take(5).ToList(),
            RecentStudents = allStudents.Take(5).ToList()
        };
        ViewData["Title"]  = "Admin Dashboard";
        ViewData["Active"] = "admin-dashboard";
        return View(vm);
    }

    // ════════════════════════════════════════════════════════
    //  FACULTY MANAGEMENT
    // ════════════════════════════════════════════════════════

    public IActionResult Faculty()
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;
        ViewData["Title"]  = "Manage Faculty";
        ViewData["Active"] = "admin-faculty";
        var list = _admin.GetAllFaculty();
        return View(list);
    }

    public IActionResult CreateFaculty()
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;
        ViewData["Title"] = "Add Faculty";
        var vm = new AdminFacultyViewModel
        {
            AllSubjects = _admin.GetAllSubjects().Select(s => s.SubjectName).ToList()
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CreateFaculty(AdminFacultyViewModel vm)
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;
        if (!ModelState.IsValid)
        {
            vm.AllSubjects = _admin.GetAllSubjects().Select(s => s.SubjectName).ToList();
            return View(vm);
        }
        _admin.CreateFaculty(vm);
        TempData["Success"] = $"Faculty '{vm.Name}' created successfully.";
        return RedirectToAction("Faculty");
    }

    public IActionResult EditFaculty(int id)
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;
        var fac = _admin.GetAllFaculty().FirstOrDefault(f => f.FacultyId == id);
        if (fac is null) return NotFound();
        var vm = new AdminFacultyViewModel
        {
            FacultyId   = fac.FacultyId,
            Name        = fac.Name,
            Email       = fac.Email,
            Subject     = fac.Subject,
            AllSubjects = _admin.GetAllSubjects().Select(s => s.SubjectName).ToList()
        };
        ViewData["Title"] = "Edit Faculty";
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult EditFaculty(AdminFacultyViewModel vm)
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;
        if (!ModelState.IsValid)
        {
            vm.AllSubjects = _admin.GetAllSubjects().Select(s => s.SubjectName).ToList();
            return View(vm);
        }
        _admin.UpdateFaculty(vm);
        TempData["Success"] = "Faculty updated.";
        return RedirectToAction("Faculty");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteFaculty(int id)
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;
        _admin.DeleteFaculty(id);
        TempData["Success"] = "Faculty removed.";
        return RedirectToAction("Faculty");
    }

    // ════════════════════════════════════════════════════════
    //  STUDENT MANAGEMENT
    // ════════════════════════════════════════════════════════

    public IActionResult Students()
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;
        ViewData["Title"]  = "Manage Students";
        ViewData["Active"] = "admin-students";
        return View(_admin.GetAllStudents());
    }

    public IActionResult CreateStudent()
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;
        var vm = new AdminStudentViewModel
        {
            Semesters = _admin.GetSemesters(),
            Classes   = _admin.GetClasses()
        };
        ViewData["Title"] = "Add Student";
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CreateStudent(AdminStudentViewModel vm)
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;
        if (!ModelState.IsValid)
        {
            vm.Semesters = _admin.GetSemesters();
            vm.Classes   = _admin.GetClasses();
            return View(vm);
        }
        _admin.CreateStudent(vm);
        TempData["Success"] = $"Student '{vm.Name}' created.";
        return RedirectToAction("Students");
    }

    // Bulk Upload page
    public IActionResult BulkUpload()
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;
        ViewData["Title"] = "Bulk Upload Students";
        ViewData["Active"] = "admin-students";
        var vm = new AdminStudentViewModel
        {
            Semesters = _admin.GetSemesters(),
            Classes   = _admin.GetClasses()
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult BulkUpload(IFormFile file, int semesterId, int classId)
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;

        if (file is null || file.Length == 0)
        {
            TempData["Error"] = "Please select a valid Excel file.";
            return RedirectToAction("BulkUpload");
        }

        try
        {
            using var stream = file.OpenReadStream();
            var (created, skipped) = _admin.BulkImportStudents(stream, semesterId, classId);
            TempData["Success"] = $"Import complete: {created} students created, {skipped} skipped (duplicate email).";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Import failed: {ex.Message}";
        }

        return RedirectToAction("Students");
    }

    // Download template
    public IActionResult DownloadTemplate()
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;
        using var wb = new ClosedXML.Excel.XLWorkbook();
        var ws = wb.AddWorksheet("Students");
        ws.Cell(1,1).Value = "Name";
        ws.Cell(1,2).Value = "Email";
        ws.Cell(1,3).Value = "Phone";
        ws.Cell(1,4).Value = "RollNo";
        ws.Cell(1,5).Value = "Password (optional)";
        // Style header
        var header = ws.Range(1,1,1,5);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
        // Sample row
        ws.Cell(2,1).Value = "John Doe";
        ws.Cell(2,2).Value = "john@example.com";
        ws.Cell(2,3).Value = "9876543210";
        ws.Cell(2,4).Value = "BCA001";
        ws.Cell(2,5).Value = "Student@123";
        ws.Columns().AdjustToContents();

        using var ms = new System.IO.MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "StudentUploadTemplate.xlsx");
    }

    // ════════════════════════════════════════════════════════
    //  SUBJECT MANAGEMENT
    // ════════════════════════════════════════════════════════

    public IActionResult Subjects()
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;
        ViewData["Title"]  = "Manage Subjects";
        ViewData["Active"] = "admin-subjects";
        return View(_admin.GetAllSubjects());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CreateSubject(string subjectName, int? semesterId, int? classId)
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;
        if (!string.IsNullOrWhiteSpace(subjectName))
        {
            _admin.CreateSubject(subjectName.Trim(), semesterId, classId);
            TempData["Success"] = $"Subject '{subjectName}' added.";
        }
        return RedirectToAction("Subjects");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteSubject(int id)
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;
        _admin.DeleteSubject(id);
        TempData["Success"] = "Subject removed.";
        return RedirectToAction("Subjects");
    }

    // ════════════════════════════════════════════════════════
    //  TIMETABLE MANAGEMENT
    // ════════════════════════════════════════════════════════

    public IActionResult Timetable(int? facultyId)
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;
        ViewData["Title"]  = "Manage Timetable";
        ViewData["Active"] = "admin-timetable";

        int fid = facultyId ?? (_admin.GetAllFaculty().FirstOrDefault()?.FacultyId ?? 0);
        var vm = new TimetableFormViewModel
        {
            FacultyId     = fid,
            AllFaculty    = _admin.GetAllFaculty(),
            Semesters     = _admin.GetSemesters(),
            Classes       = _admin.GetClasses(),
            Subjects      = _admin.GetAllSubjects().Select(s => s.SubjectName).ToList(),
            ExistingSlots = fid > 0 ? _admin.GetTimetable(fid) : new()
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult AddTimetableSlot(TimetableFormViewModel vm)
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;
        _admin.AddTimetableSlot(vm);
        TempData["Success"] = "Timetable slot added.";
        return RedirectToAction("Timetable", new { facultyId = vm.FacultyId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteTimetableSlot(int id, int facultyId)
    {
        var guard = RequireRole("admin"); if (guard is not null) return guard;
        _admin.DeleteTimetableSlot(id);
        TempData["Success"] = "Slot removed.";
        return RedirectToAction("Timetable", new { facultyId });
    }

    // AJAX: get classes by semester
    [HttpGet]
    public IActionResult GetClasses(int semesterId)
    {
        var guard = RequireRole("admin"); if (guard is not null) return Unauthorized();
        var classes = _admin.GetClasses(semesterId);
        return Json(classes.Select(c => new { c.ClassId, c.ClassName }));
    }
}

using AttendanceMS.Services.Interfaces;
using AttendanceMS.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceMS.Controllers;

public sealed class FacultyController : BaseController
{
    private readonly IFacultyService _faculty;
    private readonly IAttendanceService _attendance;
    private readonly IThresholdService _threshold;
    private readonly IReportService _report;

    public FacultyController(
        IFacultyService faculty,
        IAttendanceService attendance,
        IThresholdService threshold,
        IReportService report)
    {
        _faculty = faculty;
        _attendance = attendance;
        _threshold = threshold;
        _report = report;
    }

    // ── Dashboard ────────────────────────────────────────────────────

    // GET /Faculty
    public IActionResult Index()
    {
        var guard = RequireRole("faculty", "admin");
        if (guard is not null) return guard;

        // Admin has no Faculty record — send them to Admin dashboard
        if (SessionUserRole == "admin")
            return RedirectToAction("Index", "Admin");

        var vm = _faculty.GetDashboard(SessionUserId!.Value);
        return View(vm);
    }

    // ── Mark Attendance ──────────────────────────────────────────────

    // GET /Faculty/MarkAttendance
    public IActionResult MarkAttendance(int? semesterId, int? classId, string? subject)
    {
        var guard = RequireRole("faculty", "admin");
        if (guard is not null) return guard;

        var fac = _faculty.GetByUserId(SessionUserId!.Value);
        if (fac is null) return RedirectToAction("Index", "Admin");

        var vm = _attendance.BuildMarkForm(fac.FacultyId, semesterId, classId, subject);
        return View(vm);
    }

    // POST /Faculty/MarkAttendance
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MarkAttendance(MarkAttendanceViewModel model)
    {
        var guard = RequireRole("faculty", "admin");
        if (guard is not null) return guard;

        var fac = _faculty.GetByUserId(SessionUserId!.Value);
        if (fac is null) return RedirectToAction("Index", "Admin");

        // Re-read student rows from hidden form fields (they come as arrays)
        // The model binder handles the Students list automatically via name="Students[i].Status"

        var (success, message) = _attendance.SaveAttendance(fac.FacultyId, model);

        if (success)
            TempData["Success"] = message;
        else
            TempData["Error"] = message;

        return RedirectToAction("MarkAttendance", new
        {
            semesterId = model.SemesterId,
            classId = model.ClassId,
            subject = model.Subject
        });
    }

    // AJAX: GET /Faculty/GetClasses?semesterId=1
    [HttpGet]
    public IActionResult GetClasses(int semesterId)
    {
        var guard = RequireRole("faculty", "admin");
        if (guard is not null) return Unauthorized();

        var classes = _faculty.GetClasses(semesterId);
        return Json(classes.Select(c => new { c.ClassId, c.ClassName }));
    }

    // AJAX: GET /Faculty/GetSubjects
    [HttpGet]
    public IActionResult GetSubjects()
    {
        var guard = RequireRole("faculty", "admin");
        if (guard is not null) return Unauthorized();

        var fac = _faculty.GetByUserId(SessionUserId!.Value);
        if (fac is null) return Json(new List<string>());

        var subjects = _faculty.GetSubjectsByFaculty(fac.FacultyId);
        return Json(subjects);
    }

    // ── Timetable ────────────────────────────────────────────────────

    // GET /Faculty/Timetable
    public IActionResult Timetable()
    {
        var guard = RequireRole("faculty", "admin");
        if (guard is not null) return guard;

        var fac = _faculty.GetByUserId(SessionUserId!.Value);
        if (fac is null) return RedirectToAction("Timetable", "Admin");

        var slots = _faculty.GetAllSlots(fac.FacultyId);
        var vm = new TimetableFormViewModel
        {
            FacultyId = fac.FacultyId,
            ExistingSlots = slots,
            Semesters = _faculty.GetSemesters(),
            Classes = _faculty.GetClasses()
        };
        ViewData["Title"] = "My Timetable";
        ViewData["Active"] = "timetable";
        return View(vm);
    }

    // ── Class Report ─────────────────────────────────────────────────

    // GET /Faculty/ClassReport
    public IActionResult ClassReport(int? semesterId, int? classId, string? subject,
                                      string? fromDate, string? toDate)
    {
        var guard = RequireRole("faculty", "admin");
        if (guard is not null) return guard;

        var fac = _faculty.GetByUserId(SessionUserId!.Value);
        if (fac is null) return RedirectToAction("Index", "Admin");

        DateOnly? from = DateOnly.TryParse(fromDate, out var f) ? f : null;
        DateOnly? to = DateOnly.TryParse(toDate, out var t) ? t : null;

        var vm = _attendance.GetClassReport(fac.FacultyId, semesterId, classId, subject, from, to);
        return View(vm);
    }

    // GET /Faculty/ExportClassExcel
    public IActionResult ExportClassExcel(int? semesterId, int? classId, string? subject,
                                           string? fromDate, string? toDate)
    {
        var guard = RequireRole("faculty", "admin");
        if (guard is not null) return guard;

        var fac2 = _faculty.GetByUserId(SessionUserId!.Value);
        if (fac2 is null) return RedirectToAction("Index", "Admin");

        DateOnly? from = DateOnly.TryParse(fromDate, out var f) ? f : null;
        DateOnly? to = DateOnly.TryParse(toDate, out var t) ? t : null;

        var (bl, warn) = _threshold.GetEffectiveThreshold(fac2.FacultyId, semesterId, classId);

        var bytes = _report.ExportClassReportExcel(semesterId, classId, subject, from, to, bl, warn);
        string fname = $"ClassReport_{DateTime.Today:yyyyMMdd}.xlsx";
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fname);
    }

    // ── Threshold Settings ───────────────────────────────────────────

    // GET /Faculty/Threshold
    public IActionResult Threshold()
    {
        var guard = RequireRole("faculty", "admin");
        if (guard is not null) return guard;

        var fac = _faculty.GetByUserId(SessionUserId!.Value);
        if (fac is null) return RedirectToAction("Index", "Admin");

        var vm = _threshold.GetForm(fac.FacultyId);
        return View(vm);
    }

    // POST /Faculty/Threshold
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Threshold(ThresholdViewModel model)
    {
        var guard = RequireRole("faculty", "admin");
        if (guard is not null) return guard;

        var fac = _faculty.GetByUserId(SessionUserId!.Value);
        if (fac is null) return RedirectToAction("Index", "Admin");

        if (!ModelState.IsValid)
        {
            model.All = _threshold.GetForm(fac.FacultyId).All;
            model.Semesters = _faculty.GetSemesters();
            model.Classes = _faculty.GetClasses();
            return View(model);
        }

        try
        {
            _threshold.SaveThreshold(fac.FacultyId, model);
            TempData["Success"] = "Threshold updated successfully.";
        }
        catch (ArgumentException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Threshold");
    }
}
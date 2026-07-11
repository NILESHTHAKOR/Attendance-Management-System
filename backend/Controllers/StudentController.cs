using AttendanceMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceMS.Controllers;

public sealed class StudentController : BaseController
{
    private readonly IStudentService _student;
    private readonly IReportService  _report;

    public StudentController(IStudentService student, IReportService report)
    {
        _student = student;
        _report  = report;
    }

    // GET /Student  — Dashboard
    public IActionResult Index()
    {
        var guard = RequireRole("student");
        if (guard is not null) return guard;

        var vm = _student.GetDashboard(SessionUserId!.Value);
        return View(vm);
    }

    // GET /Student/Report
    public IActionResult Report(string? subject, string? fromDate, string? toDate)
    {
        var guard = RequireRole("student");
        if (guard is not null) return guard;

        DateOnly? from = DateOnly.TryParse(fromDate, out var f) ? f : null;
        DateOnly? to   = DateOnly.TryParse(toDate,   out var t) ? t : null;

        var vm = _student.GetReport(SessionUserId!.Value, subject, from, to);
        return View(vm);
    }

    // GET /Student/ExportExcel
    public IActionResult ExportExcel(string? subject, string? fromDate, string? toDate)
    {
        var guard = RequireRole("student");
        if (guard is not null) return guard;

        DateOnly? from = DateOnly.TryParse(fromDate, out var f) ? f : null;
        DateOnly? to   = DateOnly.TryParse(toDate,   out var t) ? t : null;

        var student = _student.GetByUserId(SessionUserId!.Value);
        if (student is null) return NotFound();

        var bytes    = _report.ExportStudentReportExcel(student.StudentId, subject, from, to);
        string fname = $"Attendance_{student.RollNo}_{DateTime.Today:yyyyMMdd}.xlsx";
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fname);
    }
}

using AttendanceMS.DTOs;
using AttendanceMS.Repositories.Interfaces;
using AttendanceMS.Services.Interfaces;
using ClosedXML.Excel;

namespace AttendanceMS.Services;

public sealed class ReportService : IReportService
{
    private readonly IReportRepository     _reports;
    private readonly IAttendanceRepository _attendance;

    public ReportService(IReportRepository reports, IAttendanceRepository attendance)
    {
        _reports    = reports;
        _attendance = attendance;
    }

    // ─── Student Excel Export ──────────────────────────────────────────────
    public byte[] ExportStudentReportExcel(int studentId, string? subject, DateOnly? from, DateOnly? to)
    {
        var summaries = _reports.GetStudentSubjectSummary(studentId, subject, from, to);
        var details   = _reports.GetStudentDetail(studentId, subject, from, to);

        using var wb  = new XLWorkbook();

        // Sheet 1 – Summary
        var wsSummary = wb.Worksheets.Add("Summary");
        WriteStudentSummarySheet(wsSummary, summaries);

        // Sheet 2 – Detailed Log
        var wsDetail = wb.Worksheets.Add("Detailed Log");
        WriteStudentDetailSheet(wsDetail, details);

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ─── Class Excel Export ────────────────────────────────────────────────
    public byte[] ExportClassReportExcel(int? semesterId, int? classId, string? subject,
                                          DateOnly? from, DateOnly? to,
                                          decimal blacklist, decimal warning)
    {
        var data = _attendance.GetClassSummary(semesterId, classId, subject, from, to);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Attendance Report");
        WriteClassReportSheet(ws, data, blacklist, warning);

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ══════════════════════════════════════════════════════════════
    //  Private sheet builders
    // ══════════════════════════════════════════════════════════════

    private static void WriteStudentSummarySheet(IXLWorksheet ws, List<AttendanceSummaryDto> summaries)
    {
        // Title
        ws.Cell(1, 1).Value = "Attendance Summary Report";
        ws.Range(1, 1, 1, 7).Merge().Style
            .Font.SetBold(true).Font.SetFontSize(14)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#1e3a5f"))
            .Font.SetFontColor(XLColor.White);

        ws.Cell(2, 1).Value = $"Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}";
        ws.Range(2, 1, 2, 7).Merge().Style.Font.SetItalic(true).Font.SetFontColor(XLColor.Gray);

        // Headers
        string[] headers = { "Subject", "Total Classes", "Present", "Absent", "Late", "Attendance %", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style
                .Font.SetBold(true).Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#2563eb"))
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }

        int row = 5;
        foreach (var s in summaries)
        {
            ws.Cell(row, 1).Value = s.Subject;
            ws.Cell(row, 2).Value = s.TotalClasses;
            ws.Cell(row, 3).Value = s.Present;
            ws.Cell(row, 4).Value = s.Absent;
            ws.Cell(row, 5).Value = s.Late;
            ws.Cell(row, 6).Value = (double)s.AttendancePercent;
            ws.Cell(row, 6).Style.NumberFormat.Format = "0.00\"%\"";
            ws.Cell(row, 7).Value = s.Status.ToUpperInvariant();

            // Status color
            var statusCell = ws.Cell(row, 7);
            statusCell.Style.Fill.SetBackgroundColor(s.Status switch
            {
                "active"      => XLColor.FromHtml("#d1fae5"),
                "warning"     => XLColor.FromHtml("#fef3c7"),
                "blacklisted" => XLColor.FromHtml("#fee2e2"),
                _             => XLColor.White
            });

            ws.Range(row, 1, row, 7).Style
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin);
            row++;
        }

        ws.Columns().AdjustToContents();
    }

    private static void WriteStudentDetailSheet(IXLWorksheet ws, List<AttendanceRecordDto> details)
    {
        ws.Cell(1, 1).Value = "Detailed Attendance Log";
        ws.Range(1, 1, 1, 6).Merge().Style
            .Font.SetBold(true).Font.SetFontSize(13)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#1e3a5f"))
            .Font.SetFontColor(XLColor.White)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        string[] headers = { "Date", "Subject", "Class", "Semester", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(3, i + 1).Value = headers[i];
            ws.Cell(3, i + 1).Style.Font.SetBold(true)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#2563eb"))
                .Font.SetFontColor(XLColor.White);
        }

        int row = 4;
        foreach (var d in details)
        {
            ws.Cell(row, 1).Value = d.Date.ToString("dd-MMM-yyyy");
            ws.Cell(row, 2).Value = d.Subject;
            ws.Cell(row, 3).Value = d.ClassName;
            ws.Cell(row, 4).Value = $"Sem {d.SemesterNumber}";
            ws.Cell(row, 5).Value = d.Status.ToUpperInvariant();

            ws.Cell(row, 5).Style.Fill.SetBackgroundColor(d.Status switch
            {
                "present" => XLColor.FromHtml("#d1fae5"),
                "absent"  => XLColor.FromHtml("#fee2e2"),
                "late"    => XLColor.FromHtml("#fef3c7"),
                _         => XLColor.White
            });

            ws.Range(row, 1, row, 5).Style
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin);
            row++;
        }

        ws.Columns().AdjustToContents();
    }

    private static void WriteClassReportSheet(
        IXLWorksheet ws, List<AttendanceSummaryDto> data, decimal blacklist, decimal warning)
    {
        ws.Cell(1, 1).Value = "Class Attendance Report";
        ws.Range(1, 1, 1, 8).Merge().Style
            .Font.SetBold(true).Font.SetFontSize(14)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#1e3a5f"))
            .Font.SetFontColor(XLColor.White)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell(2, 1).Value = $"Thresholds: Blacklist <{blacklist}%  |  Warning <{warning}%  |  Generated: {DateTime.Now:dd-MMM-yyyy}";
        ws.Range(2, 1, 2, 8).Merge().Style.Font.SetItalic(true).Font.SetFontColor(XLColor.Gray);

        string[] headers = { "#", "Roll No", "Student Name", "Subject", "Total", "Present", "Absent", "Late", "Attendance %", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(4, i + 1).Value = headers[i];
            ws.Cell(4, i + 1).Style.Font.SetBold(true)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#2563eb"))
                .Font.SetFontColor(XLColor.White)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }

        int row = 5, serial = 1;
        foreach (var s in data)
        {
            ws.Cell(row, 1).Value  = serial++;
            ws.Cell(row, 2).Value  = s.RollNo;
            ws.Cell(row, 3).Value  = s.StudentName;
            ws.Cell(row, 4).Value  = s.Subject;
            ws.Cell(row, 5).Value  = s.TotalClasses;
            ws.Cell(row, 6).Value  = s.Present;
            ws.Cell(row, 7).Value  = s.Absent;
            ws.Cell(row, 8).Value  = s.Late;
            ws.Cell(row, 9).Value  = (double)s.AttendancePercent;
            ws.Cell(row, 9).Style.NumberFormat.Format = "0.00\"%\"";
            ws.Cell(row, 10).Value = s.Status.ToUpperInvariant();

            // Row color based on status
            var rowColor = s.AttendancePercent < blacklist
                ? XLColor.FromHtml("#fee2e2")
                : s.AttendancePercent < warning
                ? XLColor.FromHtml("#fef3c7")
                : XLColor.FromHtml("#f0fdf4");

            ws.Range(row, 1, row, 10).Style
                .Fill.SetBackgroundColor(rowColor)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin);

            row++;
        }

        ws.Columns().AdjustToContents();
    }
}

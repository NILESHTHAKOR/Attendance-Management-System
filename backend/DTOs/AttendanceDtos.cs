namespace AttendanceMS.DTOs;

/// <summary>
/// Lightweight objects used to move data between Repository ↔ Service ↔ Controller.
/// No UI concerns, no annotations — pure data.
/// </summary>

public sealed class AttendanceRecordDto
{
    public int      AttendanceId    { get; set; }
    public int      StudentId       { get; set; }
    public string   StudentName     { get; set; } = string.Empty;
    public string   RollNo          { get; set; } = string.Empty;
    public string   Subject         { get; set; } = string.Empty;
    public DateOnly Date            { get; set; }
    public string   Status          { get; set; } = string.Empty;   // present|absent|late
    public string   ClassName       { get; set; } = string.Empty;
    public int      SemesterNumber  { get; set; }
}

public sealed class AttendanceSummaryDto
{
    public int      StudentId         { get; set; }
    public string   StudentName       { get; set; } = string.Empty;
    public string   RollNo            { get; set; } = string.Empty;
    public string   Subject           { get; set; } = string.Empty;
    public int      TotalClasses      { get; set; }
    public int      Present           { get; set; }
    public int      Absent            { get; set; }
    public int      Late              { get; set; }
    public decimal  AttendancePercent { get; set; }
    public string   Status            { get; set; } = string.Empty;
    public string   StudentEmail      { get; set; } = string.Empty;
}

public sealed class MarkAttendanceDto
{
    public int    StudentId  { get; set; }
    public int    ClassId    { get; set; }
    public int    SemesterId { get; set; }
    public string Subject    { get; set; } = string.Empty;
    public DateOnly Date     { get; set; }
    public string Status     { get; set; } = "present";
}

public sealed class StudentStatusUpdateDto
{
    public int     StudentId    { get; set; }
    public string  OldStatus   { get; set; } = string.Empty;
    public string  NewStatus   { get; set; } = string.Empty;
    public decimal NewPercent  { get; set; }
    public string  StudentName { get; set; } = string.Empty;
    public string  Email       { get; set; } = string.Empty;
}

public sealed class ThresholdDto
{
    public int?    FacultyId          { get; set; }
    public int?    SemesterId         { get; set; }
    public int?    ClassId            { get; set; }
    public decimal BlacklistThreshold { get; set; }
    public decimal WarningThreshold   { get; set; }
    public bool    IsGlobal           { get; set; }
}

public sealed class ReportFilterDto
{
    public int?    FacultyId  { get; set; }
    public int?    StudentId  { get; set; }
    public int?    SemesterId { get; set; }
    public int?    ClassId    { get; set; }
    public string? Subject    { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate   { get; set; }
}

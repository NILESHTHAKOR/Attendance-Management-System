using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AttendanceMS.Controllers;

/// <summary>
/// All controllers inherit this. Provides session-based auth helpers.
/// </summary>
public abstract class BaseController : Controller
{
    // ── Session key constants ────────────────────────────────────────
    protected const string SK_USER_ID   = "UserId";
    protected const string SK_USER_NAME = "UserName";
    protected const string SK_USER_ROLE = "UserRole";
    protected const string SK_USER_EMAIL= "UserEmail";

    // ── Session accessors ────────────────────────────────────────────
    protected int?   SessionUserId   => HttpContext.Session.GetInt32(SK_USER_ID);
    protected string SessionUserName => HttpContext.Session.GetString(SK_USER_NAME) ?? string.Empty;
    protected string SessionUserRole => HttpContext.Session.GetString(SK_USER_ROLE) ?? string.Empty;
    protected string SessionUserEmail=> HttpContext.Session.GetString(SK_USER_EMAIL)?? string.Empty;

    protected bool IsLoggedIn     => SessionUserId.HasValue;
    protected bool IsStudent      => SessionUserRole == "student";
    protected bool IsFaculty      => SessionUserRole == "faculty";
    protected bool IsAdmin        => SessionUserRole == "admin";
    protected bool IsFacultyOrAdmin => IsFaculty || IsAdmin;

    // ── Set session after login ──────────────────────────────────────
    protected void SetSession(int userId, string name, string role, string email)
    {
        HttpContext.Session.SetInt32(SK_USER_ID,    userId);
        HttpContext.Session.SetString(SK_USER_NAME,  name);
        HttpContext.Session.SetString(SK_USER_ROLE,  role);
        HttpContext.Session.SetString(SK_USER_EMAIL, email);
    }

    protected void ClearSession() => HttpContext.Session.Clear();

    // ── Guard helpers ────────────────────────────────────────────────
    protected IActionResult? RequireLogin()
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Auth");
        return null;
    }

    protected IActionResult? RequireRole(params string[] roles)
    {
        var redirect = RequireLogin();
        if (redirect is not null) return redirect;

        if (!roles.Contains(SessionUserRole, StringComparer.OrdinalIgnoreCase))
            return RedirectToAction("AccessDenied", "Home");

        return null;
    }

    // ── Pass common ViewBag data to all views ────────────────────────
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.UserName  = SessionUserName;
        ViewBag.UserRole  = SessionUserRole;
        ViewBag.UserEmail = SessionUserEmail;
        ViewBag.UserId    = SessionUserId;
        base.OnActionExecuting(context);
    }
}

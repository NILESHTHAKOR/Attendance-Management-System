using AttendanceMS.Services.Interfaces;
using AttendanceMS.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceMS.Controllers;

public sealed class AuthController : BaseController
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    // GET /Auth/Login
    [HttpGet]
    public IActionResult Login()
    {
        if (IsLoggedIn)
            return RedirectToDashboard();
        return View(new LoginViewModel());
    }

    // POST /Auth/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = _auth.Authenticate(model.Email, model.Password);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        SetSession(user.Id, user.Name, user.Role, user.Email);
        return RedirectToDashboard();
    }
    // POST /Auth/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        ClearSession();
        return RedirectToAction("Login");
    }

    private IActionResult RedirectToDashboard() => SessionUserRole switch
    {
        "student" => RedirectToAction("Index", "Student"),
        "faculty" => RedirectToAction("Index", "Faculty"),
        "admin" => RedirectToAction("Index", "Admin"),
        _ => RedirectToAction("Login")
    };
}
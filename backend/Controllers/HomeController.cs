using Microsoft.AspNetCore.Mvc;

namespace AttendanceMS.Controllers;

public sealed class HomeController : BaseController
{
    public IActionResult AccessDenied() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}

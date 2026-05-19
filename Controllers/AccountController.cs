using Microsoft.AspNetCore.Mvc;
using SentinelPulse.Models;

namespace SentinelPulse.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    public AccountController(AppDbContext db) { _db = db; }

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var officer = _db.Officers.FirstOrDefault(o =>
            o.BadgeNumber == model.Username && o.PasswordHash == model.Password);

        if (officer == null)
        {
            ViewBag.Error = "Invalid badge number or password.";
            return View(model);
        }

        HttpContext.Session.SetString("OfficerName", officer.Name);
        HttpContext.Session.SetString("OfficerBadge", officer.BadgeNumber);
        HttpContext.Session.SetString("OfficerRole", officer.Role);

        return RedirectToAction("Index", "Dashboard");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}

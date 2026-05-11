using Microsoft.AspNetCore.Mvc;
using SentinelPulse.Models;
using System.Linq;

namespace SentinelPulse.Controllers
{
    public class OfficersController : Controller
    {
        private readonly AppDbContext _db;
        public OfficersController(AppDbContext db) { _db = db; }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("OfficerRole") != "Admin")
                return RedirectToAction("Index", "Dashboard");
            var officers = _db.Officers.ToList();
            foreach (var o in officers)
            {
                ViewData[$"cases_{o.Id}"] = _db.Cases.Count(c => c.AssignedOfficer == o.Name && c.Status != "Closed");
            }
            return View(officers);
        }

        public IActionResult Details(int id)
        {
            if (HttpContext.Session.GetString("OfficerRole") != "Admin")
                return RedirectToAction("Index", "Dashboard");
            var officer = _db.Officers.FirstOrDefault(o => o.Id == id);
            if (officer == null) return NotFound();
            ViewBag.ActiveCases = _db.Cases.Where(c => c.AssignedOfficer == officer.Name && c.Status != "Closed").ToList();
            ViewBag.ClosedCases = _db.Cases.Count(c => c.AssignedOfficer == officer.Name && c.Status == "Closed");
            return View(officer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int id, string status)
        {
            if (HttpContext.Session.GetString("OfficerRole") != "Admin") return Forbid();
            var officer = _db.Officers.FirstOrDefault(o => o.Id == id);
            if (officer != null) { officer.Status = status; _db.SaveChanges(); }
            return RedirectToAction("Index");
        }
    }
}

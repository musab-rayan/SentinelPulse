using Microsoft.AspNetCore.Mvc;
using SentinelPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace SentinelPulse.Controllers
{
    public class CasesController : Controller
    {
        private readonly AppDbContext _db;
        public CasesController(AppDbContext db) { _db = db; }

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("OfficerRole");
            var name = HttpContext.Session.GetString("OfficerName");

            var cases = role == "Admin"
                ? _db.Cases.OrderByDescending(c => c.LastUpdated).ToList()
                : _db.Cases.Where(c => c.AssignedOfficer == name)
                           .OrderByDescending(c => c.LastUpdated).ToList();

            return View(cases);
        }

        public IActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");
            var c = _db.Cases.FirstOrDefault(x => x.CaseId == id);
            if (c == null) return NotFound();
            ViewBag.Evidence = _db.Evidence.Where(e => e.CaseId == id).OrderByDescending(e => e.CollectedDate).ToList();
            ViewBag.Suspects = _db.Suspects.Where(s => s.CaseId == id).ToList();
            return View(c);
        }

        [HttpPost]
        public IActionResult UpdateStatus(string id, string status, string? notes)
        {
            var role = HttpContext.Session.GetString("OfficerRole");
            var c = _db.Cases.FirstOrDefault(x => x.CaseId == id);
            if (c == null) return NotFound();

            // Officers can only move to Under Investigation or submit for approval
            // Admins can approve/reject/close
            if (role != "Admin" && (status == "Closed"))
                return Forbid();

            c.Status = status;
            c.LastUpdated = DateTime.Now;
            if (!string.IsNullOrEmpty(notes))
                c.InvestigationNotes = notes;
            if (status == "Closed" && !string.IsNullOrEmpty(notes))
                c.ClosureReason = notes;

            _db.SaveChanges();
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (HttpContext.Session.GetString("OfficerRole") != "Admin") return Forbid();
            var c = _db.Cases.FirstOrDefault(x => x.CaseId == id);
            if (c != null) { _db.Cases.Remove(c); _db.SaveChanges(); }
            return RedirectToAction("Index");
        }
    }
}

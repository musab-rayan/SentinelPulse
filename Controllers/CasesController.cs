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

            ViewBag.Complainants = _db.FIRs.ToDictionary(f => f.CaseId, f => f.CitizenName);

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
            ViewBag.Officers = _db.Officers.Where(o => o.Role == "Officer" && o.Status == "Active").ToList();

            var fir = _db.FIRs.FirstOrDefault(f => f.CaseId == id);
            ViewBag.Complainant = fir?.CitizenName ?? "Unknown";
            ViewBag.ComplainantCNIC = fir?.CitizenCNIC ?? "";
            ViewBag.ComplainantPhone = fir?.PhoneNumber ?? "";

            return View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(string id, string status, string? notes)
        {
            var role = HttpContext.Session.GetString("OfficerRole");
            var c = _db.Cases.FirstOrDefault(x => x.CaseId == id);
            if (c == null) return NotFound();

            if (role != "Admin" && status == "Closed")
                return Forbid();

            c.Status = status;
            c.LastUpdated = DateTime.Now;
            if (!string.IsNullOrEmpty(notes))
            {
                var officer = HttpContext.Session.GetString("OfficerName");
                var timestamp = DateTime.Now.ToString("dd MMM yyyy HH:mm");
                var newEntry = $"[{timestamp} — {officer}] {notes}";
                c.InvestigationNotes = string.IsNullOrEmpty(c.InvestigationNotes)
                    ? newEntry
                    : c.InvestigationNotes + "\n\n" + newEntry;
            }
            if (status == "Closed" && !string.IsNullOrEmpty(notes))
                c.ClosureReason = notes;

            _db.SaveChanges();
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TransferCase(string id, string newOfficer)
        {
            if (HttpContext.Session.GetString("OfficerRole") != "Admin") return Forbid();
            var c = _db.Cases.FirstOrDefault(x => x.CaseId == id);
            if (c == null) return NotFound();
            var oldOfficer = c.AssignedOfficer;
            c.AssignedOfficer = newOfficer;
            c.LastUpdated = DateTime.Now;
            c.InvestigationNotes = (c.InvestigationNotes ?? "") +
                $"\n[{DateTime.Now:dd MMM yyyy HH:mm}] Case transferred from {oldOfficer} to {newOfficer} by DSP.";
            _db.SaveChanges();
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id)
        {
            if (HttpContext.Session.GetString("OfficerRole") != "Admin") return Forbid();
            var c = _db.Cases.FirstOrDefault(x => x.CaseId == id);
            if (c != null) { _db.Cases.Remove(c); _db.SaveChanges(); }
            return RedirectToAction("Index");
        }
    }
}

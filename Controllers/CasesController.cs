using Microsoft.AspNetCore.Mvc;
using SentinelPulse.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace SentinelPulse.Controllers
{
    public class CasesController : Controller
    {
        private readonly AppDbContext _db;
        public CasesController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index(int? pageNumber, string view = "active")
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("OfficerRole");
            var name = HttpContext.Session.GetString("OfficerName");
            bool isAdmin = role == "Admin" || role == "DSP";

            ViewBag.CurrentView = view;

            IQueryable<CaseModel> query = _db.Cases;

            if (view.ToLower() == "archived")
            {
                query = query.Where(c => c.Status == "Closed" || c.Status == "Rejected");
            }
            else
            {
                query = query.Where(c => c.Status != "Closed" && c.Status != "Rejected");
                if (!isAdmin)
                {
                    query = query.Where(c => c.AssignedOfficer == name);
                }
            }

            query = query.OrderByDescending(c => c.LastUpdated);

            int pageSize = 20;
            var cases = await PaginatedList<CaseModel>.CreateAsync(query, pageNumber ?? 1, pageSize);

            ViewBag.Complainants = _db.FIRs.ToDictionary(f => f.CaseId, f => f.CitizenName);

            return View(cases);
        }

        public IActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");
            var c = _db.Cases.FirstOrDefault(x => x.CaseId == id);
            if (c == null) return NotFound();

            var officerName = HttpContext.Session.GetString("OfficerName");
            var role = HttpContext.Session.GetString("OfficerRole");
            bool isAdmin = role == "Admin" || role == "DSP";
            bool isArchived = c.Status == "Closed" || c.Status == "Rejected";
            bool isAssigned = c.AssignedOfficer == officerName;

            if (!isAssigned && !isArchived && !isAdmin)
            {
                TempData["Error"] = "Unauthorized access. You can only view your own active cases.";
                return RedirectToAction("Index");
            }

            ViewBag.IsReadOnly = !isAdmin && (!isAssigned || isArchived);

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
            var officerName = HttpContext.Session.GetString("OfficerName");
            if (string.IsNullOrEmpty(officerName))
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("OfficerRole");
            var c = _db.Cases.FirstOrDefault(x => x.CaseId == id);
            if (c == null) return NotFound();

            // Only Admin/DSP or the assigned officer may update status
            bool isAdmin = role == "Admin" || role == "DSP";
            if (!isAdmin && c.AssignedOfficer != officerName)
            {
                TempData["Error"] = "You are not authorized to update this case. Only the assigned officer or Admin/DSP can change its status.";
                return RedirectToAction("Details", new { id });
            }

            if (!isAdmin && status == "Closed")
                return Forbid();

            c.Status = status;
            c.LastUpdated = DateTime.Now;

            var fir = _db.FIRs.FirstOrDefault(f => f.CaseId == id);
            if (fir != null)
            {
                fir.Status = status;
            }
            if (!string.IsNullOrEmpty(notes))
            {
                var timestamp = DateTime.Now.ToString("dd MMM yyyy HH:mm");
                var newEntry = $"[{timestamp} — {officerName}] {notes}";
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
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");

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
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("OfficerRole") != "Admin") return Forbid();
            var c = _db.Cases.FirstOrDefault(x => x.CaseId == id);
            if (c != null) { _db.Cases.Remove(c); _db.SaveChanges(); }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReopenCase(string id)
        {
            var officerName = HttpContext.Session.GetString("OfficerName");
            if (string.IsNullOrEmpty(officerName))
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("OfficerRole");
            if (role != "Admin" && role != "DSP")
                return Forbid();

            var c = _db.Cases.FirstOrDefault(x => x.CaseId == id);
            if (c == null) return NotFound();

            if (c.Status != "Closed" && c.Status != "Rejected")
                return RedirectToAction("Details", new { id });

            c.Status = "Open";
            c.ClosureReason = null;
            c.LastUpdated = DateTime.Now;
            
            var timestamp = DateTime.Now.ToString("dd MMM yyyy HH:mm");
            var newEntry = $"[{timestamp} — {officerName}] Case reopened by Admin/DSP.";
            c.InvestigationNotes = string.IsNullOrEmpty(c.InvestigationNotes) ? newEntry : c.InvestigationNotes + "\n\n" + newEntry;

            _db.SaveChanges();

            TempData["Success"] = "Case reopened successfully.";
            return RedirectToAction("Details", new { id });
        }
    }
}

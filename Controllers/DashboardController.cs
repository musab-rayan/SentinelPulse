using Microsoft.AspNetCore.Mvc;
using SentinelPulse.Models;

namespace SentinelPulse.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        public DashboardController(AppDbContext db) { _db = db; }

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("OfficerRole");
            var name = HttpContext.Session.GetString("OfficerName");

            ViewBag.ActiveZainabAlerts = _db.MissingChildren
                .Where(m => m.Status != "Closed" && m.Status != "Found Safe" && m.Status != "Found Deceased")
                .OrderByDescending(m => m.ReportedDate)
                .Take(4)
                .ToList();

            if (role == "Admin")
            {
                var vm = new DashboardViewModel
                {
                    ActiveCases = _db.Cases.Count(c => c.Status != "Closed"),
                    PendingFIRs = _db.Cases.Count(c => c.Status == "Pending Approval"),
                    TotalOfficers = _db.Officers.Count(o => o.Role == "Officer"),
                    ClosedCases = _db.Cases.Count(c => c.Status == "Closed"),
                    RecentFIRs = _db.FIRs.OrderByDescending(f => f.DateFiled).Take(5).ToList(),
                    CrimeDistribution = _db.FIRs.GroupBy(f => f.CrimeType)
                        .ToDictionary(g => g.Key, g => g.Count())
                };
                return View("AdminDashboard", vm);
            }
            else
            {
                var officerCaseIds = _db.Cases.Where(c => c.AssignedOfficer == name).Select(c => c.CaseId).ToList();
                var vm = new DashboardViewModel
                {
                    ActiveCases = _db.Cases.Count(c => c.AssignedOfficer == name && c.Status != "Closed"),
                    PendingFIRs = _db.Cases.Count(c => c.AssignedOfficer == name && c.Status == "Pending Approval"),
                    TotalOfficers = 0,
                    ClosedCases = _db.Cases.Count(c => c.AssignedOfficer == name && c.Status == "Closed"),
                    RecentFIRs = _db.FIRs.Where(f => officerCaseIds.Contains(f.CaseId)).OrderByDescending(f => f.DateFiled).Take(5).ToList(),
                    CrimeDistribution = new Dictionary<string, int>()
                };
                return View("OfficerDashboard", vm);
            }
        }

        public IActionResult CrimeMap()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("OfficerRole");
            var name = HttpContext.Session.GetString("OfficerName");

            List<CaseModel> cases;
            if (role == "Admin")
            {
                cases = _db.Cases.Where(c => c.Latitude != null && c.Longitude != null).ToList();
            }
            else
            {
                cases = _db.Cases.Where(c => c.AssignedOfficer == name && c.Latitude != null && c.Longitude != null).ToList();
            }

            ViewBag.ZainabAlerts = _db.MissingChildren
                .Where(m => m.Latitude != null && m.Longitude != null && m.Status != "Closed" && m.Status != "Found Safe" && m.Status != "Found Deceased")
                .ToList();

            return View(cases);
        }

        public IActionResult Officers()
        {
            if (HttpContext.Session.GetString("OfficerRole") != "Admin")
                return RedirectToAction("Index");
            var officers = _db.Officers.ToList();
            foreach (var o in officers)
            {
                ViewData[$"cases_{o.Id}"] = _db.Cases.Count(c => c.AssignedOfficer == o.Name && c.Status != "Closed");
            }
            return View(officers);
        }

        public IActionResult OfficerDetails(int id)
        {
            if (HttpContext.Session.GetString("OfficerRole") != "Admin")
                return RedirectToAction("Index");
            var officer = _db.Officers.FirstOrDefault(o => o.Id == id);
            if (officer == null) return NotFound();
            ViewBag.ActiveCases = _db.Cases.Where(c => c.AssignedOfficer == officer.Name && c.Status != "Closed").ToList();
            ViewBag.ClosedCases = _db.Cases.Count(c => c.AssignedOfficer == officer.Name && c.Status == "Closed");
            return View(officer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOfficerStatus(int id, string status)
        {
            if (HttpContext.Session.GetString("OfficerRole") != "Admin") return Forbid();
            var officer = _db.Officers.FirstOrDefault(o => o.Id == id);
            if (officer != null) { officer.Status = status; _db.SaveChanges(); }
            return RedirectToAction("Officers");
        }

        [HttpGet]
        public IActionResult ZainabAlert()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");
            ViewBag.ActiveAlerts = _db.MissingChildren
                .Where(m => m.Status != "Closed" && m.Status != "Found Safe" && m.Status != "Found Deceased")
                .OrderByDescending(m => m.ReportedDate)
                .ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ZainabAlert(MissingChildModel model)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");

            model.ReportedBy = HttpContext.Session.GetString("OfficerName") ?? "Unknown";
            model.ReportedDate = DateTime.Now;
            model.Status = "Active";
            model.Priority = "High";

            // Auto-assign to 2 active officers with fewest open cases
            var officers = _db.Officers
                .Where(o => o.Role == "Officer" && o.Status == "Active")
                .OrderBy(o => _db.Cases.Count(c => c.AssignedOfficer == o.Name && c.Status != "Closed"))
                .Take(2)
                .ToList();

            var assignedNames = officers.Any() ? string.Join(", ", officers.Select(o => o.Name)) : "Unassigned";
            model.AssignedOfficer = assignedNames;

            _db.MissingChildren.Add(model);

            var alert = new AlertModel
            {
                Message = $"ZAINAB ALERT: Missing child '{model.ChildName}', Age {model.Age}, last seen at {model.LastSeenLocation}. Assigned to: {assignedNames}",
                Station = "All Stations",
                Priority = "High",
                Timestamp = DateTime.Now
            };
            _db.Alerts.Add(alert);
            _db.SaveChanges();

            TempData["Success"] = $"Zainab Alert filed for {model.ChildName}. Alert ZA-{model.AlertId:D3} broadcast to all stations. Assigned: {assignedNames}.";
            return RedirectToAction("ZainabAlert");
        }

        public IActionResult OfficerMap()
        {
            // Redirect everyone to the unified CrimeMap
            return RedirectToAction("CrimeMap");
        }

        public IActionResult ZainabAlertDetails(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");

            var alert = _db.MissingChildren.FirstOrDefault(m => m.AlertId == id);
            if (alert == null) return NotFound();

            ViewBag.Evidence = _db.Evidence.Where(e => e.CaseId == id.ToString()).OrderByDescending(e => e.CollectedDate).ToList();
            ViewBag.Suspects = _db.Suspects.Where(s => s.CaseId == id.ToString()).ToList();
            return View(alert);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateZainabAlert(string id, string status, string? notes)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");

            var alert = _db.MissingChildren.FirstOrDefault(m => m.AlertId.ToString() == id);
            if (alert == null) return NotFound();

            alert.Status = status;
            alert.LastUpdated = DateTime.Now;
            alert.LastUpdatedBy = HttpContext.Session.GetString("OfficerName");
            if (!string.IsNullOrEmpty(notes))
            {
                var entry = $"[{DateTime.Now:dd MMM yyyy HH:mm} — {alert.LastUpdatedBy}] {notes}";
                alert.UpdateNotes = string.IsNullOrEmpty(alert.UpdateNotes) ? entry : alert.UpdateNotes + "\n\n" + entry;
            }
            _db.SaveChanges();
            return RedirectToAction("ZainabAlertDetails", new { id });
        }
    }
}

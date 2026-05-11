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
                var vm = new DashboardViewModel
                {
                    ActiveCases = _db.Cases.Count(c => c.AssignedOfficer == name && c.Status != "Closed"),
                    PendingFIRs = _db.Cases.Count(c => c.AssignedOfficer == name && c.Status == "Pending Approval"),
                    TotalOfficers = 0,
                    ClosedCases = _db.Cases.Count(c => c.AssignedOfficer == name && c.Status == "Closed"),
                    RecentFIRs = _db.FIRs.Where(f => f.District != null).OrderByDescending(f => f.DateFiled).Take(5).ToList(),
                    CrimeDistribution = new Dictionary<string, int>()
                };
                return View("OfficerDashboard", vm);
            }
        }

        public IActionResult CrimeMap()
        {
            if (HttpContext.Session.GetString("OfficerRole") != "Admin")
                return RedirectToAction("Index");
            var cases = _db.Cases.Where(c => c.Latitude != null && c.Longitude != null).ToList();
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
    }
}

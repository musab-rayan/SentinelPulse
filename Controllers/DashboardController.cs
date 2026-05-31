using Microsoft.AspNetCore.Mvc;
using SentinelPulse.Models;
using System.Threading.Tasks;
using System.Linq;

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
        public async Task<IActionResult> ZainabAlert(int? pageNumber, string view = "active")
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");

            ViewBag.CurrentView = view;
            IQueryable<MissingChildModel> query = _db.MissingChildren;

            if (view.ToLower() == "archived")
            {
                query = query.Where(m => m.Status == "Closed" || m.Status == "Found Safe" || m.Status == "Found Deceased");
            }
            else
            {
                query = query.Where(m => m.Status != "Closed" && m.Status != "Found Safe" && m.Status != "Found Deceased");
            }

            query = query.OrderByDescending(m => m.ReportedDate);

            int pageSize = 20;
            var alerts = await PaginatedList<MissingChildModel>.CreateAsync(query, pageNumber ?? 1, pageSize);

            ViewBag.ActiveAlerts = alerts;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ZainabAlert(MissingChildModel model, IFormFile? photo)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");

            if (photo != null && photo.Length > 0)
            {
                var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                {
                    ModelState.AddModelError("PhotoPath", "Only .jpg, .jpeg, and .png files are allowed.");
                }
                else if (photo.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("PhotoPath", "File size cannot exceed 5MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                var query = _db.MissingChildren
                    .Where(m => m.Status != "Closed" && m.Status != "Found Safe" && m.Status != "Found Deceased")
                    .OrderByDescending(m => m.ReportedDate);
                ViewBag.CurrentView = "active";
                ViewBag.ActiveAlerts = await PaginatedList<MissingChildModel>.CreateAsync(query, 1, 20);
                return View(model);
            }

            if (photo != null && photo.Length > 0)
            {
                // Save photo
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "missing-children");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(fileStream);
                }
                model.PhotoPath = "/uploads/missing-children/" + uniqueFileName;
            }
            else
            {
                model.PhotoPath = null;
            }

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
            _db.SaveChanges(); // Saves to DB and gets model.AlertId

            foreach (var o in officers)
            {
                _db.MissingChildAssignments.Add(new MissingChildAssignment { MissingChildId = model.AlertId, OfficerId = o.Id });
            }
            _db.SaveChanges();

            var alert = new AlertModel
            {
                Message = $"ZAINAB ALERT: Missing child '{model.ChildName}', Age {model.Age}, last seen at {model.LastSeenLocation}. Assigned to: {assignedNames}",
                Station = "All Stations",
                Priority = "High",
                Timestamp = DateTime.Now,
                MissingChildAlertId = model.AlertId
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
            ViewBag.AssignedOfficerIds = _db.MissingChildAssignments.Where(a => a.MissingChildId == id).Select(a => a.OfficerId).ToList();
            
            var officerName = HttpContext.Session.GetString("OfficerName");
            var loggedInOfficer = _db.Officers.FirstOrDefault(o => o.Name == officerName);
            ViewBag.CurrentOfficerId = loggedInOfficer?.Id ?? 0;
            var role = HttpContext.Session.GetString("OfficerRole");
            ViewBag.IsAdmin = role == "Admin" || role == "DSP";

            return View(alert);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMissingChildPhoto(int id, IFormFile photo)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");

            var alert = _db.MissingChildren.FirstOrDefault(m => m.AlertId == id);
            if (alert == null) return NotFound();

            if (photo == null || photo.Length == 0)
            {
                TempData["UploadError"] = "Please select a valid photo file.";
                return RedirectToAction("ZainabAlertDetails", new { id });
            }

            var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
            {
                TempData["UploadError"] = "Only .jpg, .jpeg, and .png files are allowed.";
                return RedirectToAction("ZainabAlertDetails", new { id });
            }
            if (photo.Length > 5 * 1024 * 1024)
            {
                TempData["UploadError"] = "File size cannot exceed 5MB.";
                return RedirectToAction("ZainabAlertDetails", new { id });
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "missing-children");
            Directory.CreateDirectory(uploadsFolder);

            if (!string.IsNullOrEmpty(alert.PhotoPath))
            {
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", alert.PhotoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(fileStream);
            }
            alert.PhotoPath = "/uploads/missing-children/" + uniqueFileName;
            _db.SaveChanges();

            TempData["Success"] = "Photo updated successfully.";
            return RedirectToAction("ZainabAlertDetails", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateZainabAlert(string id, string status, string? notes,
            string? lastSeenLocation, double? latitude, double? longitude, string? action)
        {
            var officerName = HttpContext.Session.GetString("OfficerName");
            if (string.IsNullOrEmpty(officerName))
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("OfficerRole");
            bool isAdmin = role == "Admin" || role == "DSP";
            var loggedInOfficer = _db.Officers.FirstOrDefault(o => o.Name == officerName);
            var officerId = loggedInOfficer?.Id ?? 0;

            var alert = _db.MissingChildren.FirstOrDefault(m => m.AlertId.ToString() == id);
            if (alert == null) return NotFound();
            var alertId = alert.AlertId;

            bool isAssigned = _db.MissingChildAssignments.Any(a => a.MissingChildId == alertId && a.OfficerId == officerId);

            if (action == "ApprovePending")
            {
                if (!isAdmin) return Forbid();
                if (!string.IsNullOrEmpty(alert.PendingStatus))
                {
                    alert.Status = alert.PendingStatus;
                    alert.PendingStatus = null;
                    alert.RequestedByOfficerId = null;
                    alert.RequestedAt = null;
                    alert.LastUpdated = DateTime.Now;
                    alert.LastUpdatedBy = officerName;
                    if (!string.IsNullOrEmpty(notes))
                    {
                        var entry = $"[{DateTime.Now:dd MMM yyyy HH:mm} — {officerName}] Approved closure to '{alert.Status}': {notes}";
                        alert.UpdateNotes = string.IsNullOrEmpty(alert.UpdateNotes) ? entry : alert.UpdateNotes + "\n\n" + entry;
                    }
                    _db.SaveChanges();
                }
                return RedirectToAction("ZainabAlertDetails", new { id });
            }
            if (action == "RejectPending")
            {
                if (!isAdmin) return Forbid();
                alert.PendingStatus = null;
                alert.RequestedByOfficerId = null;
                alert.RequestedAt = null;
                alert.LastUpdated = DateTime.Now;
                alert.LastUpdatedBy = officerName;
                if (!string.IsNullOrEmpty(notes))
                {
                    var entry = $"[{DateTime.Now:dd MMM yyyy HH:mm} — {officerName}] Rejected closure: {notes}";
                    alert.UpdateNotes = string.IsNullOrEmpty(alert.UpdateNotes) ? entry : alert.UpdateNotes + "\n\n" + entry;
                }
                _db.SaveChanges();
                return RedirectToAction("ZainabAlertDetails", new { id });
            }

            // Server-side whitelist of allowed status values
            var allowedStatuses = new HashSet<string>
            {
                "Active", "Active Search", "Under Investigation", "Lead Found", "Sighting Reported",
                "Closed", "Found Safe", "Found Deceased"
            };

            if (!allowedStatuses.Contains(status))
            {
                TempData["Error"] = $"Invalid status value: '{status}'. Update rejected.";
                return RedirectToAction("ZainabAlertDetails", new { id });
            }

            // Terminal/closing statuses check
            var terminalStatuses = new HashSet<string> { "Closed", "Found Safe", "Found Deceased" };

            if (terminalStatuses.Contains(status))
            {
                if (isAdmin)
                {
                    // Admin sets terminal directly
                    alert.Status = status;
                }
                else
                {
                    if (!isAssigned)
                    {
                        TempData["Error"] = "You are not authorized to update this alert. Only assigned officers or Admin/DSP can change its status.";
                        return RedirectToAction("ZainabAlertDetails", new { id });
                    }
                    // Officer requests approval
                    alert.PendingStatus = status;
                    alert.RequestedByOfficerId = officerId;
                    alert.RequestedAt = DateTime.Now;
                    
                    if (!string.IsNullOrEmpty(notes))
                    {
                        var entry = $"[{DateTime.Now:dd MMM yyyy HH:mm} — {officerName}] Requested {status}: {notes}";
                        alert.UpdateNotes = string.IsNullOrEmpty(alert.UpdateNotes) ? entry : alert.UpdateNotes + "\n\n" + entry;
                    }
                    _db.SaveChanges();
                    TempData["Success"] = $"Closure request for '{status}' submitted to Admin for approval.";
                    return RedirectToAction("ZainabAlertDetails", new { id });
                }
            }
            else
            {
                // Updating non-terminal status
                if (!isAdmin && !isAssigned)
                {
                    TempData["Error"] = "You are not authorized to update this alert. Only assigned officers or Admin/DSP can change its status.";
                    return RedirectToAction("ZainabAlertDetails", new { id });
                }
                alert.Status = status;
            }


            alert.LastUpdated = DateTime.Now;
            alert.LastUpdatedBy = HttpContext.Session.GetString("OfficerName");

            // Update location if provided
            if (!string.IsNullOrEmpty(lastSeenLocation))
                alert.LastSeenLocation = lastSeenLocation;
            if (latitude.HasValue && latitude != 0)
                alert.Latitude = latitude;
            if (longitude.HasValue && longitude != 0)
                alert.Longitude = longitude;

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

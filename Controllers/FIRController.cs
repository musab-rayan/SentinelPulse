using Microsoft.AspNetCore.Mvc;
using SentinelPulse.Models;

namespace SentinelPulse.Controllers
{
    public class FIRController : Controller
    {
        private readonly AppDbContext _db;
        public FIRController(AppDbContext db) { _db = db; }

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");

            var officerBadge = HttpContext.Session.GetString("OfficerBadge");
            var officerRole  = HttpContext.Session.GetString("OfficerRole");

            // Base query: only FIRs whose Case still exists (deleted cases hide their FIR)
            var query = _db.FIRs
                .Where(f => _db.Cases.Any(c => c.CaseId == f.CaseId));

            // Officers only see FIRs from cases assigned to them
            if (officerRole == "Officer")
            {
                var officerName2 = HttpContext.Session.GetString("OfficerName");
                query = query.Where(f =>
                    _db.Cases.Any(c => c.CaseId == f.CaseId &&
                                       c.AssignedOfficer == officerName2));
            }

            return View(query.OrderByDescending(f => f.DateFiled).ToList());
        }

        public IActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");

            var officerName = HttpContext.Session.GetString("OfficerName");
            var officerRole = HttpContext.Session.GetString("OfficerRole");

            var fir = _db.FIRs.FirstOrDefault(f => f.CaseId == id);
            if (fir == null) return NotFound();

            var matchingCase = _db.Cases.FirstOrDefault(c => c.CaseId == fir.CaseId);

            if (officerRole == "Admin" || (matchingCase != null && matchingCase.AssignedOfficer == officerName))
            {
                if (matchingCase != null) ViewBag.AssignedOfficer = matchingCase.AssignedOfficer;
                return View("Confirmation", fir);
            }

            TempData["Error"] = "You are not authorized to view this FIR.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return RedirectToAction("Login", "Account");
            return View(new FIRModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FIRModel model, bool forceSubmit = false)
        {
            if (!ModelState.IsValid) return View(model);

            // CNIC duplicate check — block first attempt
            var duplicate = _db.FIRs.FirstOrDefault(f => f.CitizenCNIC == model.CitizenCNIC && f.Status != "Closed");
            if (duplicate != null && !forceSubmit)
            {
                ViewBag.CNICWarning = $"An active FIR ({duplicate.CaseId}) already exists for this CNIC. Click 'Submit Anyway' to file another.";
                ViewBag.AllowForce = true;
                return View(model);
            }

            // Auto priority based on crime type
            var highPriority = new[] { "Assault", "Murder", "Kidnapping", "Terrorism", "Armed Robbery" };
            var lowPriority  = new[] { "Vandalism", "Noise Complaint", "Trespassing" };
            var autoPriority = highPriority.Contains(model.CrimeType) ? "High"
                : lowPriority.Contains(model.CrimeType) ? "Low" : "Medium";

            model.DateFiled = DateTime.Now;
            model.Status = "Open";
            var maxId = _db.FIRs.Any()
                ? _db.FIRs.Select(f => f.CaseId).ToList()
                    .Select(cid => int.TryParse(cid.Replace("CASE-", ""), out var n) ? n : 0).Max()
                : 0;
            model.CaseId = "CASE-" + (maxId + 1).ToString("D3");

            // If officer marked location on map, use those coordinates.
            // Otherwise fall back to Nominatim geocoding.
            if (model.Latitude == null || model.Latitude == 0 ||
                model.Longitude == null || model.Longitude == 0)
            {
                try
                {
                    using var http = new HttpClient();
                    http.DefaultRequestHeaders.Add("User-Agent", "SentinelPulse/1.0");
                    var query = System.Net.WebUtility.UrlEncode(model.Location + ", " + model.District + ", Pakistan");
                    var response = await http.GetStringAsync($"https://nominatim.openstreetmap.org/search?q={query}&format=json&limit=1");
                    var results = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(response);
                    if (results != null && results.Count > 0)
                    {
                        model.Latitude  = double.Parse(results[0]["lat"].ToString()!);
                        model.Longitude = double.Parse(results[0]["lon"].ToString()!);
                    }
                }
                catch { /* geocoding failed, continue without coords */ }
            }

            var desc = (model.Description ?? "").ToLower();
            var urgentWords = new[] { "murder", "killed", "death", "stabbed", "shot", "blood", "weapon", "gun", "knife", "hostage", "kidnap", "bomb", "attack", "assault", "rape", "threat", "emergency", "critical", "severe", "violent", "abducted" };
            var moderateWords = new[] { "theft", "stolen", "robbery", "fraud", "damage", "vandalism", "harassment", "drugs", "fight", "injured", "missing" };
            var urgentCount = urgentWords.Count(w => desc.Contains(w));
            var moderateCount = moderateWords.Count(w => desc.Contains(w));

            if (urgentCount >= 2) { model.SentimentLabel = "Critical"; model.SentimentScore = "High Urgency"; }
            else if (urgentCount == 1 || moderateCount >= 2) { model.SentimentLabel = "Concerning"; model.SentimentScore = "Medium Urgency"; }
            else if (moderateCount == 1) { model.SentimentLabel = "Standard"; model.SentimentScore = "Low Urgency"; }
            else { model.SentimentLabel = "Routine"; model.SentimentScore = "Minimal Urgency"; }

            _db.FIRs.Add(model);

            var newCase = new CaseModel
            {
                CaseId = model.CaseId,
                Title = model.CrimeType + " — " + model.Location,
                AssignedOfficer = HttpContext.Session.GetString("OfficerName") ?? "Unassigned",
                Status = "Open",
                Priority = autoPriority,
                DateOpened = DateTime.Now,
                LastUpdated = DateTime.Now,
                Latitude = model.Latitude,
                Longitude = model.Longitude
            };
            _db.Cases.Add(newCase);

            _db.SaveChanges();
            return View("Confirmation", model);
        }
    }
}

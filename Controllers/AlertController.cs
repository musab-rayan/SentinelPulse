using Microsoft.AspNetCore.Mvc;
using SentinelPulse.Models;

namespace SentinelPulse.Controllers
{
    public class AlertController : Controller
    {
        private readonly AppDbContext _db;
        public AlertController(AppDbContext db) { _db = db; }

        [HttpGet]
        public IActionResult Latest()
        {
            var terminalStatuses = new[] { "Found Safe", "Found Deceased", "Closed" };
            
            var alert = _db.Alerts
                .Where(a => a.MissingChildAlertId == null || 
                            _db.MissingChildren.Any(m => m.AlertId == a.MissingChildAlertId && 
                                                         !terminalStatuses.Contains(m.Status)))
                .OrderByDescending(a => a.Timestamp)
                .FirstOrDefault();
                
            if (alert == null) return Json(new { message = "" });
            
            string? photoPath = null;
            MissingChildModel? child = null;
            if (alert.MissingChildAlertId.HasValue)
            {
                child = _db.MissingChildren.FirstOrDefault(m => m.AlertId == alert.MissingChildAlertId.Value);
                if (child != null) photoPath = child.PhotoPath;
            }

            return Json(new { 
                message = alert.Message, 
                station = alert.Station, 
                priority = alert.Priority,
                missingChildAlertId = alert.MissingChildAlertId,
                photoPath = photoPath,
                childName = child?.ChildName,
                age = child?.Age,
                lastSeen = child?.LastSeenLocation,
                assignedTo = child?.AssignedOfficer
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Send([FromBody] AlertModel model)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return Unauthorized();
            model.Timestamp = DateTime.Now;
            _db.Alerts.Add(model);
            _db.SaveChanges();
            return Json(new { ok = true });
        }
    }
}

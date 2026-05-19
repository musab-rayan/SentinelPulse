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
            var alert = _db.Alerts.OrderByDescending(a => a.Timestamp).FirstOrDefault();
            if (alert == null) return Json(new { message = "" });
            return Json(new { message = alert.Message, station = alert.Station, priority = alert.Priority });
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

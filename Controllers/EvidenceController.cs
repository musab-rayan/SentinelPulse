using Microsoft.AspNetCore.Mvc;
using SentinelPulse.Models;

namespace SentinelPulse.Controllers
{
    public class EvidenceController : Controller
    {
        private readonly AppDbContext _db;
        public EvidenceController(AppDbContext db) { _db = db; }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(EvidenceModel model, string? ReturnToAlert)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return Unauthorized();

            model.CollectedBy = HttpContext.Session.GetString("OfficerName");
            model.CollectedDate = DateTime.Now;
            _db.Evidence.Add(model);
            _db.SaveChanges();

            if (!string.IsNullOrEmpty(ReturnToAlert))
                return RedirectToAction("ZainabAlertDetails", "Dashboard", new { id = model.CaseId });

            return RedirectToAction("Details", "Cases", new { id = model.CaseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, string caseId)
        {
            if (HttpContext.Session.GetString("OfficerRole") != "Admin") return Forbid();
            var e = _db.Evidence.FirstOrDefault(x => x.Id == id);
            if (e != null) { _db.Evidence.Remove(e); _db.SaveChanges(); }
            return RedirectToAction("Details", "Cases", new { id = caseId });
        }
    }
}

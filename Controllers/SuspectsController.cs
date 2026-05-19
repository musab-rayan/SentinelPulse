using Microsoft.AspNetCore.Mvc;
using SentinelPulse.Models;

namespace SentinelPulse.Controllers
{
    public class SuspectsController : Controller
    {
        private readonly AppDbContext _db;
        public SuspectsController(AppDbContext db) { _db = db; }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(SuspectModel model, string? ReturnToAlert)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return Unauthorized();

            model.AddedDate = DateTime.Now;
            _db.Suspects.Add(model);
            _db.SaveChanges();

            if (!string.IsNullOrEmpty(ReturnToAlert))
                return RedirectToAction("ZainabAlertDetails", "Dashboard", new { id = model.CaseId });

            return RedirectToAction("Details", "Cases", new { id = model.CaseId });
        }
    }
}

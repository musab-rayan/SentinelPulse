using Microsoft.AspNetCore.Mvc;
using SentinelPulse.Models;

namespace SentinelPulse.Controllers
{
    public class EvidenceController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public EvidenceController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(EvidenceModel model, IFormFile? MediaFile, string? ReturnToAlert)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["UploadError"] = string.IsNullOrEmpty(errors) ? "Invalid evidence details." : errors;
                if (!string.IsNullOrEmpty(ReturnToAlert))
                    return RedirectToAction("ZainabAlertDetails", "Dashboard", new { id = model.CaseId });
                return RedirectToAction("Details", "Cases", new { id = model.CaseId });
            }

            // Handle media file upload
            if (MediaFile != null && MediaFile.Length > 0)
            {
                // Validate extension
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".mp4" };
                var ext = Path.GetExtension(MediaFile.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                {
                    TempData["UploadError"] = "Only .jpg, .jpeg, .png, .pdf, and .mp4 files are allowed.";
                    if (!string.IsNullOrEmpty(ReturnToAlert))
                        return RedirectToAction("ZainabAlertDetails", "Dashboard", new { id = model.CaseId });
                    return RedirectToAction("Details", "Cases", new { id = model.CaseId });
                }

                // Validate size (10 MB)
                if (MediaFile.Length > 10 * 1024 * 1024)
                {
                    TempData["UploadError"] = "File size must not exceed 10 MB.";
                    if (!string.IsNullOrEmpty(ReturnToAlert))
                        return RedirectToAction("ZainabAlertDetails", "Dashboard", new { id = model.CaseId });
                    return RedirectToAction("Details", "Cases", new { id = model.CaseId });
                }

                // Save file
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "evidence");
                Directory.CreateDirectory(uploadsDir);

                var fileName = Guid.NewGuid().ToString() + ext;
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await MediaFile.CopyToAsync(stream);
                }

                model.FilePath = "/uploads/evidence/" + fileName;
            }

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

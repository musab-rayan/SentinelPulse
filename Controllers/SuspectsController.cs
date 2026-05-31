using Microsoft.AspNetCore.Mvc;
using SentinelPulse.Models;

namespace SentinelPulse.Controllers
{
    public class SuspectsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public SuspectsController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(SuspectModel model, IFormFile? Photo, string? ReturnToAlert)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OfficerName")))
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["UploadError"] = string.IsNullOrEmpty(errors) ? "Invalid suspect details." : errors;
                if (!string.IsNullOrEmpty(ReturnToAlert))
                    return RedirectToAction("ZainabAlertDetails", "Dashboard", new { id = model.CaseId });
                return RedirectToAction("Details", "Cases", new { id = model.CaseId });
            }

            // Handle photo upload
            if (Photo != null && Photo.Length > 0)
            {
                // Validate extension
                var allowed = new[] { ".jpg", ".jpeg", ".png" };
                var ext = Path.GetExtension(Photo.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                {
                    TempData["UploadError"] = "Only .jpg, .jpeg, and .png files are allowed for suspect photos.";
                    if (!string.IsNullOrEmpty(ReturnToAlert))
                        return RedirectToAction("ZainabAlertDetails", "Dashboard", new { id = model.CaseId });
                    return RedirectToAction("Details", "Cases", new { id = model.CaseId });
                }

                // Validate size (10 MB)
                if (Photo.Length > 10 * 1024 * 1024)
                {
                    TempData["UploadError"] = "File size must not exceed 10 MB.";
                    if (!string.IsNullOrEmpty(ReturnToAlert))
                        return RedirectToAction("ZainabAlertDetails", "Dashboard", new { id = model.CaseId });
                    return RedirectToAction("Details", "Cases", new { id = model.CaseId });
                }

                // Save file
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "suspects");
                Directory.CreateDirectory(uploadsDir);

                var fileName = Guid.NewGuid().ToString() + ext;
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Photo.CopyToAsync(stream);
                }

                model.PhotoPath = "/uploads/suspects/" + fileName;
            }

            model.AddedDate = DateTime.Now;
            _db.Suspects.Add(model);
            _db.SaveChanges();

            if (!string.IsNullOrEmpty(ReturnToAlert))
                return RedirectToAction("ZainabAlertDetails", "Dashboard", new { id = model.CaseId });

            return RedirectToAction("Details", "Cases", new { id = model.CaseId });
        }
    }
}

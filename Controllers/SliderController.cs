using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SL_Bullion.DAL;
using SL_Bullion.Models;

namespace SL_Bullion.Controllers
{
    public class SliderController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SliderController(BullionDbContext context, IToastNotification alert, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _alert = alert;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> List()
        {
            return View(await _context.tblSlider.Where(s => s.ClientId == HttpContext.Session.GetInt32("clientId")).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("SliderId,ClientId,SliderImage,SliderThumbnailPath,SliderPath")] Slider slider)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    slider.ClientId = HttpContext.Session.GetInt32("clientId") ?? 0;
                    if (slider.ClientId == 0)
                    {
                        ModelState.AddModelError("", "Invalid client session. Please log in again.");
                        return RedirectToAction(nameof(List));
                    }

                    int sliderCount = await _context.tblSlider.AsNoTracking().CountAsync(s => s.ClientId == slider.ClientId);

                    int totalSlider = await _context.tblMaster.AsNoTracking().Where(_ => _.id == slider.ClientId).Select(_ => _.totalSlider ?? 0).FirstOrDefaultAsync();

                    if (sliderCount >= totalSlider)
                    {
                        _alert.AddWarningToastMessage($"You can only add up to {totalSlider} sliders.");
                        return RedirectToAction(nameof(List));
                    }

                    if (slider.SliderImage != null && slider.SliderImage.Length > 0)
                    {
                        string path = SaveFile(slider.SliderImage);
                        if (string.IsNullOrEmpty(path))
                        {
                            ModelState.AddModelError("SliderImage", "Failed to save the image. Please try again.");
                            return RedirectToAction(nameof(List));
                        }

                        slider.SliderPath = path;

                        string thumbnailImagePath = SaveThumbnail(slider.SliderImage);
                        if (string.IsNullOrEmpty(thumbnailImagePath))
                        {
                            ModelState.AddModelError("SliderThumbnailPath", "Failed to save the thumbnail. Please try again.");
                            return RedirectToAction(nameof(List));
                        }

                        slider.SliderThumbnailPath = thumbnailImagePath;
                    }
                    slider.CreatedDate = DateTime.UtcNow;
                    _context.Add(slider);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(List));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(slider);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.tblSlider == null)
            {
                return NotFound();
            }

            var slider = await _context.tblSlider.FindAsync(id);
            if (slider == null)
            {
                return NotFound();
            }
            return PartialView("Action", slider);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("SliderId,ClientId,SliderImage,SliderThumbnailPath,SliderPath")] Slider slider)
        {
            slider.ClientId = HttpContext.Session.GetInt32("clientId") ?? 0;
            if (slider.ClientId == 0)
            {
                ModelState.AddModelError("", "Invalid client session. Please log in again.");
                return View(slider);
            }

            if (id != slider.SliderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (slider.SliderImage != null && slider.SliderImage.Length > 0)
                    {
                        string newPhotoPath = SaveFile(slider.SliderImage);
                        slider.SliderPath = newPhotoPath;
                    }

                    string thumbnailImagePath = SaveThumbnail(slider.SliderImage);
                    slider.SliderThumbnailPath = thumbnailImagePath;
                    slider.ModifiedDate = DateTime.UtcNow;
                    _context.Update(slider);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SliderExists(slider.SliderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(List));
            }
            return View(slider);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.tblSlider == null)
            {
                return NotFound();
            }

            var slider = await _context.tblSlider
                .FirstOrDefaultAsync(m => m.SliderId == id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.tblSlider == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Slider'  is null.");
            }

            var slider = await _context.tblSlider.FindAsync(id);

            if (slider != null)
            {
                // Delete Slider main image file
                if (!string.IsNullOrEmpty(slider.SliderPath))
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath,
                                                   slider.SliderPath.TrimStart('~', '/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Delete thumbnail image file
                if (!string.IsNullOrEmpty(slider.SliderThumbnailPath))
                {
                    string thumbPath = Path.Combine(_webHostEnvironment.WebRootPath,
                                                    slider.SliderThumbnailPath.TrimStart('~', '/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(thumbPath))
                    {
                        System.IO.File.Delete(thumbPath);
                    }
                }

                _context.tblSlider.Remove(slider);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(List));
        }

        private string SaveFile(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/slider");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var image = SixLabors.ImageSharp.Image.Load(file.OpenReadStream()))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(1400, 800)
                    }));

                    var encoder = new JpegEncoder
                    {
                        Quality = 90
                    };

                    image.Save(filePath, encoder);
                }

                //string fileUrl = Url.Content("~/SliderImage/" + uniqueFileName);
                string fileUrl = $"{Request.Scheme}://{Request.Host}/images/slider/{uniqueFileName}";
                return fileUrl;
            }
            return null;
        }

        private string SaveThumbnail(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "slider", "thumbnails");

                //Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/jewellery", "thumbnails");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_thumbnail" + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var image = SixLabors.ImageSharp.Image.Load(file.OpenReadStream()))
                {

                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(1000, 640),
                        Mode = ResizeMode.Crop
                    }));

                    var encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
                    {
                        Quality = 90
                    };

                    image.Save(filePath);
                }

                //string fileUrl = Url.Content("~/slider/thumbnails/" + uniqueFileName);
                string fileUrl = $"{Request.Scheme}://{Request.Host}/images/slider/thumbnails/{uniqueFileName}";
                return fileUrl;
            }
            return null;
        }

        private bool SliderExists(int id)
        {
            return (_context.tblSlider?.Any(e => e.SliderId == id)).GetValueOrDefault();
        }
    }
}

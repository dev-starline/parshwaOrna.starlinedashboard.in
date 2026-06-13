using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SL_Bullion.DAL;
using SL_Bullion.Models;

namespace SL_Bullion.Controllers
{
    public class JewelleryController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;

        public JewelleryController(BullionDbContext context, IToastNotification alert)
        {
            _context = context;
            _alert = alert;
        }

        [HttpGet]
        public JsonResult GetFirstSubCategory(int categoryId)
        {
            var subCategory = _context.tblSubCategory.Where(sc => sc.CategoryId == categoryId).OrderBy(sc => sc.Id)
                              .Select(sc => new
                              {
                                  id = sc.Id,
                                  name = sc.Name
                              }).ToList();

            return Json(subCategory);
        }

        public async Task<IActionResult> List()
        {
            int clientId = HttpContext.Session.GetInt32("clientId") ?? 0;

            var viewModel = new JewelleryViewModel
            {
                Categories = await _context.tblCategory.Where(c => c.ClientId == clientId && c.isDisplay).ToListAsync(),

                SubCategories = await _context.tblSubCategory.Where(s => s.ClientId == clientId /*&& s.isDisplay*/).ToListAsync(),

                Jewelleries = await _context.tblJewellery.Where(j => j.ClientId == clientId).ToListAsync()
            };

            ViewBag.Categories = _context.tblCategory.Where(c => c.ClientId == clientId && c.isDisplay).ToList();

            ViewBag.SubCategories = new SelectList(_context.tblSubCategory.Where(s => s.ClientId == clientId && s.isDisplay).ToList(), "Id", "Name");

            var master = await _context.tblMaster.FirstOrDefaultAsync(m => m.id == clientId);
            ViewBag.isJewellery = master?.isJewellery ?? false;
            ViewBag.isCategory = master?.isCategory ?? false;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddJewellery([Bind("Id,CategoryId,SubCategoryId,ClientId,isDisplay,TagNo,Description,Name,Image,CreatedDate,ModifiedDate")] Jewellery jewellery, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                var clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();

                jewellery.ClientId = clientId;
                jewellery.CreatedDate = DateTime.Now;
                jewellery.isDisplay = true;

                _context.tblJewellery.Add(jewellery);
                await _context.SaveChangesAsync();

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/jewellery", clientId.ToString(), jewellery.Id.ToString());

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var imageName = ImageFile.FileName;
                    var imagePath = Path.Combine(folderPath, imageName);

                    using (var image = await Image.LoadAsync(ImageFile.OpenReadStream()))
                    {
                        int maxWidth = 1200;
                        if (image.Width > maxWidth)
                        {
                            image.Mutate(x => x.Resize(new ResizeOptions
                            {
                                Mode = ResizeMode.Max,
                                Size = new Size(maxWidth, 0) // height auto adjust
                            }));
                        }

                        var encoder = new JpegEncoder()
                        {
                            Quality = 70
                        };

                        await image.SaveAsync(imagePath, encoder);
                    }

                    //using (var stream = new FileStream(imagePath, FileMode.Create))
                    //{
                    //    await ImageFile.CopyToAsync(stream);
                    //}

                    var imageUrl = $"{Request.Scheme}://{Request.Host}/images/jewellery/{clientId}/{jewellery.Id}/{imageName}";
                    jewellery.Image = imageUrl;

                    _context.tblJewellery.Update(jewellery);
                    await _context.SaveChangesAsync();
                }

                _alert.AddSuccessToastMessage("Jewellery created.");
                return RedirectToAction("List");
            }

            return View(jewellery);
        }

        [HttpPost]
        public async Task<IActionResult> EditJewellery(Jewellery jewellery, IFormFile? ImageFile)
        {
            var existingJewellery = await _context.tblJewellery.FindAsync(jewellery.Id);
            if (existingJewellery == null)
                return NotFound();

            existingJewellery.TagNo = jewellery.TagNo;
            existingJewellery.Name = jewellery.Name;
            existingJewellery.Description = jewellery.Description;
            existingJewellery.isDisplay = jewellery.isDisplay;
            //existingJewellery.isHome = jewellery.isHome;
            existingJewellery.ModifiedDate = DateTime.Now;

            var clientId = existingJewellery.ClientId;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/jewellery", clientId.ToString(), existingJewellery.Id.ToString());

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = ImageFile.FileName;
                var filePath = Path.Combine(folderPath, fileName);

                using (var image = await Image.LoadAsync(ImageFile.OpenReadStream()))
                {
                    int maxWidth = 1200;
                    if (image.Width > maxWidth)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Max,
                            Size = new Size(maxWidth, 0)
                        }));
                    }

                    var encoder = new JpegEncoder()
                    {
                        Quality = 70
                    };

                    await image.SaveAsync(filePath, encoder);
                }

                if (!string.IsNullOrEmpty(existingJewellery.Image))
                {
                    var oldRelativePath = existingJewellery.Image.Replace($"{Request.Scheme}://{Request.Host}", "").TrimStart('/');
                    var oldPhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldRelativePath);

                    if (System.IO.File.Exists(oldPhysicalPath))
                        System.IO.File.Delete(oldPhysicalPath);
                }

                var imageUrl = $"{Request.Scheme}://{Request.Host}/images/jewellery/{clientId}/{existingJewellery.Id}/{fileName}";

                existingJewellery.Image = imageUrl;
            }

            await _context.SaveChangesAsync();
            _alert.AddSuccessToastMessage("Jewellery Edited.");
            return RedirectToAction("List");
        }

        [HttpPost, ActionName("DeleteJewellery")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJewellery(int id)
        {
            var jewellery = await _context.tblJewellery.FindAsync(id);

            if (jewellery == null)
                return NotFound();

            if (!string.IsNullOrEmpty(jewellery.Image))
            {
                var oldRelativePath = jewellery.Image.Replace($"{Request.Scheme}://{Request.Host}", "").TrimStart('/');
                var oldPhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldRelativePath);

                if (System.IO.File.Exists(oldPhysicalPath))
                    System.IO.File.Delete(oldPhysicalPath);
            }

            _context.tblJewellery.Remove(jewellery);
            await _context.SaveChangesAsync();
            _alert.AddSuccessToastMessage("Jewellery deleted.");

            return RedirectToAction(nameof(List));
        }

        [HttpGet]
        public async Task<IActionResult> SearchJewellery(string searchText)
        {
            var clientId = HttpContext.Session.GetInt32("clientId") ?? 0;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return RedirectToAction("List");
            }

            var filteredJewelleries = await (from j in _context.tblJewellery
                                             join c in _context.tblCategory on j.CategoryId equals c.Id into categoryJoin
                                             from c in categoryJoin.DefaultIfEmpty()
                                             join s in _context.tblSubCategory on j.SubCategoryId equals s.Id into subCategoryJoin
                                             from s in subCategoryJoin.DefaultIfEmpty()
                                             where j.ClientId == clientId && (
                                             (!string.IsNullOrEmpty(j.TagNo) && j.TagNo.ToLower().Contains(searchText.ToLower())) ||
                                             (!string.IsNullOrEmpty(j.Name) && j.Name.ToLower().Contains(searchText.ToLower())) ||
                                             (!string.IsNullOrEmpty(j.Description) && j.Description.ToLower().Contains(searchText.ToLower())) ||
                                             (c != null && c.Name.ToLower().Contains(searchText.ToLower())) ||
                                             (s != null && s.Name.ToLower().Contains(searchText.ToLower())))
                                             select new Jewellery
                                             {
                                                 Id = j.Id,
                                                 TagNo = j.TagNo,
                                                 Name = j.Name,
                                                 Description = j.Description,
                                                 Image = j.Image,
                                                 isDisplay = j.isDisplay,
                                                 //isHome = j.isHome,
                                                 CategoryId = j.CategoryId,
                                                 SubCategoryId = j.SubCategoryId,
                                                 CreatedDate = j.CreatedDate
                                             }).ToListAsync();

            var viewModel = new JewelleryViewModel
            {
                Categories = await _context.tblCategory.Where(c => c.ClientId == clientId).ToListAsync(),
                SubCategories = await _context.tblSubCategory.Where(s => s.ClientId == clientId).ToListAsync(),
                Jewelleries = filteredJewelleries
            };

            ViewBag.Categories = _context.tblCategory.Where(c => c.ClientId == clientId).ToList();

            ViewBag.SubCategories = new SelectList(
                                    _context.tblSubCategory.Where(s => s.ClientId == clientId).ToList(), "Id", "Name");
            ViewBag.SearchText = searchText;
            return View("List", viewModel);
        }
    }
}

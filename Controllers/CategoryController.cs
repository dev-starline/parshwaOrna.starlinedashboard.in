using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.DAL;
using SL_Bullion.Models;

namespace SL_Bullion.Controllers
{
    public class CategoryController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;

        public CategoryController(BullionDbContext context, IToastNotification alert)
        {
            _context = context;
            _alert = alert;
        }

        [HttpGet]
        public IActionResult List()
        {
            var clientId = HttpContext.Session.GetInt32("clientId") ?? 0;

            var categories = _context.tblCategory
         .Where(c => c.ClientId == clientId).ToList();

            var isActiveCategories = _context.tblCategory.Where(c => c.ClientId == clientId && c.isDisplay);

            var subCategories = _context.tblSubCategory
                .Where(sc => sc.ClientId == clientId).ToList();

            var viewModel = new CategorySubCategoryViewModel
            {
                Categories = categories,
                SubCategories = subCategories
            };

            ViewBag.CategoryList = new SelectList(isActiveCategories, "Id", "Name");

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,ClientId,Name,isDisplay,CreatedDate,ModifiedDate")] Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            category.ClientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
            category.CreatedDate = DateTime.UtcNow;
            var data = _context.tblCategory.Add(category);
            await _context.SaveChangesAsync();

            if (category.Id > 0)
            {
                _alert.AddSuccessToastMessage("Category created.");
            }

            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingCategory = await _context.tblCategory.AsNoTracking().FirstOrDefaultAsync(x => x.Id == category.Id);

                    if (existingCategory == null)
                        return NotFound();


                    category.ClientId = existingCategory.ClientId;
                    category.CreatedDate = existingCategory.CreatedDate;
                    category.ModifiedDate = DateTime.Now;

                    _context.tblCategory.Update(category);
                    await _context.SaveChangesAsync();

                    _alert.AddSuccessToastMessage("Category Edited.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            return View(category);
        }

        private bool CategoryExists(int id)
        {
            return _context.tblCategory.Any(e => e.Id == id);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.tblCategory.FindAsync(id);
            var subCategories = await _context.tblSubCategory.Where(_ => _.CategoryId == category.Id).ToListAsync();
            var jewellery = await _context.tblJewellery.Where(_ => _.CategoryId == category.Id).ToListAsync();

            if (subCategories.Any() && jewellery.Any())
            {
                _alert.AddWarningToastMessage("Category cannot be deleted because it's connected to one or more subcategories.");
                return RedirectToAction(nameof(List));
            }

            if (category != null)
            {
                _context.tblCategory.Remove(category);
                await _context.SaveChangesAsync();
                _alert.AddSuccessToastMessage("Category deleted.");
            }
            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubCategory([Bind("Id,CategoryId,ClientId,Name,isDisplay,CreatedDate,ModifiedDate")] SubCategory subCategory)
        {
            if (!ModelState.IsValid)
            {
                var categories = _context.tblCategory.Where(c => c.ClientId == HttpContext.Session.GetInt32("clientId")).ToList();

                ViewBag.CategoryList = new SelectList(categories, "Id", "Name");

                return View(subCategory);
            }

            subCategory.ClientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();

            subCategory.CreatedDate = DateTime.UtcNow;

            _context.tblSubCategory.Add(subCategory);
            await _context.SaveChangesAsync();

            if (subCategory.Id > 0)
            {
                _alert.AddSuccessToastMessage("Sub-category created.");
            }
            return RedirectToAction(nameof(List));
        }

        [HttpPost, ActionName("EditSubCategory")]
        public async Task<IActionResult> EditSubCategory(SubCategory subCategory)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingSubCategory = await _context.tblSubCategory.AsNoTracking().FirstOrDefaultAsync(x => x.Id == subCategory.Id);

                    if (existingSubCategory == null)
                        return NotFound();

                    subCategory.ClientId = existingSubCategory.ClientId;
                    subCategory.CreatedDate = existingSubCategory.CreatedDate;
                    subCategory.ModifiedDate = DateTime.Now;

                    _context.tblSubCategory.Update(subCategory);
                    await _context.SaveChangesAsync();

                    _alert.AddSuccessToastMessage("Sub-Category Edited.");
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(subCategory.Id))
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
            return View(subCategory);
        }

        [HttpPost, ActionName("DeleteSubCategory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubCategory(int id)
        {
            var subCategory = await _context.tblSubCategory.FindAsync(id);
            var jewellery = await _context.tblJewellery.Where(_ => _.SubCategoryId == subCategory.Id).ToListAsync();

            if (jewellery.Any())
            {
                _alert.AddWarningToastMessage("SubCategory cannot be deleted because it's connected to one or more jewelleries.");
                return RedirectToAction(nameof(List));
            }

            if (subCategory != null)
            {
                _context.tblSubCategory.Remove(subCategory);
                await _context.SaveChangesAsync();
                _alert.AddSuccessToastMessage("Sub-Category deleted.");
            }

            return RedirectToAction(nameof(List));
        }
    }
}

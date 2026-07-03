using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using SL_Bullion.Constant;

namespace SL_Bullion.Controllers
{
    public class ContactController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationConstant _constatnt;

        public ContactController(BullionDbContext context, IToastNotification alert, IWebHostEnvironment webHostEnvironment, ApplicationConstant constatnt)
        {
            _context = context;
            _alert = alert;
            _webHostEnvironment = webHostEnvironment;
            _constatnt = constatnt;
        }

        // GET: Contact
        public async Task<IActionResult> List()
        {
            return View();
        }
        public JsonResult getContactDetails()
        {
            var result = _context.tblContact.Where(s => s.clientId == HttpContext.Session.GetInt32("clientId")).ToList();
            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,clientId,marqueeTop,marqueeBottom,marqueeTop1,marqueeBottom1,number1,number2,number3,number4,number5,number6,number7,whatsAppNo,address1,address2,address3,email1,email2,isBuy,isSell,isHigh,isLow,bannerWeb,bannerApp,modifiedDate,bannerWebImage,bannerAppImage")] Contact contact)
        {
            if (id != contact.id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(contact.silverCoinHeader));
            ModelState.Remove(nameof(contact.goldCoinHeader));
            ModelState.Remove(nameof(contact.isSilverCoinHeader));
            ModelState.Remove(nameof(contact.isGoldCoinHeader));
            ModelState.Remove(nameof(contact.isCoinRate));

            if (ModelState.IsValid)
            {
                try
                {
                    string? fileUrl = null;
                    contact.clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
                    fileUrl = saveFile(contact.bannerWebImage, contact.clientId, "web");
                    contact.bannerWeb = fileUrl;
                    fileUrl = saveFile(contact.bannerAppImage, contact.clientId, "app");
                    contact.bannerApp = fileUrl;
                    var existingData = _context.tblContact.Where(e => e.id == contact.id).SingleOrDefault();
                    if (existingData != null)
                    {
                        contact.isRate = existingData.isRate;
                        contact.isHedge = existingData.isHedge;
                        contact.goldDifferance = existingData.goldDifferance;
                        contact.silverDifferance = existingData.silverDifferance;
                        if (contact.bannerWebImage == null)
                        {
                            contact.bannerWeb = existingData.bannerWeb;
                        }
                        if (contact.bannerAppImage == null)
                        {
                            contact.bannerApp = existingData.bannerApp;
                        }
                        contact.silverCoinHeader = existingData.silverCoinHeader;
                        contact.goldCoinHeader = existingData.goldCoinHeader;
                        contact.isSilverCoinHeader = existingData.isSilverCoinHeader;
                        contact.isGoldCoinHeader = existingData.isGoldCoinHeader;
                        contact.isCoinTrade = existingData.isCoinRate;
                        _context.Entry(existingData).State = EntityState.Detached;
                    }

                    if (!clientExists(contact.clientId))
                    {
                        _context.Add(contact);
                    }
                    else
                    {
                        _context.Update(contact);
                    }
                    await _context.SaveChangesAsync();
                    _constatnt.pushContactDetails(contact.clientId);
                    _alert.AddSuccessToastMessage("Updated contact details.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.id))
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
            return View(contact);
        }

        private string saveFile(IFormFile? file, int clientId, string type)
        {
            string returnData = null;
            if (file != null)
            {
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images\\banner\\" + clientId + "\\" + type + "");
                string fileExtension = Path.GetExtension(file.FileName);
                returnData = Path.Combine($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + "/images/banner/" + clientId + "/" + type + "/" + file.FileName);
                string filePath = Path.Combine(uploadFolder, file.FileName);
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
            }
            return returnData;
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string type)
        {
            try
            {
                var contact = await _context.tblContact.FindAsync(id);
                if (contact == null)
                {
                    _alert.AddWarningToastMessage("Contact not found.");
                    return RedirectToAction(nameof(List));
                }

                string filePathToDelete = null;

                if (type == "web" && !string.IsNullOrEmpty(contact.bannerWeb))
                {
                    filePathToDelete = GetPhysicalPath(contact.bannerWeb);
                    contact.bannerWeb = null;
                }
                else if (type == "app" && !string.IsNullOrEmpty(contact.bannerApp))
                {
                    filePathToDelete = GetPhysicalPath(contact.bannerApp);
                    contact.bannerApp = null;
                }
                else
                {
                    _alert.AddInfoToastMessage("No banner found to delete.");
                    return RedirectToAction(nameof(List));
                }

                if (!string.IsNullOrEmpty(filePathToDelete) && System.IO.File.Exists(filePathToDelete))
                {
                    System.IO.File.Delete(filePathToDelete);
                }

                _context.Update(contact);
                await _context.SaveChangesAsync();

                _constatnt.pushContactDetails(contact.clientId);

                _alert.AddSuccessToastMessage("Banner deleted successfully.");
            }
            catch (Exception)
            {
                _alert.AddErrorToastMessage("Failed to delete banner.");
            }

            return RedirectToAction(nameof(List));
        }

        private string GetPhysicalPath(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
                return null;

            var cleanPath = virtualPath.Split('?')[0].Trim();

            if (Uri.TryCreate(cleanPath, UriKind.Absolute, out Uri uri))
            {
                cleanPath = uri.AbsolutePath;
            }
            cleanPath = cleanPath.TrimStart('~', '/');

            return Path.Combine(_webHostEnvironment.WebRootPath, cleanPath);
        }

        private bool clientExists(int clientId)
        {
            return _context.tblContact.Any(e => e.clientId == clientId);
        }

        private bool ContactExists(int id)
        {
            return _context.tblContact.Any(e => e.id == id);
        }

    }
}

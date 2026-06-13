using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using System.Globalization;
using System.Reflection.Metadata;

namespace SL_Bullion.Controllers
{
    public class UpdateController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        private readonly ApplicationConstant _constatnt;

        public UpdateController(BullionDbContext context, IToastNotification alert, ApplicationConstant constatnt)
        {
            _context = context;
            _alert = alert;
            _constatnt = constatnt;
        }
        public async Task<IActionResult> List()
        {
            int? clientId = HttpContext.Session.GetInt32("clientId");

            DateTime today = DateTime.Today;
            DateTime todayEnd = today.AddDays(1).AddTicks(-1);

            var data = await _context.tblUpdate
                .Where(s => s.clientId == clientId && s.modifiedDate >= today && s.modifiedDate <= todayEnd)
                .OrderByDescending(s => s.modifiedDate).ToListAsync();

            ViewBag.FromDate = today;
            ViewBag.ToDate = today;

            return View(data);
        }

        public async Task<IActionResult> search(string fromDate, string toDate)
        {
            DateTime fromDateValue, toDateValue;

            string dateFormat = "yyyy-MM-dd";

            if (!DateTime.TryParseExact(fromDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDateValue) ||
            !DateTime.TryParseExact(toDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out toDateValue))
            {
                _alert.AddSuccessToastMessage("Invalid date format.");
                return RedirectToAction(nameof(List));
            }

            ViewBag.FromDate = fromDateValue;
            ViewBag.ToDate = toDateValue;

            toDateValue = toDateValue.AddDays(1).AddTicks(-1);

            int? clientId = HttpContext.Session.GetInt32("clientId");

            toDateValue = toDateValue.Date.Add(new TimeSpan(23, 59, 59));
            var update = await _context.tblUpdate.Where(s => s.clientId == clientId &&
                                                        s.modifiedDate >= fromDateValue &&
                                                        s.modifiedDate <= toDateValue)
                                     .OrderByDescending(s => s.modifiedDate).ToListAsync();

            return View("List", update);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Update update)
        {
            if (string.IsNullOrWhiteSpace(update.title))
            {
                _alert.AddErrorToastMessage("Title is required.");
                return RedirectToAction(nameof(List));
            }
            if (string.IsNullOrWhiteSpace(update.message))
            {
                _alert.AddErrorToastMessage("Message is required.");
                return RedirectToAction(nameof(List));
            }
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(List));
            }
            update.clientId = HttpContext.Session.GetInt32("clientId") ?? 0;
            _context.tblUpdate.Add(update);
            await _context.SaveChangesAsync();
            _constatnt.pushAlert(update.clientId, update.title, update.message, "1");
            _alert.AddSuccessToastMessage("Update successfully created.");
            return RedirectToAction(nameof(List));
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var update = await _context.tblUpdate.FindAsync(id);
            if (update != null)
            {
                _context.tblUpdate.Remove(update);
            }

            await _context.SaveChangesAsync();
            _alert.AddSuccessToastMessage("Update deleted.");
            return RedirectToAction(nameof(List));
        }


        [HttpPost, ActionName("Resend")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resend(int id)
        {

            int clientId = HttpContext.Session.GetInt32("clientId") ?? 0;

            var original = await _context.tblUpdate.FindAsync(id);
            if (original == null || original.clientId != clientId)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(original.title) || string.IsNullOrEmpty(original.message))
            {
                return BadRequest("Title or message cannot be empty.");
            }

            var copy = new SL_Bullion.Models.Update
            {
                clientId = original.clientId,
                title = original.title,
                message = original.message,
                description = original.description,
                modifiedDate = DateTime.Now
            };

            try
            {
                _context.tblUpdate.Add(copy);
                await _context.SaveChangesAsync();
                _constatnt.pushAlert(copy.clientId, copy.title, copy.message, "1");
            }
            catch (Exception ex)
            {

                _alert.AddErrorToastMessage("Failed to resend message." + ex.Message);
                return RedirectToAction(nameof(List));
            }

            _alert.AddSuccessToastMessage("Message resent successfully.");
            return RedirectToAction(nameof(List));
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.DAL;

namespace SL_Bullion.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        public FeedbackController(BullionDbContext context, IToastNotification alert)
        {
            _context = context;
            _alert = alert;
        }
        public async Task<IActionResult> List()
        {
            return View(await _context.tblFeedback.AsNoTracking().Where(s => s.clientId == HttpContext.Session.GetInt32("clientId")).OrderByDescending(s => s.id).ToListAsync());
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var Feedback = await _context.tblFeedback.FindAsync(id);
            if (Feedback != null)
            {
                _context.tblFeedback.Remove(Feedback);
            }

            await _context.SaveChangesAsync();
            _alert.AddSuccessToastMessage("Feedback deleted.");
            return RedirectToAction(nameof(List));
        }
    }
}

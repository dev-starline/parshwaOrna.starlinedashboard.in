using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.DAL;
using SL_Bullion.Models;

namespace SL_Bullion.Controllers
{
    public class OrderLogController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        private const int PageSize = 20;

        public OrderLogController(BullionDbContext context,IToastNotification alert)
        {
            _context = context;
            _alert = alert;
        }

        public async Task<IActionResult> List(int page = 1)
        {
            var clientId = HttpContext.Session.GetInt32("clientId");

            if (clientId == null)
            {
                return RedirectToAction("Login", "Account");
            }
          
            var query = from log in _context.tblOrderFailureLog.AsNoTracking()
                        join sym in _context.tblSymbol.AsNoTracking()
                        on log.symbolId equals sym.id
                        where log.clientId == clientId
                        orderby log.id descending
                        select new OrderFailureLogViewModel
                        {
                            id = log.id,
                            clientId = log.clientId,
                            loginId = log.loginId,
                            name = log.name,
                            firmName = log.firmName,
                            mobileNumber = log.mobile,
                            quantity = log.quantity,
                            price = log.price,
                            reason = log.reason,
                            createdAt = log.cdate,
                            symbolName = sym.name
                        };
         
            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);          
            var logs = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();          
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(logs);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var log = await _context.tblOrderFailureLog.FindAsync(id);

            if (log != null)
            {
                _context.tblOrderFailureLog.Remove(log);
                await _context.SaveChangesAsync();

                _alert.AddSuccessToastMessage("Order log deleted successfully.");
            }

            return RedirectToAction(nameof(List));
        }
    }
}
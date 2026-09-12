using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using System.Text.Json.Nodes;

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

        [HttpPost]
        public async Task<IActionResult> selectedItems([FromBody] JsonArray obj)
        {
            try
            {
                if (obj == null || obj.Count == 0)
                    return BadRequest("No records selected.");

                string type = obj[0]["type"]?.ToString();

                if (type == "delete")
                {
                    var ids = obj.Select(x => Convert.ToInt32(x["id"]?.ToString())).ToList();
                    var records = await _context.tblOrderFailureLog.Where(x => ids.Contains(x.id)).ToListAsync();
                    _context.tblOrderFailureLog.RemoveRange(records);
                    await _context.SaveChangesAsync();
                    _alert.AddSuccessToastMessage("Order log deleted successfully.");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
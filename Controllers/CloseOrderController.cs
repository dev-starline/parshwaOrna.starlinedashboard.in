using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using System.Globalization;

namespace SL_Bullion.Controllers
{
    public class CloseOrderController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        private readonly ApplicationConstant _constatnt;
        private readonly AdminService _adminService;
        public CloseOrderController(BullionDbContext context, IToastNotification alert, ApplicationConstant constatnt, AdminService adminService)
        {
            _context = context;
            _alert = alert;
            _constatnt = constatnt;
            _adminService = adminService;
        }
        public async Task<IActionResult> List()
        {
            var orders = getOrder("", "", DateTime.Now, DateTime.Now);
            return View(await orders);
        }

        private async Task<List<CloseOrder>> getOrder(string type, string data, DateTime fromDate, DateTime toDate)
        {
            var query = from o in _context.tblCloseOrder
                        join a in _context.tblAccount
                            on new { o.clientId, o.loginId }
                            equals new { a.clientId, a.loginId } into ac
                        from a in ac.DefaultIfEmpty()
                        where o.clientId == HttpContext.Session.GetInt32("clientId")
                        select new CloseOrder
                        {
                            id = o.id,
                            dealNo = o.dealNo,
                            loginId = o.loginId,
                            name = _context.tblAccount.Where(a => a.clientId == o.clientId && a.loginId == o.loginId).Select(a => a.name).FirstOrDefault(),
                            firm = _context.tblAccount.Where(a => a.clientId == o.clientId && a.loginId == o.loginId).Select(a => a.firmName).FirstOrDefault(),
                            symbolName = o.symbolName,
                            rateType = o.rateType,
                            tradeTypeView = o.tradeType == 1 ? "Buy" : o.tradeType == 2 ? "Sell" : o.tradeType == 3 ? "BuyLimit" : "SellLimit",
                            volumeOpen = o.volumeOpen,
                            volume = o.volume,
                            exchange = o.exchange,
                            rateOpen = o.rateOpen,
                            rate = o.rate,
                            total = o.total,
                            deviceType = o.deviceType,
                            orderTime = o.orderTime,
                            editorderTime = o.editorderTime,
                            closeTime = o.closeTime,
                            ip = o.ip,
                            comment = o.comment
                        };

            if (type == "search")
            {
                if (!string.IsNullOrEmpty(data))
                {
                    if (int.TryParse(data, out var parsedDealNo))
                    {
                        query = query.Where(o =>
                            (o.loginId.Contains(data) || o.dealNo == parsedDealNo) &&
                            (o.closeTime.Date >= fromDate.Date && o.closeTime.Date <= toDate.Date));
                    }
                    else
                    {
                        query = query.Where(o =>
                             (o.loginId.Contains(data) || o.name.Contains(data) || o.firm.Contains(data)) &&
                             (o.closeTime >= fromDate && o.closeTime <= toDate));
                    }
                }
                else
                {
                    query = query.Where(o =>
                        o.closeTime.Date >= fromDate.Date && o.closeTime.Date <= toDate.Date);
                }
            }

            else
            {
                // When no search → show ONLY today's closed orders
                var today = DateTime.Today;
                query = query.Where(o => o.closeTime.Date == today);
            }

            var orders = await query.OrderByDescending(o => o.closeTime).ToListAsync();
            return orders;
        }

        public async Task<IActionResult> search(string fromDate, string toDate, string loginId)
        {
            DateTime fromDateValue;
            DateTime toDateValue;
            string dateFormat = "yyyy-MM-dd";
            if (!DateTime.TryParseExact(fromDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDateValue) ||
            !DateTime.TryParseExact(toDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out toDateValue))
            {
                _alert.AddSuccessToastMessage("Invalid date format.");
                return RedirectToAction(nameof(List));
            }
            toDateValue = toDateValue.Date.Add(new TimeSpan(23, 59, 59));
            var orders = getOrder("search", loginId, fromDateValue, toDateValue);
            return View("List", await orders);
        }

        [HttpPost, ActionName("Open")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenConfirmed(int id)
        {
            await _adminService.removeOrder(id, "close", "open");
            await _context.SaveChangesAsync();
            _alert.AddSuccessToastMessage("order open.");
            return RedirectToAction(nameof(List));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _adminService.removeOrder(id, "close", "delete");
            await _context.SaveChangesAsync();
            _alert.AddSuccessToastMessage("order open.");
            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> ExportToExcel(string fromDate, string toDate)
        {
            int clientId = HttpContext.Session.GetInt32("clientId") ?? 0;
            DateTime fromDateValue;
            DateTime toDateValue;
            string dateFormat = "yyyy-MM-dd";
            if (!DateTime.TryParseExact(fromDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDateValue) ||
                !DateTime.TryParseExact(toDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out toDateValue))
            {
                _alert.AddSuccessToastMessage("Invalid date format.");
                return RedirectToAction(nameof(List));
            }

            toDateValue = toDateValue.Date.Add(new TimeSpan(23, 59, 59));
            var excelData = await (from o in _context.tblCloseOrder
                                   join a in _context.tblAccount
                                       on new { o.clientId, o.loginId } equals new { a.clientId, a.loginId }
                                   where o.clientId == clientId && o.closeTime >= fromDateValue
                                    && o.closeTime <= toDateValue
                                   orderby o.closeTime descending // ✅ LIFO: Last orders first
                                   select new
                                   {
                                       OrderNo = o.dealNo,
                                       LoginId = o.loginId,
                                       FirmName = a != null ? a.firmName : string.Empty,
                                       Symbol = o.symbolName,
                                       RateType = o.rateType,
                                       TradeType = o.tradeType == 1 ? "Buy" : o.tradeType == 2 ? "Sell" : o.tradeType == 3 ? "BuyLimit" : "SellLimit",
                                       OpenQuantity = o.volumeOpen,
                                       Quantity = o.volume,
                                       Exchange = o.exchange,
                                       OpenPrice = o.rateOpen,
                                       Price = o.rate,
                                       Total = o.total,
                                       From = o.deviceType,
                                       CloseTime = o.closeTime.ToString("dd-mm-yyyy ") ?? string.Empty,
                                       Time = o.orderTime.ToString("dd-MM-yyyy ") ?? string.Empty,
                                       UpdateTime = o.editorderTime.ToString("dd-MM-yyyy ") ?? string.Empty
                                   }).ToListAsync();
            if (excelData == null || !excelData.Any())
            {
                return RedirectToAction(nameof(List));
            }
            var fileContent = _constatnt.generateExcelFromList(excelData, "CloseOrderData");
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CloseOrder{DateTime.Now}.xlsx");
        }
    }
}

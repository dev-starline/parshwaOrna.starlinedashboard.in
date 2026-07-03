using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using System.Globalization;

namespace SL_Bullion.Controllers
{
    public class DeleteOrderController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        private readonly ApplicationConstant _constatnt;
        private readonly AdminService _adminService;
        public DeleteOrderController(BullionDbContext context, IToastNotification alert, ApplicationConstant constatnt, AdminService adminService)
        {
            _context = context;
            _alert = alert;
            _constatnt = constatnt;
            _adminService = adminService;
        }
        public async Task<IActionResult> List(string tradeFilter = "all")
        {
            var orders = getOrder("", "", DateTime.Now, DateTime.Now, tradeFilter);
            return View(await orders);
        }
        private async Task<List<DeleteOrder>> getOrder(string type, string data, DateTime fromDate, DateTime toDate, string tradeFilter)
        {
            int clientId = HttpContext.Session.GetInt32("clientId") ?? 0;

            var baseQuery = from o in _context.tblDeleteOrder
                            join a in _context.tblAccount
                            on new { o.clientId, o.loginId }
                            equals new { a.clientId, a.loginId }
                where o.clientId == clientId
                select new
                {
                    o,
                    FirmName = a.firmName,
                    Mobile = a.mobile
                };

            if (type == "search")
            {
                if (!string.IsNullOrEmpty(data))
                {
                    if (int.TryParse(data, out var parsedDealNo))
                    {
                        baseQuery = baseQuery.Where(x =>
                            (x.o.loginId.Contains(data) || x.o.dealNo == parsedDealNo) &&
                            x.o.deleteTime >= fromDate && x.o.deleteTime <= toDate);
                    }
                    else
                    {
                        baseQuery = baseQuery.Where(x =>
                            (x.o.loginId.Contains(data) || x.FirmName.Contains(data)) &&
                            x.o.deleteTime >= fromDate && x.o.deleteTime <= toDate);
                    }
                }
                else
                {
                    baseQuery = baseQuery.Where(x => x.o.deleteTime >= fromDate && x.o.deleteTime <= toDate);
                }
            }
            else
            {
                baseQuery = baseQuery.Where(x => x.o.deleteTime.Date == DateTime.Today);
            }

            if (string.Equals(tradeFilter, "buy", StringComparison.OrdinalIgnoreCase))
            {
                baseQuery = baseQuery.Where(x => x.o.tradeType == 1);
            }
            else if (string.Equals(tradeFilter, "sell", StringComparison.OrdinalIgnoreCase))
            {
                baseQuery = baseQuery.Where(x => x.o.tradeType == 2);
            }

            // Final projection (safe now)
            return await baseQuery
                .OrderByDescending(x => x.o.deleteTime)
                .Select(x => new DeleteOrder
                {
                    id = x.o.id,
                    dealNo = x.o.dealNo,
                    loginId = x.o.loginId,
                    firm = x.FirmName,
                    mobile = x.Mobile,
                    symbolName = x.o.symbolName,
                    rateType = x.o.rateType,
                    tradeTypeView = x.o.tradeType == 1 ? "Buy" : x.o.tradeType == 2 ? "Sell" : x.o.tradeType == 3 ? "BuyLimit" : "SellLimit",
                    volume = x.o.volume,
                    exchange = x.o.exchange,
                    rate = x.o.rate,
                    total = x.o.total,
                    deviceType = x.o.deviceType,
                    orderTime = x.o.orderTime,
                    editorderTime = x.o.editorderTime,
                    deleteTime = x.o.deleteTime,
                    ip = x.o.ip,
                    comment = x.o.comment
                }).ToListAsync();
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
            var tradeFilter = Request.Query["tradeFilter"].ToString();
            var orders = getOrder("search", loginId, fromDateValue, toDateValue, tradeFilter);
            return View("List", await orders);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string tradeFilter = "all")
        {
            await _adminService.removeOrder(id, "delete", "open");
            await _context.SaveChangesAsync();
            _alert.AddSuccessToastMessage("order open.");
            return RedirectToAction(nameof(List), new { tradeFilter });
        }
    }
}

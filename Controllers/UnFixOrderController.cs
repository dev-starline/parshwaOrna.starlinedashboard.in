using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using OfficeOpenXml.Packaging.Ionic.Zlib;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using System.Globalization;
using System.Reflection.Metadata;

namespace SL_Bullion.Controllers
{
    public class UnFixOrderController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        private readonly AdminService _adminService;
        public UnFixOrderController(BullionDbContext context, IToastNotification alert, AdminService adminService)
        {
            _context = context;
            _alert = alert;
            _adminService = adminService;
        }
        public async Task<IActionResult> List()
        {
            var orders = getUnFixOrder("", "", DateTime.Now, DateTime.Now);
            return View(await orders);
        }
        private async Task<List<UnFixOrder>> getUnFixOrder(string type, string data, DateTime fromDate, DateTime toDate)
        {
            var query = _context.tblUnFixOrder.Where(o => o.clientId == HttpContext.Session.GetInt32("clientId"));
            if (type == "search")
            {
                if (!string.IsNullOrEmpty(data))
                {
                    if (int.TryParse(data, out var parsedDealNo))
                    {
                        query = query.Where(o =>
                            (o.loginId.Contains(data) || o.dealNo == parsedDealNo) &&
                            (o.editorderTime.Date >= fromDate.Date && o.editorderTime.Date <= toDate.Date));
                    }
                    else
                    {
                        query = query.Where(o =>
                            o.loginId.Contains(data) &&
                            (o.editorderTime.Date >= fromDate.Date && o.editorderTime.Date <= toDate.Date));
                    }
                }
                else
                {
                    query = query.Where(o =>
                        o.editorderTime.Date >= fromDate.Date && o.editorderTime.Date <= toDate.Date);
                }
            }
            var orders = await query.Select(o => new UnFixOrder
            {
                id = o.id,
                dealNo = o.dealNo,
                loginId = o.loginId,
                name = _context.tblAccount.Where(a => a.clientId == o.clientId && a.loginId == o.loginId).Select(a => a.name).FirstOrDefault(),
                firm = _context.tblAccount.Where(a => a.clientId == o.clientId && a.loginId == o.loginId).Select(a => a.firmName).FirstOrDefault(),
                symbolName = o.symbolName,
                rateType = o.rateType,
                tradeType = o.tradeType,
                tradeTypeView = o.tradeType == 1 && o.isLimit == false ? "Buy" : o.tradeType == 1 && o.isLimit == true ? "BuyLimit" : o.tradeType == 2 && o.isLimit == false ? "Sell" : o.tradeType == 2 && o.isLimit == true ? "SellLimit" : o.tradeType == 3 ? "BuyLimit" : o.tradeType == 4 ? "SellLimit" : "Buy",
                volume = o.volume,
                margin = o.margin,
                exchange = o.exchange,
                rate = o.rate,
                total = o.total,
                deviceType = o.deviceType,
                orderTime = o.orderTime,
                editorderTime = o.editorderTime,
                ip = o.ip,
                comment = o.comment
            }).OrderByDescending(o => o.dealNo).ToListAsync();
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
            var orders = getUnFixOrder("search", loginId, fromDateValue, toDateValue);
            return View("List", await orders);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _adminService.removeOrder(id, "unfix", "delete");
            await _context.SaveChangesAsync();
            _alert.AddSuccessToastMessage("order deleted.");
            return RedirectToAction(nameof(List));
        }
    }
}

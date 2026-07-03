using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Elfie.Model;
using Microsoft.CodeAnalysis.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NToastNotify;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using StackExchange.Redis;
using System.Globalization;
using System.Text.Json.Nodes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SL_Bullion.Controllers
{

    public class OpenOrderController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        private readonly ApplicationConstant _constatnt;
        private readonly AdminService _adminService;
        public OpenOrderController(BullionDbContext context, IToastNotification alert, ApplicationConstant constatnt, AdminService adminService)
        {
            _context = context;
            _alert = alert;
            _constatnt = constatnt;
            _adminService = adminService;
        }
        public async Task<IActionResult> List(string tradeFilter = "all")
        {
            var orders = getOpenOrder("", "", DateTime.Now, DateTime.Now, tradeFilter);
            return View(await orders);
        }

        private async Task<List<OpenOrder>> getOpenOrder(string type, string data, DateTime fromDate, DateTime toDate, string tradeFilter)
        {
            var clientId = HttpContext.Session.GetInt32("clientId");

            //var query = _context.tblOpenOrder.Where(o => o.clientId == HttpContext.Session.GetInt32("clientId"));

            var query = from o in _context.tblOpenOrder
                        join a in _context.tblAccount
                        on new { o.clientId, o.loginId } equals new { a.clientId, a.loginId }
                        where o.clientId == clientId
                        select new { o, a };

            if (type == "search")
            {
                if (!string.IsNullOrEmpty(data))
                {
                    if (int.TryParse(data, out var parsedDealNo))
                    {
                        query = query.Where(x =>
                     (x.o.loginId.Contains(data) ||
                      x.o.dealNo == parsedDealNo ||
                      x.a.name.Contains(data) ||
                      x.a.firmName.Contains(data)) &&
                     x.o.editorderTime >= fromDate && x.o.editorderTime <= toDate);
                    }
                    else
                    {
                        query = query.Where(x =>
                    (x.o.loginId.Contains(data) ||
                     x.a.name.Contains(data) ||
                     x.a.firmName.Contains(data)) &&
                    x.o.editorderTime >= fromDate && x.o.editorderTime <= toDate);
                    }
                }
                else
                {
                    query = query.Where(x =>
                x.o.editorderTime >= fromDate && x.o.editorderTime <= toDate);
                }
            }

            if (string.Equals(tradeFilter, "buy", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.o.tradeType == 1);
            }
            else if (string.Equals(tradeFilter, "sell", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.o.tradeType == 2);
            }


            var orders = await query
                .Select(x => new OpenOrder
                {
                    id = x.o.id,
                    dealNo = x.o.dealNo,
                    loginId = x.o.loginId,
                    name = x.a.name,
                    firm = x.a.firmName,
                    mobile = x.a.mobile,
                    symbolName = x.o.symbolName,
                    rateType = x.o.rateType,
                    tradeType = x.o.tradeType,
                    tradeTypeView = x.o.tradeType == 1 && x.o.isLimit == false ? "Buy" : x.o.tradeType == 1 && x.o.isLimit == true ? "BuyLimit" : x.o.tradeType == 2 && x.o.isLimit == false ? "Sell" : x.o.tradeType == 2 && x.o.isLimit == true ? "SellLimit" : x.o.tradeType == 3 ? "BuyLimit" : x.o.tradeType == 4 ? "SellLimit" : "Buy",
                    volume = x.o.volume,
                    margin = x.o.margin,
                    differenceRate = x.o.rate - x.o.exchange,
                    rate = x.o.rate,
                    total = x.o.total,
                    tax = x.o.tax,
                    deviceType = x.o.deviceType,
                    orderTime = x.o.orderTime,
                    editorderTime = x.o.editorderTime,
                    ip = x.o.ip,
                    isHedge = x.o.isHedge,
                    comment = x.o.comment
                }).OrderByDescending(o => o.editorderTime)
                  .ToListAsync();
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
            var tradeFilter = Request.Query["tradeFilter"].ToString();
            var orders = getOpenOrder("search", loginId, fromDateValue, toDateValue, tradeFilter);
            return View("List", await orders);
        }

        [HttpPost]
        public async Task<IActionResult> selectedItems([FromBody] JsonArray obj)
        {
            try
            {
                string type = obj[0]["type"].ToString();
                if (type == "delete")
                {
                    foreach (var item in obj)
                    {
                        await _adminService.removeOrder(Convert.ToInt32(item["id"].ToString()), "open", "delete");
                    }
                }
                else if (type == "close")
                {
                    foreach (var item in obj)
                    {
                        var existingData = await _context.tblOpenOrder.FindAsync(Convert.ToInt32(item["id"].ToString()));
                        var response = await openToClose(Convert.ToInt32(item["id"].ToString()), existingData.volume, existingData.rate, existingData.comment);
                    }

                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }

            return Ok(200);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string tradeFilter = "all")
        {
            await _adminService.removeOrder(id, "open", "delete");
            await _context.SaveChangesAsync();
            _alert.AddSuccessToastMessage("order deleted.");
            return RedirectToAction(nameof(List), new { tradeFilter });
        }
        [HttpPost, ActionName("Open")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenConfirmed(int id, string tradeFilter = "all")
        {
            await _adminService.removeOrder(id, "open", "open");
            await _context.SaveChangesAsync();
            _alert.AddSuccessToastMessage("order open.");
            return RedirectToAction(nameof(List), new { tradeFilter });
        }
        public async Task<IActionResult> CloseOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.tblOpenOrder.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return PartialView("Action", order);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseOrder(int id, [Bind("id,clientId,volume,rate,comment")] OpenOrder order, string tradeFilter = "all")
        {
            if (id != order.id)
            {
                return NotFound();
            }

            try
            {
                var response = await openToClose(id, order.volume, order.rate, order.comment);
                await _context.SaveChangesAsync();
                _alert.AddSuccessToastMessage("Order close done.");
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return RedirectToAction(nameof(List), new { tradeFilter });
        }

        private async Task<int> openToClose(int id, double volume, double rate, string comment)
        {
            int code = 200;
            var existingData = await _context.tblOpenOrder.FindAsync(id);
            CloseOrder closeOrder = new CloseOrder();
            var symbol = _context.tblSymbol.Where(s => s.id == existingData.symbolId).Select(s => new
            {
                division = s.division,
                multiply = s.multiply,
                margin = s.initialMargin,
            }).ToList();
            if (existingData == null)
            {
                code = 400;
                return code;
            }
            closeOrder.clientId = existingData.clientId;
            closeOrder.loginId = existingData.loginId;
            closeOrder.dealNo = existingData.dealNo;
            closeOrder.symbolId = existingData.symbolId;
            closeOrder.symbolName = existingData.symbolName;
            closeOrder.source = existingData.source;
            closeOrder.rateType = existingData.rateType;
            closeOrder.volume = volume;
            closeOrder.volumeOpen = existingData.volume;
            closeOrder.tradeType = existingData.tradeType;
            closeOrder.rate = rate;
            closeOrder.rateOpen = existingData.rate;
            closeOrder.exchange = existingData.exchange;
            closeOrder.total = _constatnt.getTotalRate(existingData.source, rate, symbol[0].multiply, symbol[0].division, volume);
            closeOrder.premium = existingData.premium;
            closeOrder.margin = symbol[0].margin * volume;
            closeOrder.ip = existingData.ip;
            closeOrder.deviceType = existingData.deviceType;
            closeOrder.comment = comment;
            closeOrder.orderTime = existingData.orderTime;
            closeOrder.editorderTime = existingData.editorderTime;
            closeOrder.closeTime = DateTime.Now;
            if (existingData.volume == volume)
            {
                _context.tblCloseOrder.Add(closeOrder);
                _context.tblOpenOrder.Remove(existingData);
            }
            else if (existingData.volume > volume)
            {
                _context.tblCloseOrder.Add(closeOrder);
                existingData.margin = symbol[0].margin * (existingData.volume - volume);
                existingData.total = _constatnt.getTotalRate(existingData.source, existingData.rate, symbol[0].multiply, symbol[0].division, existingData.volume - volume);
                existingData.tax = _constatnt.getTaxRate(existingData.total, existingData.symbolId, existingData.clientId, existingData.loginId);
                existingData.volume = existingData.volume - volume;
                _context.tblOpenOrder.Update(existingData);
            }
            else
            {
                _alert.AddSuccessToastMessage("Volume greater than order volume.");
                code = 400;
                return code;
            }
            return code;
        }

        public async Task<IActionResult> ExportToExcel(string fromDate, string toDate, string tradeFilter = "all")
        {
            int clientId = HttpContext.Session.GetInt32("clientId") ?? 0;
            DateTime fromDateValue;
            DateTime toDateValue;
            string dateFormat = "yyyy-MM-dd";
            if (!DateTime.TryParseExact(fromDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDateValue) ||
                !DateTime.TryParseExact(toDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out toDateValue))
            {
                _alert.AddSuccessToastMessage("Invalid date format.");               
                return RedirectToAction(nameof(List), new { tradeFilter });
            }

            toDateValue = toDateValue.Date.Add(new TimeSpan(23, 59, 59));
            var excelData = await (
                from o in _context.tblOpenOrder
                join a in _context.tblAccount on new { o.clientId, o.loginId } equals new { a.clientId, a.loginId }
                where o.clientId == clientId && o.orderTime >= fromDateValue && o.orderTime <= toDateValue
                && ((o.tradeType == 1) || (o.tradeType == 2))
                orderby o.orderTime descending // ✅ LIFO: Last orders first
                select new
                {
                    OrderNo = o.dealNo,
                    LoginId = o.loginId,
                    FirmName = a.firmName ?? string.Empty,
                    Symbol = o.symbolName,
                    RateType = o.rateType,
                    TradeType = (o.tradeType == 1 && !o.isLimit) ? "Buy" :
                                (o.tradeType == 1 && o.isLimit) ? "BuyLimit" : (o.tradeType == 2 && !o.isLimit) ? "Sell" :
                                (o.tradeType == 2 && o.isLimit) ? "SellLimit" : (o.tradeType == 3) ? "BuyLimit" :
                                (o.tradeType == 4) ? "SellLimit" : "Buy",
                    Quantity = o.volume,
                    Margin = o.margin,
                    Price = o.rate,
                    Total = o.total,
                    Premium = o.premium,
                    TotalTax = o.tax,
                    From = o.deviceType,
                    Time = o.orderTime.ToString("dd-MM-yyyy ") ?? string.Empty,
                    UpdateTime = o.editorderTime.ToString("dd-MM-yyyy ") ?? string.Empty,
                    Comment = o.comment
                }).ToListAsync();

            if (string.Equals(tradeFilter, "buy", StringComparison.OrdinalIgnoreCase))
            {
                excelData = excelData.Where(x => x.TradeType.StartsWith("Buy", StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else if (string.Equals(tradeFilter, "sell", StringComparison.OrdinalIgnoreCase))
            {
                excelData = excelData.Where(x => x.TradeType.StartsWith("Sell", StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (excelData == null || !excelData.Any())
            {
                return RedirectToAction(nameof(List), new { tradeFilter });
            }


            var fileContent = _constatnt.generateExcelFromList(excelData, "OpenOrderData");
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"OpenOrder{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
    }

}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using System.Globalization;

namespace SL_Bullion.Controllers
{
    public class CoinCloseOrderController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        private readonly ApplicationConstant _constatnt;
        public CoinCloseOrderController(BullionDbContext context, IToastNotification alert, ApplicationConstant constatnt)
        {
            _context = context;
            _alert = alert;
            _constatnt = constatnt;
        }
        // GET: CoinCloseOrderController
        public ActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> List()
        {
            int loginId = HttpContext.Request.Query["loginId"].ToString() == "" ? 0 : int.Parse(HttpContext.Request.Query["loginId"].ToString());
            DateTime fromDate = HttpContext.Request.Query["fromDate"].ToString() == "" ? DateTime.Now : DateTime.Parse(HttpContext.Request.Query["fromDate"]);
            DateTime toDate = HttpContext.Request.Query["toDate"].ToString() == "" ? DateTime.Now : DateTime.Parse(HttpContext.Request.Query["toDate"]);
            var orders = await getCloseOrder("", loginId, fromDate, toDate);
            //var orders = await getCloseOrder("", "", DateTime.Now, DateTime.Now);
            return View(orders);
        }

        private async Task<List<CoinCloseOrderListDto>> getCloseOrder(string type, int loginId, DateTime fromDate, DateTime toDate)
        {

            int clientId = Int32.Parse(HttpContext.Session.GetInt32("clientId").ToString());
            var orders = new List<CoinCloseOrderListDto>();
            orders = await GetCloseOrdersAsync(clientId, fromDate, toDate, loginId);
            return orders;
        }

        public async Task<List<CoinCloseOrderListDto>> GetCloseOrdersAsync(
    int clientId, DateTime fromDate, DateTime toDate, int loginId)
        {
            var fromDate1 = fromDate.Date.AddSeconds(1);
            var toDate1 = toDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            var baseQuery = from o in _context.tblCloseOrderCoin
                            join a in _context.tblAccount
                                on new { o.ClientId, o.LoginID } equals new { ClientId = a.clientId, LoginID = a.loginId }
                                into accountGroup
                            from a in accountGroup.DefaultIfEmpty()
                            orderby o.ModifiedDate descending
                            //.Where(o => o.ClientId == clientId
                            //         //&& o.LoginID == loginId
                            //         && o.ModifiedDate >= fromDate1 && o.ModifiedDate <= toDate1)
                            select new CoinCloseOrderListDto
                            {
                                OpenOrderID = o.OpenOrderID,
                                ClientId = o.ClientId,
                                DealNo = o.DealNo,
                                LoginID = o.LoginID.ToString(),
                                UserName = o.UserName,
                                SymbolID = o.SymbolID,
                                SymbolName = o.SymbolName,
                                Source = o.Source,
                                Rate = o.Rate,
                                Exchange = o.Exchange,
                                Total = o.Total,
                                IP = o.IP,
                                Mac = o.Mac,
                                Volume = o.Volume,
                                OpenTradeDateTime = o.OpenTradeDateTime,
                                TradeType = o.TradeType == "1" ? "Buy" : o.TradeType == "2" ? "Sell" : "Buy",
                                TradeFrom = o.TradeFrom,
                                Comment = o.Comment,
                                ModifiedDate = o.ModifiedDate,
                                FirmName = _context.tblAccount
                                    .Where(a => a.clientId == clientId && a.loginId == o.LoginID.ToString())
                                    .Select(a => a.firmName)
                                    .FirstOrDefault() ?? "FirmName",
                                ClosePrice = o.ClosePrice,
                                CloseDateTime = o.CloseDateTime,
                                Name = a.name
                            };
            //.ToListAsync(); // async EF call

            //var result = baseQuery
            //    .GroupBy(o => o.DealNo)
            //    .SelectMany(g => g.OrderByDescending(x => x.CloseDateTime)
            //                      .Select((x, index) => { x.Rank = index + 1; return x; }))
            //    .ToList();
            if (loginId > 0)
            {
                var result = await baseQuery.Where(o => o.ClientId == clientId
                            && (o.LoginID == loginId.ToString() || o.DealNo == loginId)
                            && o.ModifiedDate >= fromDate1
                            && o.ModifiedDate <= toDate1).ToListAsync();
                var ranked = result
                .GroupBy(o => o.DealNo)
                .SelectMany(g => g.Select((o, index) =>
                {
                    o.Rank = index + 1;
                    return o;
                }))
                .ToList();
                return ranked;
            }
            else
            {
                var result = await baseQuery.Where(o => o.ClientId == clientId
                            && o.ModifiedDate >= fromDate1
                            && o.ModifiedDate <= toDate1).ToListAsync();
                var ranked = result
                .GroupBy(o => o.DealNo)
                .SelectMany(g => g.Select((o, index) =>
                {
                    o.Rank = index + 1;
                    return o;
                }))
                .ToList();
                return ranked;
            }
            //return result;
        }
        public async Task<IActionResult> ExportToExcel(string fromDate, string toDate)
        {
            int clientId = HttpContext.Session.GetInt32("clientId") ?? 0;
            int loginId = HttpContext.Request.Query["loginId"].ToString() == "" ? 0 : int.Parse(HttpContext.Request.Query["loginId"].ToString());
            DateTime fromDateValue;
            DateTime toDateValue;
            string dateFormat = "yyyy-MM-dd";
            var ranked = new List<CoinCloseOrderListDto>();
            if (!DateTime.TryParseExact(fromDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDateValue) ||
                !DateTime.TryParseExact(toDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out toDateValue))
            {
                _alert.AddSuccessToastMessage("Invalid date format.");
                return RedirectToAction(nameof(List));
            }

            toDateValue = toDateValue.Date.Add(new TimeSpan(23, 59, 59));
            var excelData = from o in _context.tblCloseOrderCoin
                            join a in _context.tblAccount
                                on new { o.ClientId, o.LoginID } equals new { ClientId = a.clientId, LoginID = a.loginId }
                                into accountGroup
                            from a in accountGroup.DefaultIfEmpty()
                            where o.ClientId == clientId
                            orderby o.ModifiedDate descending
                            //.Where(o => o.ClientId == clientId
                            //         //&& o.LoginID == loginId
                            //         && o.ModifiedDate >= fromDate1 && o.ModifiedDate <= toDate1)
                            select new CoinCloseOrderListDto
                            {
                                OpenOrderID = o.OpenOrderID,
                                ClientId = o.ClientId,
                                DealNo = o.DealNo,
                                LoginID = o.LoginID,
                                UserName = o.UserName,
                                SymbolID = o.SymbolID,
                                SymbolName = o.SymbolName,
                                Source = o.Source,
                                Rate = o.Rate,
                                Exchange = o.Exchange,
                                Total = o.Total,
                                IP = o.IP,
                                Mac = o.Mac,
                                Volume = o.Volume,
                                OpenTradeDateTime = o.OpenTradeDateTime,
                                TradeType = o.TradeType == "1" ? "Buy" : o.TradeType == "2" ? "Sell" : "Buy",
                                TradeFrom = o.TradeFrom,
                                Comment = o.Comment,
                                ModifiedDate = o.ModifiedDate,
                                FirmName = _context.tblAccount
                        .Where(a => a.clientId == clientId && a.loginId == o.LoginID.ToString())
                        .Select(a => a.firmName)
                        .FirstOrDefault() ?? "FirmName",
                                ClosePrice = o.ClosePrice,
                                CloseDateTime = o.CloseDateTime,
                                Name = a.name
                            };
            //Where(o => o.ClientId == clientId);
            //.Where(o => o.ClientId == clientId
            //            && o.ModifiedDate >= fromDateValue
            //            && o.ModifiedDate <= toDateValue).ToListAsync();
            if (loginId > 0)
            {
                var result = await excelData.Where(o =>
                            (o.LoginID == loginId.ToString() || o.DealNo == loginId)
                            && o.ModifiedDate >= fromDateValue
                            && o.ModifiedDate <= toDateValue).ToListAsync();
                ranked = result
               .GroupBy(o => o.DealNo)
               .SelectMany(g => g.Select((o, index) =>
               {
                   o.Rank = index + 1;
                   return o;
               }))
               .ToList();
                //return ranked;
            }
            else
            {
                var result = await excelData.Where(o => o.ClientId == clientId
                            && o.ModifiedDate >= fromDateValue
                            && o.ModifiedDate <= toDateValue).ToListAsync();
                ranked = result
               .GroupBy(o => o.DealNo)
               .SelectMany(g => g.Select((o, index) =>
               {
                   o.Rank = index + 1;
                   return o;
               }))
               .ToList();
                //return ranked;
            }
            if (ranked == null || !ranked.Any())
            {
                return RedirectToAction(nameof(List));
            }
            var fileContent = _constatnt.generateExcelFromList(ranked.Select(o => new
            {
                DealNo = $"{o.Rank.ToString()}-{o.DealNo.ToString()}",
                LoginID = o.LoginID,
                UserName = o.UserName,
                Name = o.Name,
                SymbolName = o.SymbolName,
                Source = o.Source,
                Rate = o.Rate,
                Exchange = o.Exchange,
                Total = o.Total,
                Volume = o.Volume,
                OpenTradeDateTime = o.OpenTradeDateTime,
                TradeType = o.TradeType,
                TradeFrom = o.TradeFrom,
                Comment = o.Comment,
                ModifiedDate = o.ModifiedDate,
                FirmName = o.FirmName,
                ClosePrice = o.ClosePrice,
                CloseDate = o.CloseDateTime
            }).ToList(), "CoinCloseOrderData");
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CoinCloseOrder{DateTime.Now}.xlsx");
        }
        // GET: CoinCloseOrderController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CoinCloseOrderController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CoinCloseOrderController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CoinCloseOrderController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CoinCloseOrderController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CoinCloseOrderController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CoinCloseOrderController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Open(int id, IFormCollection collection)
        {
            try
            {
                var coinCloseOrder = _context.tblCloseOrderCoin.Where(x => x.OpenOrderID == id).FirstOrDefault();
                var openOrderCoin = new OpenOrderCoin
                {
                    //OpenOrderID = coinCloseOrder.OpenOrderID,
                    ClientId = coinCloseOrder.ClientId,
                    DealNo = coinCloseOrder.DealNo,
                    LoginID = coinCloseOrder.LoginID.ToString(),
                    UserName = coinCloseOrder.UserName,
                    SymbolID = coinCloseOrder.SymbolID,
                    SymbolName = coinCloseOrder.SymbolName,
                    Source = coinCloseOrder.Source,
                    Rate = coinCloseOrder.Rate,
                    Exchange = coinCloseOrder.Exchange,
                    IP = coinCloseOrder.IP,
                    Mac = coinCloseOrder.Mac,
                    Volume = coinCloseOrder.Volume,
                    OpenTradeDateTime = coinCloseOrder.OpenTradeDateTime,
                    TradeType = coinCloseOrder.TradeType,
                    TradeFrom = coinCloseOrder.TradeFrom,
                    Comment = coinCloseOrder.Comment,
                    ModifiedDate = DateTime.Now
                };
                _context.tblOpenOrderCoin.Add(openOrderCoin);
                _context.tblCloseOrderCoin.Remove(coinCloseOrder);
                _context.SaveChanges();
                return RedirectToAction(nameof(List));
            }
            catch
            {
                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                var coinCloseOrder = _context.tblCloseOrderCoin.Where(x => x.OpenOrderID == id).FirstOrDefault();
                var openOrderCoin = new OpenOrderCoinHistory
                {
                    OpenOrderID = coinCloseOrder.OpenOrderID,
                    ClientId = coinCloseOrder.ClientId,
                    DealNo = coinCloseOrder.DealNo,
                    LoginID = coinCloseOrder.LoginID.ToString(),
                    UserName = coinCloseOrder.UserName,
                    SymbolID = coinCloseOrder.SymbolID,
                    SymbolName = coinCloseOrder.SymbolName,
                    Source = coinCloseOrder.Source,
                    Rate = coinCloseOrder.Rate,
                    Exchange = coinCloseOrder.Exchange,
                    Total = coinCloseOrder.Total,
                    IP = coinCloseOrder.IP,
                    Mac = coinCloseOrder.Mac,
                    Volume = coinCloseOrder.Volume,
                    OpenTradeDateTime = coinCloseOrder.OpenTradeDateTime,
                    TradeType = coinCloseOrder.TradeType,
                    TradeFrom = coinCloseOrder.TradeFrom,
                    Comment = coinCloseOrder.Comment,
                    ModifiedDate = DateTime.Now,
                    CloseDateTime = coinCloseOrder.CloseDateTime,
                    ClosePrice = coinCloseOrder.ClosePrice,
                    DeleteDate = DateTime.Now
                };
                _context.tblOpenOrderCoinHistory.Add(openOrderCoin);
                _context.tblCloseOrderCoin.Remove(coinCloseOrder);
                _context.SaveChanges();
                return RedirectToAction(nameof(List));
            }
            catch
            {
                return View();
            }
        }
    }
}

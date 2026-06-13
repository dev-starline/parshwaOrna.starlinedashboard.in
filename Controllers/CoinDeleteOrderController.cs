using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using System.Globalization;

namespace SL_Bullion.Controllers
{
    public class CoinDeleteOrderController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        private readonly ApplicationConstant _constatnt;
        public CoinDeleteOrderController(BullionDbContext context, IToastNotification alert, ApplicationConstant constatnt)
        {
            _context = context;
            _alert = alert;
            _constatnt = constatnt;
        }
        public async Task<IActionResult> List()
        {
            int loginId = HttpContext.Request.Query["loginId"].ToString() == "" ? 0 : int.Parse(HttpContext.Request.Query["loginId"].ToString());
            DateTime fromDate = HttpContext.Request.Query["fromDate"].ToString() == "" ? DateTime.Now : DateTime.Parse(HttpContext.Request.Query["fromDate"]);
            DateTime toDate = HttpContext.Request.Query["toDate"].ToString() == "" ? DateTime.Now : DateTime.Parse(HttpContext.Request.Query["toDate"]);
            var orders = await getOpenOrder("", loginId, fromDate, toDate);
            //var orders = await getOpenOrder("", "", DateTime.Now, DateTime.Now);
            return View(orders);
        }

        private async Task<List<OpenOrderCoinHistoryListDto>> getOpenOrder(string type, int loginId, DateTime fromDate, DateTime toDate)
        {
            int clientId = Int32.Parse(HttpContext.Session.GetInt32("clientId").ToString());
            var orders = new List<OpenOrderCoinHistoryListDto>();
            orders = await GetDeleteCoinOrdersAsync(clientId, fromDate, toDate, loginId);
            return orders;
        }
        // GET: CoinOpenOrderController
        public ActionResult Index()
        {
            return View();
        }
        public async Task<List<OpenOrderCoinHistoryListDto>> GetDeleteCoinOrdersAsync(int clientId, DateTime fromDate, DateTime toDate, int loginId)
        {
            //using var context = _contextFactory.CreateDbContext();

            var fromDate1 = fromDate.Date.AddSeconds(1);
            var toDate1 = toDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            var query = from o in _context.tblOpenOrderCoinHistory
                        join a in _context.tblAccount
                            on new { o.ClientId, o.LoginID } equals new { ClientId = a.clientId, LoginID = a.loginId }
                            into accountGroup
                        from a in accountGroup.DefaultIfEmpty()
                            //where o.ClientId == clientId
                            //      && (o.LoginID == loginId.ToString())
                            //      && o.ModifiedDate >= fromDate1
                            //      && o.ModifiedDate <= toDate1
                        orderby o.ModifiedDate descending
                        select new OpenOrderCoinHistoryListDto
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

                            // Format dates AFTER materialization (in memory)
                            OpenTradeDateTime = o.OpenTradeDateTime,
                            TradeType = o.TradeType == "1" ? "Buy" : o.TradeType == "2" ? "Sell" : "Buy",
                            TradeFrom = o.TradeFrom,
                            Comment = o.Comment,
                            ModifiedDate = o.ModifiedDate,

                            // Subquery for FirmName
                            FirmName = _context.tblAccount
                                              .Where(a => a.clientId == clientId)
                                              .Select(a => a.firmName)
                                              .FirstOrDefault() ?? "FirmName",

                            DeleteDate = o.DeleteDate,
                            Name = a.name
                        };
            if (loginId > 0)
            {
                var result = await query.Where(o => o.ClientId == clientId
                            && (o.LoginID == loginId.ToString() || o.DealNo == loginId)
                            && o.DeleteDate >= fromDate1
                            && o.DeleteDate <= toDate1).ToListAsync();
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
                var result = await query.Where(o => o.ClientId == clientId
                            && o.DeleteDate >= fromDate1
                            && o.DeleteDate <= toDate1).ToListAsync();
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
            //return query;
        }

        public async Task<IActionResult> ExportToExcel(string fromDate, string toDate)
        {
            int clientId = HttpContext.Session.GetInt32("clientId") ?? 0;
            int loginId = HttpContext.Request.Query["loginId"].ToString() == "" ? 0 : int.Parse(HttpContext.Request.Query["loginId"].ToString());
            DateTime fromDateValue;
            DateTime toDateValue;
            string dateFormat = "yyyy-MM-dd";
            var ranked = new List<OpenOrderCoinHistoryListDto>();
            if (!DateTime.TryParseExact(fromDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDateValue) ||
                !DateTime.TryParseExact(toDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out toDateValue))
            {
                _alert.AddSuccessToastMessage("Invalid date format.");
                return RedirectToAction(nameof(List));
            }

            toDateValue = toDateValue.Date.Add(new TimeSpan(23, 59, 59));
            var excelData = (from o in _context.tblOpenOrderCoinHistory
                             join a in _context.tblAccount
                                  on new { o.ClientId, o.LoginID } equals new { ClientId = a.clientId, LoginID = a.loginId }
                                  into accountGroup
                             from a in accountGroup.DefaultIfEmpty()
                                 //where o.ClientId == clientId
                                 //      && (o.LoginID == loginId.ToString())
                                 //      && o.ModifiedDate >= fromDate1
                                 //      && o.ModifiedDate <= toDate1
                             orderby o.ModifiedDate descending
                             orderby o.ModifiedDate descending
                             select new OpenOrderCoinHistoryListDto
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

                                 // Subquery for FirmName
                                 FirmName = _context.tblAccount
                                                   .Where(a => a.clientId == clientId)
                                                   .Select(a => a.firmName)
                                                   .FirstOrDefault() ?? "FirmName",

                                 DeleteDate = o.DeleteDate,
                                 Name = a.name
                             }
                            ).Where(o => o.ClientId == clientId);
            //.Where(o => o.ClientId == clientId
            //&& o.DeleteDate >= fromDateValue
            //&& o.DeleteDate <= toDateValue).ToListAsync();
            if (loginId > 0)
            {
                var result = await excelData.Where(o =>
                            (o.LoginID == loginId.ToString() || o.DealNo == loginId)
                            && o.DeleteDate >= fromDateValue
                            && o.DeleteDate <= toDateValue).ToListAsync();
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
                            && o.DeleteDate >= fromDateValue
                            && o.DeleteDate <= toDateValue).ToListAsync();
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
                DeleteDate = o.DeleteDate
            }).ToList(), "CoinDeleteOrderData");
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CoinDeleteOrder{DateTime.Now}.xlsx");
        }
        // GET: CoinDeleteOrderController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CoinDeleteOrderController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CoinDeleteOrderController/Create
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

        // GET: CoinDeleteOrderController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CoinDeleteOrderController/Edit/5
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

        // GET: CoinDeleteOrderController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CoinDeleteOrderController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                var openOrderCoinHistory = _context.tblOpenOrderCoinHistory.Find(id);
                var openOrderCoin = new OpenOrderCoin
                {
                    //OpenOrderID = openOrderCoinHistory.OpenOrderID,
                    ClientId = openOrderCoinHistory.ClientId,
                    DealNo = openOrderCoinHistory.DealNo,
                    LoginID = openOrderCoinHistory.LoginID,
                    UserName = openOrderCoinHistory.UserName,
                    SymbolID = openOrderCoinHistory.SymbolID,
                    SymbolName = openOrderCoinHistory.SymbolName,
                    Source = openOrderCoinHistory.Source,
                    Rate = openOrderCoinHistory.Rate,
                    Exchange = openOrderCoinHistory.Exchange,
                    Total = openOrderCoinHistory.Total,
                    IP = openOrderCoinHistory.IP,
                    Mac = openOrderCoinHistory.Mac,
                    Volume = openOrderCoinHistory.Volume,
                    OpenTradeDateTime = openOrderCoinHistory.OpenTradeDateTime,
                    TradeType = openOrderCoinHistory.TradeType,
                    TradeFrom = openOrderCoinHistory.TradeFrom,
                    Comment = openOrderCoinHistory.Comment,
                    ModifiedDate = DateTime.Now
                };
                _context.tblOpenOrderCoin.Add(openOrderCoin);
                _context.tblOpenOrderCoinHistory.Remove(openOrderCoinHistory);
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using System.Globalization;

namespace SL_Bullion.Controllers
{
    public class CoinOpenOrderController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        private readonly ApplicationConstant _constatnt;
        private readonly MessageConstant _messageConstatnt;
        [BindProperty]
        public List<string> SelectedRowIds { get; set; } = new();

        public CoinOpenOrderController(BullionDbContext context, IToastNotification alert, ApplicationConstant constatnt, MessageConstant messageConstatnt)
        {
            _context = context;
            _alert = alert;
            _constatnt = constatnt;
            _messageConstatnt = messageConstatnt;
        }
        public async Task<IActionResult> List()
        {

            int loginId = HttpContext.Request.Query["loginId"].ToString() == "" ? 0 : int.Parse(HttpContext.Request.Query["loginId"].ToString());
            DateTime fromDate = HttpContext.Request.Query["fromDate"].ToString() == "" ? DateTime.Now : DateTime.Parse(HttpContext.Request.Query["fromDate"]);
            DateTime toDate = HttpContext.Request.Query["toDate"].ToString() == "" ? DateTime.Now : DateTime.Parse(HttpContext.Request.Query["toDate"]);
            bool allRecord = HttpContext.Request.Query["allRecord"].ToString() == "" ? true : bool.Parse(HttpContext.Request.Query["allRecord"].ToString());
            var orders = await getOpenOrder("", loginId, fromDate, toDate, allRecord);
            return View(orders);
        }
        //public async Task<IActionResult> search()
        //{
        //    int loginId = int.Parse(HttpContext.Request.Query["loginId"].ToString());
        //    DateTime fromDate = DateTime.Parse( HttpContext.Request.Query["fromDate"]);
        //    DateTime toDate = DateTime.Parse(HttpContext.Request.Query["toDate"]);


        //    var orders = await getOpenOrder("", loginId, fromDate, toDate);
        //    return View(orders);
        //}
        private async Task<List<OpenOrderCoinDto>> getOpenOrder(string type, int loginId, DateTime fromDate, DateTime toDate, bool allRecord)
        {

            int clientId = Int32.Parse(HttpContext.Session.GetInt32("clientId").ToString());
            var orders = new List<OpenOrderCoinDto>();
            orders = await GetOpenOrdersAsync(clientId, fromDate, toDate, loginId, allRecord);
            return orders;
        }
        // GET: CoinOpenOrderController
        public ActionResult Index()
        {
            return View();
        }
        public async Task<List<OpenOrderCoinDto>> GetOpenOrdersAsync(int clientId, DateTime fromDate, DateTime toDate, int loginId, bool allRecord)
        {
            //using var context = _contextFactory.CreateDbContext();

            var fromDate1 = fromDate.Date.AddSeconds(1);
            var toDate1 = toDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            var query = from o in _context.tblOpenOrderCoin
                        join a in _context.tblAccount
                            on new { o.ClientId, o.LoginID } equals new { ClientId = a.clientId, LoginID = a.loginId }
                            into accountGroup
                        from a in accountGroup.DefaultIfEmpty()
                            //where o.ClientId == clientId
                            //      && (o.LoginID == loginId )
                            //      && o.ModifiedDate >= fromDate1
                            //      && o.ModifiedDate <= toDate1
                        orderby o.ModifiedDate descending
                        select new OpenOrderCoinDto
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
                            Name = a.name
                            // FirmName with fallback
                            //FirmName = a != null ? a.firmName : "FirmName"
                        };
            if (loginId > 0)
            {
                var result = await query.Where(o => o.ClientId == clientId
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
                if (allRecord)
                {
                    var result = await query.Where(o => o.ClientId == clientId).ToListAsync();
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

            }

        }

        public async Task<IActionResult> ExportToExcel(string fromDate, string toDate)
        {
            int clientId = HttpContext.Session.GetInt32("clientId") ?? 0;
            int loginId = HttpContext.Request.Query["loginId"].ToString() == "" ? 0 : int.Parse(HttpContext.Request.Query["loginId"].ToString());
            DateTime fromDateValue;
            DateTime toDateValue;
            string dateFormat = "yyyy-MM-dd";
            var ranked = new List<OpenOrderCoinDto>();
            if (!DateTime.TryParseExact(fromDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDateValue) ||
                !DateTime.TryParseExact(toDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out toDateValue))
            {
                _alert.AddSuccessToastMessage("Invalid date format.");
                return RedirectToAction(nameof(List));
            }

            toDateValue = toDateValue.Date.Add(new TimeSpan(23, 59, 59));
            var excelData = (from o in _context.tblOpenOrderCoin
                             join a in _context.tblAccount
                                 on new { o.ClientId, o.LoginID } equals new { ClientId = a.clientId, LoginID = a.loginId }
                                 into accJoin
                             from a in accJoin.DefaultIfEmpty()
                                 //where o.ClientId == clientId
                                 //      && o.ModifiedDate >= fromDate1
                                 //      && o.ModifiedDate <= toDate1
                             orderby o.ModifiedDate descending
                             select new OpenOrderCoinDto
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
                                 Name = a.name
                             });
            //       .Where(o => o.ClientId == clientId
            //&& o.ModifiedDate >= fromDateValue
            //&& o.ModifiedDate <= toDateValue).ToListAsync();
            if (loginId > 0)
            {
                var result = await excelData.Where(o => o.ClientId == clientId
                            && (o.LoginID == loginId.ToString() || o.DealNo == loginId)
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
                TradeType = o.TradeType == "1" ? "Buy" : o.TradeType == "2" ? "Sell" : "Buy",
                TradeFrom = o.TradeFrom,
                Comment = o.Comment,
                ModifiedDate = o.ModifiedDate
            }).ToList(), "CoinOpenOrderData");
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CoinOpenOrder{DateTime.Now}.xlsx");
        }
        public async Task<IActionResult> CloseOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.tblOpenOrderCoin.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return PartialView("Action", order);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseOrder(int id, [Bind("OpenOrderID,ClientId,LoginID,DealNo,Volume,Rate,Comment,SymbolName")] OpenOrderCoin order)
        {
            if (id != order.OpenOrderID)
            {
                return NotFound();
            }

            try
            {
                var response = await openToClose(id, order.Volume, order.Rate, order.Comment ?? "");
                await _context.SaveChangesAsync();
                _alert.AddSuccessToastMessage("Order close done.");
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return RedirectToAction(nameof(List));
        }
        [HttpPost, ActionName("CloseOrderList")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseOrderList(List<string> ids)
        {

            try
            {
                foreach (var order in ids)
                {
                    var openOrderCoin = _context.tblOpenOrderCoin.Where(x => x.OpenOrderID == int.Parse(order)).FirstOrDefault();
                    var response = await openToClose(openOrderCoin.OpenOrderID, openOrderCoin.Volume, openOrderCoin.Rate, openOrderCoin.Comment);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return RedirectToAction(nameof(List));
        }

        private async Task<int> openToClose(int id, double volume, double rate, string comment)
        {
            int code = 200;
            double OpenVolume = 0, Balance = 0, Margin = 0, InitailMargin = 0, Totalvol = 0;
            Totalvol = _context.tblOpenOrderCoin.Where(x => x.OpenOrderID == id).Select(x => x.Volume).FirstOrDefault();
            OpenVolume = Totalvol - volume;
            if (volume == Totalvol)
            {
                var OpenOrderCoin = _context.tblOpenOrderCoin.Where(x => x.OpenOrderID == id).FirstOrDefault();
                var coinHistory = new CloseOrderCoin()
                {
                    ClientId = OpenOrderCoin.ClientId,
                    DealNo = OpenOrderCoin.DealNo,
                    CloseDateTime = DateTime.Now,
                    ClosePrice = rate,
                    Comment = comment,
                    IP = OpenOrderCoin.IP,
                    LoginID = OpenOrderCoin.LoginID,
                    ModifiedDate = DateTime.Now,
                    OpenTradeDateTime = OpenOrderCoin.OpenTradeDateTime,
                    Rate = OpenOrderCoin.Rate,
                    Source = OpenOrderCoin.Source,
                    SymbolID = OpenOrderCoin.SymbolID,
                    SymbolName = OpenOrderCoin.SymbolName,
                    TradeFrom = OpenOrderCoin.TradeFrom.ToString(),
                    Exchange = OpenOrderCoin.Exchange,
                    UserName = OpenOrderCoin.UserName,
                    Volume = OpenOrderCoin.Volume,
                    TradeType = OpenOrderCoin.TradeType,
                    Mac = OpenOrderCoin.Mac
                };
                _context.tblCloseOrderCoin.Add(coinHistory);
                _context.tblOpenOrderCoin.RemoveRange(OpenOrderCoin);
                await _context.SaveChangesAsync();
            }
            else if (Totalvol > volume)
            {
                var OpenOrderCoin = _context.tblOpenOrderCoin.Where(x => x.OpenOrderID == id).FirstOrDefault();
                var coinHistory = new CloseOrderCoin()
                {
                    ClientId = OpenOrderCoin.ClientId,
                    DealNo = OpenOrderCoin.DealNo,
                    CloseDateTime = DateTime.Now,
                    ClosePrice = rate,
                    Comment = comment,
                    IP = OpenOrderCoin.IP,
                    LoginID = OpenOrderCoin.LoginID,
                    ModifiedDate = DateTime.Now,
                    OpenTradeDateTime = OpenOrderCoin.OpenTradeDateTime,
                    Rate = OpenOrderCoin.Rate,
                    Source = OpenOrderCoin.Source,
                    SymbolID = OpenOrderCoin.SymbolID,
                    SymbolName = OpenOrderCoin.SymbolName,
                    TradeFrom = OpenOrderCoin.TradeType.ToString(),
                    Exchange = OpenOrderCoin.Exchange,
                    UserName = OpenOrderCoin.UserName,
                    Volume = volume,
                    TradeType = OpenOrderCoin.TradeType,
                    Mac = OpenOrderCoin.Mac
                };
                _context.tblCloseOrderCoin.Add(coinHistory);
                //_context.tblOpenOrderCoin.RemoveRange(OpenOrderCoin);
                OpenOrderCoin.Volume = OpenVolume;
                _context.tblOpenOrderCoin.Update(OpenOrderCoin);
                await _context.SaveChangesAsync();

                //_context.tblOpenOrderCoin.Add(new OpenOrderCoin
                //{
                //    ClientId = OpenOrderCoin.ClientId,
                //    DealNo = OpenOrderCoin.DealNo,
                //    OpenTradeDateTime = OpenOrderCoin.OpenTradeDateTime,
                //    LoginID = OpenOrderCoin.LoginID,
                //    UserName = OpenOrderCoin.UserName,
                //    SymbolID = OpenOrderCoin.SymbolID,
                //    SymbolName = OpenOrderCoin.SymbolName,
                //    Source = OpenOrderCoin.Source,
                //    Rate = OpenOrderCoin.Rate,
                //    Exchange = OpenOrderCoin.Exchange,
                //    Total = OpenOrderCoin.Total,
                //    IP = OpenOrderCoin.IP,
                //    Mac = OpenOrderCoin.Mac,
                //    Volume = OpenVolume,
                //    TradeType = OpenOrderCoin.TradeType,
                //    TradeFrom = OpenOrderCoin.TradeFrom,
                //    Comment = OpenOrderCoin.Comment,
                //    ModifiedDate = DateTime.Now
                //});
            }
            //var existingData = await _context.tblOpenOrderCoin.FindAsync(id);
            //CloseOrderCoin closeOrder = new CloseOrderCoin();

            return code;
        }
        // GET: CoinOpenOrderController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CoinOpenOrderController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CoinOpenOrderController/Create
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

        // GET: CoinOpenOrderController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CoinOpenOrderController/Edit/5
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

        // GET: CoinOpenOrderController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }
        [HttpPost, ActionName("DeleteOrderList")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrderList(List<string> ids)
        {

            try
            {
                foreach (var order in ids)
                {
                    Delete(int.Parse(order), null);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return RedirectToAction(nameof(List));
        }
        // POST: CoinOpenOrderController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                var OpenOrderCoin = _context.tblOpenOrderCoin.Where(x => x.OpenOrderID == id).FirstOrDefault();
                var coinHistory = new OpenOrderCoinHistory()
                {
                    ClientId = OpenOrderCoin.ClientId,
                    DealNo = OpenOrderCoin.DealNo,
                    DeleteDate = DateTime.Now,
                    CloseDateTime = null,
                    ClosePrice = null,
                    Comment = OpenOrderCoin.Comment,
                    IP = OpenOrderCoin.IP,
                    LoginID = OpenOrderCoin.LoginID,
                    ModifiedDate = OpenOrderCoin.ModifiedDate,
                    OpenTradeDateTime = OpenOrderCoin.OpenTradeDateTime,
                    Rate = OpenOrderCoin.Rate,
                    Source = OpenOrderCoin.Source,
                    SymbolID = OpenOrderCoin.SymbolID,
                    SymbolName = OpenOrderCoin.SymbolName,
                    TradeFrom = OpenOrderCoin.TradeType.ToString(),
                    Exchange = OpenOrderCoin.Exchange,
                    OpenOrderID = OpenOrderCoin.OpenOrderID,
                    UserName = OpenOrderCoin.UserName,
                    Volume = OpenOrderCoin.Volume,
                    TradeType = OpenOrderCoin.TradeType,
                    Mac = OpenOrderCoin.Mac
                };
                _context.tblOpenOrderCoinHistory.Add(coinHistory);
                _context.tblOpenOrderCoin.RemoveRange(OpenOrderCoin);
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

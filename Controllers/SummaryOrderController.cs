using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using System.Globalization;

namespace SL_Bullion.Controllers
{
    public class SummaryOrderController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        private readonly ApplicationConstant _constatnt;
        private readonly AdminService _adminService;
        public SummaryOrderController(BullionDbContext context, IToastNotification alert, ApplicationConstant constatnt, AdminService adminService)
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

        private async Task<TradeSummaryResponse> getOrder(string type, string data, DateTime fromDate, DateTime toDate)
        {
            var clientId = HttpContext.Session.GetInt32("clientId");

            var allOrders =
                        (from o in _context.tblOpenOrder
                         where o.clientId == clientId && (o.tradeType == 1 || o.tradeType == 2)
                         select new
                         {
                             o.clientId,
                             o.symbolId,
                             o.tradeType,
                             o.volume,
                             o.rate,
                             o.orderTime,
                             o.source,
                             o.exchange
                         })
                        .Concat(
                         from c in _context.tblCloseOrder
                         where c.clientId == clientId && (c.tradeType == 1 || c.tradeType == 2)
                         select new
                         {
                             c.clientId,
                             c.symbolId,
                             c.tradeType,
                             c.volume,
                             c.rate,
                             c.orderTime,
                             c.source,
                             c.exchange
                         });

            #region Symbol-Wise Summary

            var symbolSummaryQuery = from o in allOrders
                                     join s in _context.tblSymbol on o.symbolId equals s.id into symbolJoin
                                     from s in symbolJoin.DefaultIfEmpty()
                                     group new { o, s } by new
                                     {
                                         OrderDate = o.orderTime.Date,
                                         o.symbolId,
                                         SymbolName = s != null ? s.name : "name"
                                     } into g
                                     select new SummaryOrder
                                     {
                                         orderTime = g.Key.OrderDate,
                                         symbolId = g.Key.symbolId,
                                         symbolName = g.Key.SymbolName,

                                         buyCount = g.Count(x => x.o.tradeType == 1),
                                         sellCount = g.Count(x => x.o.tradeType == 2),

                                         buyVolume = g.Where(x => x.o.tradeType == 1).Sum(x => (decimal?)x.o.volume) ?? 0,
                                         sellVolume = g.Where(x => x.o.tradeType == 2).Sum(x => (decimal?)x.o.volume) ?? 0,

                                         buyAvg = (g.Where(x => x.o.tradeType == 1).Sum(x => (double?)x.o.rate * x.o.volume) ?? 0) >
                                                    0 ? (g.Where(x => x.o.tradeType == 1).Sum(x => (double?)x.o.rate * x.o.volume) ?? 0) /
                                                    (g.Where(x => x.o.tradeType == 1).Sum(x => (double?)x.o.volume) ?? 0) : 0,

                                         sellAvg = (g.Where(x => x.o.tradeType == 2).Sum(x => (double?)x.o.rate * x.o.volume) ?? 0) >
                                                    0 ? (g.Where(x => x.o.tradeType == 2).Sum(x => (double?)x.o.rate * x.o.volume) ?? 0) /
                                                    (g.Where(x => x.o.tradeType == 2).Sum(x => (double?)x.o.volume) ?? 0) : 0,

                                         netCount = g.Count(x => x.o.tradeType == 1) - g.Count(x => x.o.tradeType == 2),
                                         netGram = (g.Where(x => x.o.tradeType == 1).Sum(x => (decimal?)x.o.volume) ?? 0)
                                                    - (g.Where(x => x.o.tradeType == 2).Sum(x => (decimal?)x.o.volume) ?? 0)
                                     };

            var symbolSummary = await symbolSummaryQuery.OrderByDescending(x => x.orderTime).ToListAsync();

            #endregion

            #region Metal-Wise Summary

            var metalSummaryQuery = from o in allOrders
                                    group o by o.orderTime.Date into g
                                    select new MetalTradeSummary
                                    {
                                        Date = g.Key,
                                        NoOfTrades = g.Count(),

                                        TotalBuyGold = g.Where(x => x.source.ToLower() == "gold").Sum(x => (decimal?)x.volume) ?? 0,
                                        TotalBuySilver = g.Where(x => x.source.ToLower() == "silver").Sum(x => (decimal?)x.volume) ?? 0,

                                        AverageBuyGold = g.Where(x => x.source.ToLower() == "gold" && x.tradeType == 1).Sum(x => (double?)x.volume) > 0
                                            ? g.Where(x => x.source.ToLower() == "gold" && x.tradeType == 1).Sum(x => (double)x.exchange * (double)x.volume)
                                                / g.Where(x => x.source.ToLower() == "gold" && x.tradeType == 1).Sum(x => (double)x.volume)
                                            : 0,

                                        AverageSellGold = g.Where(x => x.source.ToLower() == "gold" && x.tradeType == 2).Sum(x => (double?)x.volume) > 0
                                            ? g.Where(x => x.source.ToLower() == "gold" && x.tradeType == 2).Sum(x => (double)x.exchange * (double)x.volume)
                                                / g.Where(x => x.source.ToLower() == "gold" && x.tradeType == 2).Sum(x => (double)x.volume)
                                            : 0,

                                        AverageBuySilver = g.Where(x => x.source.ToLower() == "silver" && x.tradeType == 1).Sum(x => (double?)x.volume) > 0
                                            ? g.Where(x => x.source.ToLower() == "silver" && x.tradeType == 1).Sum(x => (double)x.exchange * (double)x.volume)
                                                / g.Where(x => x.source.ToLower() == "silver" && x.tradeType == 1).Sum(x => (double)x.volume)
                                            : 0,

                                        AverageSellSilver = g.Where(x => x.source.ToLower() == "silver" && x.tradeType == 2).Sum(x => (double?)x.volume) > 0
                                            ? g.Where(x => x.source.ToLower() == "silver" && x.tradeType == 2).Sum(x => (double)x.exchange * (double)x.volume)
                                                / g.Where(x => x.source.ToLower() == "silver" && x.tradeType == 2).Sum(x => (double)x.volume)
                                            : 0
                                    };


            var metalSummary = await metalSummaryQuery.OrderByDescending(o => o.Date).ToListAsync();


            #endregion


            #region SummarySymbol-Wise  (No Repeating Symbol Names)

            var SummaryQuery = from o in _context.tblOpenOrder
                               where o.clientId == clientId && (o.tradeType == 1 || o.tradeType == 2)
                               join s in _context.tblSymbol on o.symbolId equals s.id into symbolJoin
                               from s in symbolJoin.DefaultIfEmpty()
                               group new { o, s } by new
                               {
                                   o.symbolId,
                                   SymbolName = s != null ? s.name : "name"
                               } into g
                               select new SummaryOrder
                               {
                                   symbolId = g.Key.symbolId,
                                   symbolName = g.Key.SymbolName,

                                   buyCount = g.Count(x => x.o.tradeType == 1),
                                   sellCount = g.Count(x => x.o.tradeType == 2),

                                   buyVolume = g.Where(x => x.o.tradeType == 1).Sum(x => (decimal?)x.o.volume) ?? 0,
                                   sellVolume = g.Where(x => x.o.tradeType == 2).Sum(x => (decimal?)x.o.volume) ?? 0,

                                   buyAvg = (g.Where(x => x.o.tradeType == 1).Sum(x => (double?)x.o.rate * x.o.volume) ?? 0) >
                                            0 ? (g.Where(x => x.o.tradeType == 1).Sum(x => (double?)x.o.rate * x.o.volume) ?? 0) /
                                            (g.Where(x => x.o.tradeType == 1).Sum(x => (double?)x.o.volume) ?? 0) : 0,
                                   sellAvg = (g.Where(x => x.o.tradeType == 2).Sum(x => (double?)x.o.rate * x.o.volume) ?? 0) >
                                             0 ? (g.Where(x => x.o.tradeType == 2).Sum(x => (double?)x.o.rate * x.o.volume) ?? 0) /
                                             (g.Where(x => x.o.tradeType == 2).Sum(x => (double?)x.o.volume) ?? 0) : 0,

                                   netCount = g.Count(x => x.o.tradeType == 1) - g.Count(x => x.o.tradeType == 2),
                                   netGram = (g.Where(x => x.o.tradeType == 1).Sum(x => (decimal?)x.o.volume) ?? 0)
                                             - (g.Where(x => x.o.tradeType == 2).Sum(x => (decimal?)x.o.volume) ?? 0)
                               };

            var summarySymbol = await SummaryQuery.OrderByDescending(x => x.netGram).ToListAsync();

            #endregion

            // Combine both summaries into a response
            var result = new TradeSummaryResponse
            {
                SymbolSummary = symbolSummary,
                MetalSummary = metalSummary,
                SummarySymbol = summarySymbol
            };
            return result;
        }
    }
}

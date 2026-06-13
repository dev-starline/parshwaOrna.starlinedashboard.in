using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SL_Bullion.DAL;
using SL_Bullion.Models;

namespace SL_Bullion.Controllers
{
    public class CoinSummaryOrderController : Controller
    {
        private readonly BullionDbContext _context;
        public CoinSummaryOrderController(BullionDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> List()
        {
            var orders = await getOpenOrder("", "", DateTime.Now, DateTime.Now);
            return View(orders);
        }

        private async Task<List<SymbolOrderStatsDto>> getOpenOrder(string type, string data, DateTime fromDate, DateTime toDate)
        {
            int clientId = Int32.Parse(HttpContext.Session.GetInt32("clientId").ToString());
            var orders = new List<SymbolOrderStatsDto>();
            orders = await GetCoinSummaryOrdersAsync(clientId, fromDate, toDate, 0);
            return orders;
        }
        // GET: CoinOpenOrderController
        public ActionResult Index()
        {
            return View();
        }
        public async Task<List<SymbolOrderStatsDto>> GetCoinSummaryOrdersAsync(int clientId, DateTime fromDate, DateTime toDate, int loginId)
        {
            var stats = await _context.tblOpenOrderCoin
                            .Where(o => o.ClientId == clientId)
                            .GroupBy(o => o.SymbolID)
                            .Select(g => new SymbolOrderStatsDto
                            {
                                SymbolID = g.Key,
                                SymbolName = _context.tblCoin
                                    .Where(c => c.clientId == clientId && c.id == g.Key)
                                    .Select(c => c.name)
                                    .FirstOrDefault() ?? "0",
                                BuyCount = g.Count(x => x.TradeType == "1"),
                                SellCount = g.Count(x => x.TradeType == "2"),
                                BuyVolume = g.Where(x => x.TradeType == "1").Sum(x => (double?)x.Volume) ?? 0,
                                SellVolume = g.Where(x => x.TradeType == "2").Sum(x => (double?)x.Volume) ?? 0,
                                BuyAvg = g.Where(x => x.TradeType == "1").Any()
                                    ? Math.Round(g.Where(x => x.TradeType == "1").Sum(x => x.Rate * x.Volume) /
                                                  g.Where(x => x.TradeType == "1").Sum(x => x.Volume), 2)
                                    : 0,
                                SellAvg = g.Where(x => x.TradeType == "2").Any()
                                    ? Math.Round(g.Where(x => x.TradeType == "2").Sum(x => x.Rate * x.Volume) /
                                                  g.Where(x => x.TradeType == "2").Sum(x => x.Volume), 2)
                                    : 0,
                                NetCount = g.Count(x => x.TradeType == "1") - g.Count(x => x.TradeType == "2"),
                                NetGrams = (g.Where(x => x.TradeType == "1").Sum(x => (double?)x.Volume) ?? 0)
                                         - (g.Where(x => x.TradeType == "2").Sum(x => (double?)x.Volume) ?? 0)
                            })
                            .ToListAsync();

            return stats;
        }

        // GET: CoinSummaryOrderController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CoinSummaryOrderController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CoinSummaryOrderController/Create
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

        // GET: CoinSummaryOrderController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CoinSummaryOrderController/Edit/5
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

        // GET: CoinSummaryOrderController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CoinSummaryOrderController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
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
    }
}

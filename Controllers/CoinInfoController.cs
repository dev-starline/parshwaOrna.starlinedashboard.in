using Microsoft.AspNetCore.Mvc;
using SL_Bullion.DAL;

namespace SL_Bullion.Controllers
{
    public class CoinInfoController : Controller
    {
        private readonly BullionDbContext _context;

        public CoinInfoController(BullionDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public JsonResult getData()
        {
            var result = _context.tblCoinInfo
                .Where(c => c.ClientId == HttpContext.Session.GetInt32("clientId"))
                .Select(c => new
                {
                    c.Id,
                    user = c.UserName,
                    request = c.Request,
                    response = c.Response,
                    createDate = c.CreateDate
                })
                .OrderByDescending(c => c.Id)
                .ToList();

            return Json(result);
        }
    }
}
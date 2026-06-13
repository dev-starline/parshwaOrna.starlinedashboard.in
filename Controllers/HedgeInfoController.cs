using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using System.Reflection.Metadata;

namespace SL_Bullion.Controllers
{
    public class HedgeInfoController : Controller
    {
        private readonly BullionDbContext _context;
        public HedgeInfoController(BullionDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> List()
        {
            return View();
        }
        public JsonResult getData()
        {
            var result = _context.tblHedgeInfo.Where(h => h.clientId == HttpContext.Session.GetInt32("clientId"))
                .Select(h=>new
                {
                    h.id,
                    user=_context.tblMaster.Where(m => m.id == h.clientId).Select(m => m.userName).FirstOrDefault(),
                    h.request,
                    h.response,
                    h.createDate
                }).ToList();
            return Json(result);
        }
    }
}

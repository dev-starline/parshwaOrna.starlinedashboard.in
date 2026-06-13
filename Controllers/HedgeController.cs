using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using System.Reflection.Metadata;
using System.Text.Json.Nodes;

namespace SL_Bullion.Controllers
{
    public class HedgeController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        public HedgeController(BullionDbContext context, IToastNotification alert)
        {
            _context = context;
            _alert = alert;
        }
        public async Task<IActionResult> List()
        {
            var hedge = _context.tblSymbol.Where(s => !_context.tblHedge.Any(h => h.symbolId == s.id)).Select(s => new Hedge { symbolId = s.id, clientId = s.clientId }).ToList();
            _context.tblHedge.AddRange(hedge);

            var hedgeToRemove = _context.tblHedge.Where(h => !_context.tblSymbol.Any(s => s.id == h.symbolId)).ToList();
            _context.tblHedge.RemoveRange(hedgeToRemove);

            await _context.SaveChangesAsync();

            getHedgeSymbol();

            var result = _context.tblHedge.Where(h => h.clientId == HttpContext.Session.GetInt32("clientId")).Select(h => new Hedge
            {
                id=h.id,
                symbolName = _context.tblSymbol.Where(s => s.id == h.symbolId).Select(a => a.name).FirstOrDefault(),
                hedgeSymbolId = h.hedgeSymbolId,
                division=h.division,
                status=h.status
            }).ToListAsync();

            return View(await result);
        }
        [NonAction]
        private void getHedgeSymbol()
        {
            var hedgeSymbol = _context.tblHedgeSymbol.Where(hs => hs.clientId == HttpContext.Session.GetInt32("clientId").GetValueOrDefault()).Select(hs => new { id = hs.id, name = hs.name }).ToList();
            ViewBag.hedgeSymbol = new SelectList(hedgeSymbol, "id", "name");
        }
        public JsonResult getHedgeDetails()
        {
            var hedge = _context.tblHedgeSymbol.Where(s => s.clientId == HttpContext.Session.GetInt32("clientId")).ToList();
            var contact = _context.tblContact.Where(s => s.clientId == HttpContext.Session.GetInt32("clientId")).Select(c => new
            {
                isHedge = c.isHedge

            }).ToList();
            var obj = new
            {
                hedge = hedge,
                contact = contact
            };
            return Json(obj);
        }
        [HttpPost]
        public async Task<IActionResult> isHedgeUpdate([FromBody] JsonObject obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE tblContact SET isHedge = {bool.Parse(obj["isHedge"].ToString())} WHERE clientId = {HttpContext.Session.GetInt32("clientId")}");
                    _alert.AddSuccessToastMessage("Action change done.");
                }
                catch (DbUpdateConcurrencyException)
                {

                }
            }

            return Ok(200);
        }
        [HttpPost]
        public async Task<IActionResult> saveHedgeSymbol([FromBody] JsonObject obj)
        {
            HedgeSymbol symbol=new HedgeSymbol();
            symbol.clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
            symbol.name = obj["name"].ToString();
            if (TryValidateModel(symbol))
            {
                try
                {
                    var data = _context.Add(symbol);
                    await _context.SaveChangesAsync();
                    var isSuccess = data.Entity.id;
                    if (isSuccess > 0)
                    {
                        _alert.AddSuccessToastMessage("Hedge symbol add.");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    
                }
            }
            else
            {
                var errorValue = ModelState[ModelState.Keys.FirstOrDefault()]?.Errors.FirstOrDefault()?.ErrorMessage;
                _alert.AddWarningToastMessage(errorValue?.ToString());
            }

            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> updateHedge([FromBody] JsonObject obj)
        {
            var hedge = await _context.tblHedge.FindAsync(Convert.ToInt32(obj["id"].ToString()));
            if (hedge == null)
            {
                return NotFound();
            }
            hedge.hedgeSymbolId = Convert.ToInt32(obj["hedgeSymbolID"].ToString());
            hedge.status = (bool)obj["status"];
            hedge.division= Convert.ToInt32(obj["division"].ToString());
            if (TryValidateModel(hedge))
            {
                try
                {
                    var data = _context.Update(hedge);
                    await _context.SaveChangesAsync();
                    var isSuccess = data.Entity.id;
                    if (isSuccess > 0)
                    {
                        _alert.AddSuccessToastMessage("Hedge data Edited.");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    
                }
            }
            else
            {
                var errorValue = ModelState[ModelState.Keys.FirstOrDefault()]?.Errors.FirstOrDefault()?.ErrorMessage;
                _alert.AddWarningToastMessage(errorValue?.ToString());
            }

            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> deleteHedgeSymbol([FromBody] JsonObject obj)
        {
            var symbol = await _context.tblHedgeSymbol.FindAsync(Convert.ToInt32(obj["id"].ToString()));
            if (symbol != null)
            {
                _context.tblHedgeSymbol.Remove(symbol);
                await _context.SaveChangesAsync();
            }
            _alert.AddSuccessToastMessage("Symbol deleted.");

            return Ok();
        }
    }
}

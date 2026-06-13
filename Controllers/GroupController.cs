using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Model;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SL_Bullion.Controllers
{
    public class GroupController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly ApplicationConstant _constatnt;
        private readonly IToastNotification _alert;

        public GroupController(BullionDbContext context, ApplicationConstant constatnt, IToastNotification alert)
        {
            _context = context;
            _constatnt = constatnt;
            _alert = alert;
        }

        public async Task<IActionResult> List()
        {
            return View(await _context.tblGroup.Where(g => g.clientId == HttpContext.Session.GetInt32("clientId")).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,clientId,name,buyPremiumGold,sellPremiumGold,buyPremiumSilver,sellPremiumSilver,isTrade,isEnable")] Group group)
        {
            if (string.IsNullOrWhiteSpace(group.name))
            {
                _alert.AddWarningToastMessage("Group Name is required.");
                return RedirectToAction(nameof(List));
            }

            int clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
            int groupCount = await _context.tblGroup.CountAsync(s => s.clientId == clientId);
            int? groupLimit = await _context.tblMaster.Where(m => m.id == clientId).Select(m => m.group).FirstOrDefaultAsync();

            if (groupLimit.HasValue && groupCount >= groupLimit.Value)
            {
                _alert.AddWarningToastMessage("Group not created due to limit exceeded.");
                return RedirectToAction(nameof(List));
            }

            group.clientId = clientId;
            if (ModelState.IsValid)
            {
                var data = _context.tblGroup.Add(group);
                await _context.SaveChangesAsync();

                _constatnt.pushGroupDetails(data.Entity.id);

                _alert.AddSuccessToastMessage("Group created successfully.");
                return RedirectToAction(nameof(List));
            }

            return View(group);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var group = await _context.tblGroup.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }
            return PartialView("Action", group);
        }
        public JsonResult getGroupSymbol(int id)
        {
            var symbol = _context.tblSymbol.Where(s => !_context.tblGroupSymbol.Any(gs => gs.symbolId == s.id && gs.groupId==id) && s.clientId== HttpContext.Session.GetInt32("clientId").GetValueOrDefault())
                                 .Select(s => new GroupSymbol { groupId = id,symbolId=s.id }).ToList();

            _context.tblGroupSymbol.AddRange(symbol);
            _context.SaveChanges();

            var groupSymbol = _context.tblGroupSymbol.Where(gs => gs.groupId == id).Select(gs => new
            {
                id = gs.id,
                name = _context.tblSymbol.Where(s => s.id == gs.symbolId).Select(s => s.name).FirstOrDefault(),
                isView = gs.isView,
                buyPremium = gs.buyPremium,
                sellPremium = gs.sellPremium,
                oneClick = gs.oneClick,
                inTotal = gs.inTotal,
                step = gs.step
            }).ToList();
            return Json(groupSymbol);
        }

        [HttpPost]
        public async Task<IActionResult> saveAll([FromBody] JsonObject obj)
        {
            if (obj.Count > 0 && obj != null)
            {
                Group group = new Group();
                group.id = Convert.ToInt32(obj["group"]["id"].ToString());
                group.clientId= HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
                group.name = obj["group"]["name"].ToString();
                group.buyPremiumGold = Convert.ToDouble(obj["group"]["buyPremiumGold"].ToString());
                group.sellPremiumGold = Convert.ToDouble(obj["group"]["sellPremiumGold"].ToString());
                group.buyPremiumSilver = Convert.ToDouble(obj["group"]["buyPremiumSilver"].ToString());
                group.sellPremiumSilver = Convert.ToDouble(obj["group"]["sellPremiumSilver"].ToString());
                _context.Update(group);
                await _context.SaveChangesAsync();
                foreach (var item in obj["symbol"].AsArray())
                {
                    var symbol = await _context.tblGroupSymbol.FindAsync(Convert.ToInt32(item["id"].ToString()));
                    symbol.isView =Convert.ToBoolean(item["isView"].ToString());
                    symbol.buyPremium = Convert.ToDouble(item["buyPremium"].ToString());
                    symbol.sellPremium = Convert.ToDouble(item["sellPremium"].ToString());
                    symbol.oneClick = Convert.ToDouble(item["oneClick"].ToString());
                    symbol.inTotal = Convert.ToDouble(item["inTotal"].ToString());
                    symbol.step = Convert.ToDouble(item["step"].ToString());

                    if (ModelState.IsValid)
                    {
                        try
                        {
                            _context.Update(symbol);
                            await _context.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            
                        }
                    }
                }

                _constatnt.pushGroupDetails(group.id);
                _constatnt.pushPendingOrder();
            }
            return Ok();
        }
        //

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.tblAccount.Any(a => a.groupId == id))
            {
                _alert.AddWarningToastMessage("Group assign in account user,Not deletable.");
                return RedirectToAction(nameof(List));
            }
            var group = await _context.tblGroup.FindAsync(id);
            if (group != null)
            {
                _context.tblGroup.Remove(group);
            }

            await _context.SaveChangesAsync();
            _constatnt.pushGroupDetails(id);
            return RedirectToAction(nameof(List));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,clientId,name,buyPremiumGold,sellPremiumGold,buyPremiumSilver,sellPremiumSilver,isTrade,isEnable")] Group @group)
        {
            if (id != @group.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(@group.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(@group);
        }

        // GET: Group/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.tblGroup
                .FirstOrDefaultAsync(m => m.id == id);
            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }

        // POST: Group/Delete/5
        

        private bool GroupExists(int id)
        {
            return _context.tblGroup.Any(e => e.id == id);
        }
    }
}

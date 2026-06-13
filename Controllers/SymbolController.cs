using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SL_Bullion.Controllers
{
    public class SymbolController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        private readonly ApplicationConstant _constatnt;

        public SymbolController(BullionDbContext context, IToastNotification alert, ApplicationConstant constatnt)
        {
            _context = context;
            _alert = alert;
            _constatnt = constatnt;
        }


        public async Task<IActionResult> List()
        {
            var clientId = HttpContext.Session.GetInt32("clientId");    
            var symbols = await _context.tblSymbol.Where(s => s.clientId == clientId).OrderBy(s => s.index).ToListAsync();
            await SetCityViewDataAsync();
            var vm = new SymbolViewModel { Symbols = symbols };
            return View(vm);
        }



        public JsonResult getBankCalculation()
        {
            var bank = _context.tblBankRate.Where(s => s.clientId == HttpContext.Session.GetInt32("clientId")).ToList();
            var contact = _context.tblContact.Where(s => s.clientId == HttpContext.Session.GetInt32("clientId")).Select(c => new
            {
                isRate = c.isRate,
                isLogin = c.isLogin,
                isTrade = c.isTrade

            }).ToList();
            var obj = new
            {
                bank = bank,
                contact = contact
            };
            return Json(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,clientId,name,source,sourceType,isView,rateType,buyPremium,sellPremium,division,multiply,gst,createDate,modifiedDate,changePremiumDate")] Symbol symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol.name))
            {
                _alert.AddWarningToastMessage("Symbol name is required.");
                return RedirectToAction(nameof(List));
            }
            if (ModelState.IsValid)
            {
                int clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
                int symbolCount = await _context.tblSymbol.CountAsync(s => s.clientId == clientId);
                int? symbolLimit = await _context.tblMaster.Where(m => m.id == clientId).Select(m => m.symbol).FirstOrDefaultAsync();
                if (symbolLimit.HasValue && symbolCount >= symbolLimit.Value)
                {
                    _alert.AddWarningToastMessage("Symbol not created due to limit exceeded.");
                    return RedirectToAction(nameof(List));
                }
                symbol.clientId = clientId;
                var data = _context.tblSymbol.Add(symbol);
                await _context.SaveChangesAsync();
                _constatnt.setSymbolRedis();
                var isSuccess = data.Entity.id;
                if (isSuccess > 0)
                {
                    SymbolSession session = new SymbolSession();
                    session.symbolId = data.Entity.id;
                    _context.Add(session);
                    await _context.SaveChangesAsync();
                    _alert.AddSuccessToastMessage("Symbol created.");
                }
                return RedirectToAction(nameof(List));
            }
            return View(symbol);
        }



        public async Task<IActionResult> Edit(int id)
        {

            if (id == null)
            {
                return NotFound();
            }
            var symbol = await _context.tblSymbol.FindAsync(id);
            if (symbol == null) return NotFound();

            await SetCityViewDataAsync();
           return PartialView("Action", symbol);
        }

        private async Task SetCityViewDataAsync()
        {
            var clientId = HttpContext.Session.GetInt32("clientId");
            var master = await _context.tblMaster.FirstOrDefaultAsync(m => m.id == clientId);

            ViewData["IsCityEnabled"] = master?.isCity ?? false;

            ViewData["Cities"] = (master?.isCity ?? false)
                ? await _context.tblCity
                    .Where(c => c.ClientId == clientId)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToListAsync()
                : new List<SelectListItem>();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,clientId,name,source,sourceType,isView,isTerminal,isTrade,rateType,buyPremium,identifier,sellPremium,buyCommonPremium,sellCommonPremium,typeCommonPremium,symbolType,division,multiply,gst,digit,stock,initialMargin,isBill,gstBill,tcsBill,tdsBill,high,low,createDate,modifiedDate,changePremiumDate,CityId")] Symbol symbol)
        {
            if (id != symbol.id)
            {
                return NotFound();
            }

            var existingData = await _context.tblSymbol.FindAsync(id);
            if (existingData == null)
            {
                return NotFound();
            }

            bool wasCorrected = false;
            if (symbol.division <= 0)
            {
                symbol.division = 1; wasCorrected = true;
            }

            if (symbol.multiply <= 0)
            {
                symbol.multiply = 1; wasCorrected = true;
            }

            if (wasCorrected)
            {
                _alert.AddWarningToastMessage("⚠️ Division or Multiply was less than 0. Values were reset to 1.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Entry(existingData).State = EntityState.Detached;
                    symbol.clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
                    symbol.index = existingData.index;

                    _context.Update(symbol);
                    await _context.SaveChangesAsync();

                    _constatnt.setSymbolRedis();
                    _alert.AddSuccessToastMessage("✅ Symbol updated successfully.");
                    return RedirectToAction(nameof(List));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SymbolExists(symbol.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            _alert.AddErrorToastMessage("❌ Validation failed. Please check value .");
            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        public async Task<IActionResult> updatePremium([FromBody] JsonObject obj)
        {
            var symbol = await _context.tblSymbol.FindAsync(Convert.ToInt32(obj["id"].ToString()));
            if (symbol == null)
            {
                return NotFound();
            }
            symbol.isView = obj["isView"].GetValue<bool>();
            symbol.isTerminal = obj["isTerminal"].GetValue<bool>();
            symbol.isTrade = obj["isTrade"].GetValue<bool>();
            symbol.buyPremium = obj["buyPremium"].ToString();
            symbol.name = obj["name"].ToString();
            symbol.sellPremium = obj["sellPremium"].ToString();
            if (TryValidateModel(symbol))
            {
                try
                {
                    var data = _context.Update(symbol);
                    await _context.SaveChangesAsync();
                    var isSuccess = data.Entity.id;
                    if (isSuccess > 0)
                    {
                        _constatnt.setSymbolRedis();
                        _alert.AddSuccessToastMessage("Premium Edited.");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SymbolExists(symbol.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
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
        public async Task<IActionResult> saveAll([FromBody] JsonArray obj)
        {
            if (obj.Count > 0 && obj != null)
            {
                foreach (var item in obj)
                {
                    var symbol = await _context.tblSymbol.FindAsync(Convert.ToInt32(item["id"].ToString())); 
                    symbol.isView = ConvertToBool(item["isView"]);
                    symbol.isTerminal = ConvertToBool(item["isTerminal"]);
                    symbol.isTrade = ConvertToBool(item["isTrade"]);
                    symbol.name = item["name"].ToString();
                    symbol.buyPremium = item["buyPremium"].ToString();
                    symbol.sellPremium = item["sellPremium"].ToString();

                    if (ModelState.IsValid)
                    {
                        try
                        {
                            _context.Update(symbol);
                            await _context.SaveChangesAsync();

                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!SymbolExists(symbol.id))
                            {
                                return NotFound();
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
                _constatnt.setSymbolRedis();
                _alert.AddSuccessToastMessage("Save all done.");
            }
            return Ok();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.tblOpenOrder.Any(o => o.symbolId == id))
            {
                _alert.AddWarningToastMessage("Symbol exist in openorder,Not deletable.");
                return RedirectToAction(nameof(List));
            }
            var symbol = await _context.tblSymbol.FindAsync(id);
            if (symbol != null)
            {
                _context.tblSymbol.Remove(symbol);
                await _context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tblGroupSymbol WHERE symbolId = {id}");
            }

            await _context.SaveChangesAsync();
            _constatnt.setSymbolRedis();
            _alert.AddSuccessToastMessage("Symbol deleted.");
            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        public async Task<IActionResult> changeRateType([FromBody] JsonObject obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE tblSymbol SET rateType = {obj["rateType"].ToString()} WHERE clientId = {HttpContext.Session.GetInt32("clientId")}");
                    _constatnt.setSymbolRedis();
                    _alert.AddSuccessToastMessage("Symbol rate type edited.");
                }
                catch (DbUpdateConcurrencyException)
                {

                }
            }

            return Ok(200);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> updateBank(int id, [Bind("id,clientId,premiumGold,premiumSilver,spotTypeGold,spotTypeSilver,interBankGold,interBankSilver,conversionGold,conversionSilver,customDutyGold,customDutySilver,marginGold,marginSilver,gstGold,gstSilver,divisionGold,divisionSilver,multiplyGold,multiplySilver,modifiedDate")] BankRate bank)
        {
            if (id != bank.id)
            {
                return NotFound();
            }

            bool corrected = false;
            if (bank.divisionGold < 1) { bank.divisionGold = 1; corrected = true; }
            if (bank.divisionSilver < 1) { bank.divisionSilver = 1; corrected = true; }
            if (bank.multiplyGold < 1) { bank.multiplyGold = 1; corrected = true; }
            if (bank.multiplySilver < 1) { bank.multiplySilver = 1; corrected = true; }

            bank.clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();

            if (!clientExists(bank.clientId))
            {
                _context.Add(bank);
            }
            else
            {
                _context.Update(bank);
            }

            if (corrected)
            {
                _alert.AddWarningToastMessage("⚠️ Invalid values were reset to default (1).");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    _constatnt.setBankRateRedis();
                    _alert.AddSuccessToastMessage("Bank calculation updated.");
                    return RedirectToAction(nameof(List));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SymbolExists(bank.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }


            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        public async Task<IActionResult> setCommonPremium([FromBody] JsonObject obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE tblSymbol SET buyCommonPremium = {obj["goldBuyCommonPremium"]?.ToString()},sellCommonPremium = {obj["goldSellCommonPremium"]?.ToString()} WHERE clientId = {HttpContext.Session.GetInt32("clientId")} and source='gold'");
                    await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE tblSymbol SET buyCommonPremium = {obj["silverBuyCommonPremium"]?.ToString()},sellCommonPremium = {obj["silverSellCommonPremium"]?.ToString()} WHERE clientId = {HttpContext.Session.GetInt32("clientId")} and source='silver'");
                    _constatnt.setSymbolRedis();
                    _alert.AddSuccessToastMessage("CommonPremium edited.");
                }
                catch (DbUpdateConcurrencyException)
                {

                }
            }

            return Ok(200);
        }

        [HttpPost]
        public async Task<IActionResult> IsRateUpdate([FromBody] JsonObject obj)
        {
            try
            {
                bool isRate = obj["isRate"]?.ToString() == "true";
                bool isTrade = obj["isTrade"]?.ToString() == "true";
                bool isLogin = obj["isLogin"]?.ToString() == "true";

                int? clientId = HttpContext.Session.GetInt32("clientId");
                if (!clientId.HasValue)
                {
                    return Unauthorized("Client ID not found in session.");
                }

                var contact = await _context.tblContact.FirstOrDefaultAsync(c => c.clientId == clientId);
                if (contact == null)
                {
                    return NotFound("Client contact details not found.");
                }

                bool tradeChanged = contact.isTrade != isTrade;
                contact.isRate = isRate;
                contact.isTrade = isTrade;
                contact.isLogin = isLogin;

                await _context.SaveChangesAsync();
                _constatnt?.pushContactDetails(clientId.Value);

                if (tradeChanged)
                {
                    if (contact.isTrade)
                    {
                        _constatnt?.pushAlert(clientId.Value, "Trade", "Trade Enable", "2");
                    }
                    else
                    {
                        _constatnt?.pushAlert(clientId.Value, "Trade", "Trade Disable", "2");
                    }
                }

                _alert?.AddSuccessToastMessage("Settings updated successfully.");
                return Ok(new { message = "Settings updated successfully." });
            }
            catch (Exception ex)
            {
                _alert?.AddErrorToastMessage($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the settings.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> updateSequance([FromBody] JsonArray obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int index = 1;
                    foreach (var item in obj)
                    {
                        var symbol = await _context.tblSymbol.FindAsync(Convert.ToInt32(item.ToString()));
                        symbol.index = index;
                        _context.Update(symbol);
                        await _context.SaveChangesAsync();
                        index++;
                    }
                    _constatnt.setSymbolRedis();
                    _alert.AddSuccessToastMessage("Sequance edited.");
                }
                catch (Exception ex)
                {
                    _alert.AddErrorToastMessage(ex.Message);
                }
            }

            return Ok(200);
        }

        public JsonResult getSession(int symbolId)
        {
            var session = _context.tblSymbolSession.Where(ss => ss.symbolId == symbolId).FirstOrDefault();
            return Json(session);
        }
        [HttpPost]
        public async Task<IActionResult> saveSessionDetails([FromBody] JsonObject obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool isExist = _context.tblSymbolSession.Any(ss => ss.symbolId == Convert.ToInt32(obj["symbolId"].ToString()));
                    if (isExist)
                    {
                        var symbolSession = _context.tblSymbolSession.FirstOrDefault(ss => ss.symbolId == Convert.ToInt32(obj["symbolId"].ToString()));
                        symbolSession.session = JsonSerializer.Serialize(obj);
                        _context.Update(symbolSession);
                    }
                    else
                    {
                        SymbolSession session = new SymbolSession();
                        session.symbolId = Convert.ToInt32(obj["symbolId"].ToString());
                        session.session = JsonSerializer.Serialize(obj);
                        _context.Add(session);

                    }
                    await _context.SaveChangesAsync();
                    _alert.AddSuccessToastMessage("Session details updated.");
                }
                catch (DbUpdateConcurrencyException)
                {

                }
                return Ok(200);
            }
            return View("List");
        }

        private bool SymbolExists(int id)
        {
            return _context.tblSymbol.Any(e => e.id == id);
        }

        private bool clientExists(int clientId)
        {
            return _context.tblBankRate.Any(e => e.clientId == clientId);
        }

        private bool ConvertToBool(dynamic value)
        {
            if (value == null) return false;
            string str = value.ToString().ToLower();
            return str == "true" || str == "on" || str == "1";
        }
    }

}

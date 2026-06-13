using Azure;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NToastNotify;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using SL_Bullion.WebAPI;
using System;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace SL_Bullion.Controllers
{
    public class TradeController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        private readonly ApplicationConstant _constatnt;
        private readonly ApiService _apiService;
        public TradeController(BullionDbContext context, IToastNotification alert, ApplicationConstant constatnt, ApiService apiService)
        {
            _context = context;
            _alert = alert;
            _constatnt = constatnt;
            _apiService = apiService;
        }
        public IActionResult List()
        {
            getSymbol();
            return View();
        }

        public IActionResult search(string query)
        {
            int clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();

            var listResults = _context.tblAccount
                .Where(a => a.clientId == clientId && !a.isRegister &&
                           (a.loginId.Contains(query) ||
                            a.name.Contains(query) ||
                            a.firmName.Contains(query)))
                .Select(a => new
                {
                    data = new
                    {
                        loginId = a.loginId,
                        name = a.name,
                        firmName = a.firmName,
                        accId = a.id.ToString(),
                        groupId = a.groupId.ToString()
                    }
                })
                .ToList();

            return Ok(listResults);
        }
        //public IActionResult search(string query)
        //{
        //    int clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();

        //    var results = _context.tblAccount.Where(a => a.clientId == clientId && (a.loginId.Contains(query) || a.name.Contains(query) || a.firmName.Contains(query)))
        //    .Select(a => new
        //    {
        //        data = a.loginId + " || " + a.name + " || " + a.firmName
        //    }).ToList();
        //    List<string> listResults = results.Select(r => r.data).ToList();
        //    return Ok(listResults);
        //}
        [NonAction]
        private void getSymbol()
        {
            var symbol = _context.tblSymbol.Where(s => s.clientId == HttpContext.Session.GetInt32("clientId").GetValueOrDefault() && s.isTrade == true).Select(s => new { id = s.id, name = s.name }).ToList();
            ViewBag.symbol = new SelectList(symbol, "id", "name");
        }
        public JsonResult getSymbolVolume(string loginId, int symbolId)
        {
            int clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
            var account = _context.tblAccount.Where(a => a.loginId == loginId && a.clientId == clientId).Select(a => new { a.id, a.groupId }).ToList();
            var volume = _context.tblGroupSymbol.Where(gs => gs.groupId == account[0].groupId && gs.symbolId == symbolId).Select(gs => new
            { oneClick = gs.oneClick, inTotal = gs.inTotal, step = gs.step, accountId = account[0].id, groupId = account[0].groupId }).ToList();
            return Json(volume);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> createTrade(string loginId, int symbolId, double volume, double exchange, double rate, int tradeType, string rateType, int premium, string? comment, string fromDate)
        //{
        //    int clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
        //    loginId = loginId.Split(" || ")[0];
        //    string userName = HttpContext.Session.GetString("userName");
        //    var account = await _context.tblAccount.Where(a => a.loginId == loginId && a.clientId == clientId).Select(a => new { a.id, a.groupId }).FirstOrDefaultAsync();

        //    JsonObject orderDetails = _apiService.verifyOrder(tradeType, userName, symbolId, clientId, loginId, rate, volume);

        //    if (!(Convert.ToDouble(orderDetails?["remainingStock"].ToString()) >= volume))
        //    {
        //        _alert.AddWarningToastMessage("Stock not available on symbol.");
        //        return RedirectToAction(nameof(List));
        //    }
        //    if (!((decimal)orderDetails?["remainingMargin"] >= Convert.ToDecimal(orderDetails?["margin"].ToString())))
        //    {
        //        _alert.AddWarningToastMessage("Margin not available for execute order.");
        //        return RedirectToAction(nameof(List));
        //    }
        //    OrderBase order;

        //    if (rateType == "unFix" || rateType == "rateCut")
        //    {
        //        orderDetails["rate"] = rate;
        //        orderDetails["total"] = _constatnt.getTotalRate((string)orderDetails?["source"], rate, 1, 1, volume);
        //        order = new UnFixOrder();
        //        if (rateType == "rateCut")
        //        {
        //            JsonObject rateCutOrder = await _apiService.verifyRateCutOrder(volume, premium, symbolId, (string)orderDetails?["source"], rate);
        //            order = new OpenOrder();
        //            if (!(bool)rateCutOrder?["isVolume"])
        //            {
        //                _alert.AddSuccessToastMessage("Volume not exist.");
        //                return RedirectToAction(nameof(List));
        //            }
        //            orderDetails["rate"] = (double)rateCutOrder?["rate"];
        //            orderDetails["total"] = (decimal)rateCutOrder?["total"];
        //        }
        //    }
        //    else
        //    {
        //        orderDetails["rate"] = rate;
        //        orderDetails["total"] = _constatnt.getTotalRate((string)orderDetails?["source"], rate, 1, 1, volume);
        //        orderDetails["tax"] = _constatnt.getTaxRate((decimal)orderDetails?["total"], symbolId, clientId, loginId);
        //        order = new OpenOrder();
        //    }
        //    order.clientId = clientId;
        //    order.loginId = loginId;
        //    order.dealNo = _context.tblMaster.Where(m => m.id == clientId).Select(m => m.startDealNo).FirstOrDefault();
        //    order.symbolId = symbolId;
        //    order.symbolName = (string)orderDetails?["symbolName"];
        //    order.source = (string)orderDetails?["source"];
        //    order.rateType = (string)orderDetails?["rateType"];
        //    order.volume = volume;
        //    order.tradeType = tradeType;
        //    order.rate = (double)orderDetails?["rate"];
        //    order.exchange = (double)orderDetails?["exchange"];
        //    order.total = (decimal)orderDetails?["total"];
        //    order.tax = (decimal)orderDetails?["tax"];
        //    order.premium = (double)orderDetails?["premium"];
        //    order.margin = (double)orderDetails?["margin"];
        //    order.ip = HttpContext.Connection.RemoteIpAddress.ToString();
        //    order.deviceType = "admin";
        //    order.comment = comment;
        //    order.orderTime = DateOnly.Parse(fromDate).ToDateTime(TimeOnly.FromDateTime(DateTime.Now));
        //    order.editorderTime = DateOnly.Parse(fromDate).ToDateTime(TimeOnly.FromDateTime(DateTime.Now));
        //    if (ModelState.IsValid)
        //    {
        //        var createOrder = _context.Add(order);
        //        await _context.SaveChangesAsync();
        //        var isSuccess = createOrder.Entity.id;
        //        JsonObject afterOrder = await _apiService.updateOrder(userName, tradeType, symbolId, volume, order.dealNo, (int)orderDetails["accountId"], order.deviceType, order.id);

        //        _alert.AddSuccessToastMessage($"Trade successfully done for {loginId}.");
        //    }
        //    return RedirectToAction(nameof(List));
        //}

        [HttpPost]
        public async Task<IActionResult> createTrade([FromBody] JsonObject obj)
        {
            int clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
            string loginId = obj?["loginId"].ToString().Split(" || ")[0];
            string userName = HttpContext.Session.GetString("userName");
            var account = await _context.tblAccount.Where(a => a.loginId == loginId && a.clientId == clientId).Select(a => new { a.id, a.groupId }).FirstOrDefaultAsync();
            int symbolId = Convert.ToInt32(obj?["symbolId"].ToString());
            var symbol = await _context.tblSymbol.AsNoTracking().Where(a => a.id == symbolId).Select(a => new { a.division, a.multiply }).FirstOrDefaultAsync();
            if (obj?["tradeType"].ToString() == "0")
            {
                _alert.AddWarningToastMessage("Please Select TradeType.");
                return Ok();
            }
            JsonObject orderDetails = _apiService.verifyOrder(Convert.ToInt32(obj?["tradeType"].ToString()), userName, Convert.ToInt32(obj?["symbolId"].ToString()), clientId, loginId, Convert.ToDouble(obj?["rate"].ToString()), Convert.ToDouble(obj?["volume"].ToString()));

            if (!(Convert.ToDouble(orderDetails?["remainingStock"].ToString()) >= Convert.ToDouble(obj?["volume"].ToString())))
            {
                _alert.AddWarningToastMessage("Stock not available on symbol.");
                return Ok();
            }
            if (!((decimal)orderDetails?["remainingMargin"] >= Convert.ToDecimal(orderDetails?["margin"].ToString())))
            {
                _alert.AddWarningToastMessage("Margin not available for execute order.");
                return Ok();
            }
            OrderBase order;

            if (obj?["rateType"].ToString() == "unFix" || obj?["rateType"].ToString() == "rateCut")
            {
                orderDetails["rate"] = Convert.ToDouble(obj?["rate"].ToString());
                orderDetails["total"] = _constatnt.getTotalRate((string)orderDetails?["source"], Convert.ToDouble(obj?["rate"].ToString()), symbol.multiply, symbol.division, Convert.ToDouble(obj?["volume"].ToString()));
                order = new UnFixOrder();
                if (obj?["rateType"].ToString() == "rateCut")
                {
                    JsonObject rateCutOrder = await _apiService.verifyRateCutOrder(Convert.ToDouble(obj?["volume"].ToString()), Convert.ToInt32(obj?["premium"].ToString()), Convert.ToInt32(obj?["symbolId"].ToString()), (string)orderDetails?["source"], Convert.ToDouble(obj?["rate"].ToString()));
                    order = new OpenOrder();
                    if (!(bool)rateCutOrder?["isVolume"])
                    {
                        _alert.AddSuccessToastMessage("Volume not exist.");
                        return RedirectToAction(nameof(List));
                    }
                    orderDetails["rate"] = (double)rateCutOrder?["rate"];
                    orderDetails["total"] = (decimal)rateCutOrder?["total"];
                }
            }
            else
            {
                orderDetails["rate"] = Convert.ToDouble(obj?["rate"].ToString());
                orderDetails["total"] = _constatnt.getTotalRate((string)orderDetails?["source"], Convert.ToDouble(obj?["rate"].ToString()), symbol.multiply, symbol.division, Convert.ToDouble(obj?["volume"].ToString()));
                orderDetails["tax"] = _constatnt.getTaxRate((decimal)orderDetails?["total"], Convert.ToInt32(obj?["symbolId"].ToString()), clientId, loginId);
                order = new OpenOrder();
            }
            order.clientId = clientId;
            order.loginId = loginId;
            order.dealNo = _context.tblMaster.Where(m => m.id == clientId).Select(m => m.startDealNo).FirstOrDefault();
            order.symbolId = Convert.ToInt32(obj?["symbolId"].ToString());
            order.symbolName = (string)orderDetails?["symbolName"];
            order.source = (string)orderDetails?["source"];
            order.rateType = (string)orderDetails?["rateType"];
            order.volume = Convert.ToDouble(obj?["volume"].ToString());
            order.tradeType = Convert.ToInt32(obj?["tradeType"].ToString());
            order.rate = (double)orderDetails?["rate"];
            order.exchange = (double)orderDetails?["exchange"];
            order.total = (decimal)orderDetails?["total"];
            order.tax = (decimal)orderDetails?["tax"];
            order.premium = (double)orderDetails?["premium"];
            order.margin = (double)orderDetails?["margin"];
            order.ip = HttpContext.Connection.RemoteIpAddress.ToString();
            order.deviceType = "admin";
            order.comment = obj?["comment"].ToString();
            order.orderTime = DateOnly.Parse(obj?["fromDate"].ToString()).ToDateTime(TimeOnly.FromDateTime(DateTime.Now));
            order.editorderTime = DateOnly.Parse(obj?["fromDate"].ToString()).ToDateTime(TimeOnly.FromDateTime(DateTime.Now));
            if (ModelState.IsValid)
            {
                var createOrder = _context.Add(order);
                await _context.SaveChangesAsync();
                var isSuccess = createOrder.Entity.id;
                JsonObject afterOrder = await _apiService.updateOrder(userName, Convert.ToInt32(obj?["tradeType"].ToString()), Convert.ToInt32(obj?["symbolId"].ToString()), Convert.ToDouble(obj?["volume"].ToString()), order.dealNo, (int)orderDetails["accountId"], order.deviceType, order.id);

                _alert.AddSuccessToastMessage($"Trade successfully done for {loginId}.");
            }
            return Ok();
        }

        public async Task<IActionResult> fillPremium(string rateType, string loginId)
        {
            int clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
            var fixPremium = _context.tblUnFixOrder.Where(uo => uo.clientId == clientId && uo.loginId == loginId).Select(uo => new
            { value = uo.id, text = uo.rate }).ToList();

            return Json(fixPremium);
        }
    }
}

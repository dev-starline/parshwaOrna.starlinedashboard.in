using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SL_Bullion.Constant;
using SL_Bullion.Controllers;
using SL_Bullion.DAL;
using StackExchange.Redis;
using System.Reflection.Metadata;
using System.Text.Json;
using static SL_Bullion.WebAPI.bullionController;

namespace SL_Bullion.WebAPI
{
    [Route("api/[controller]")]
    [ApiController]
    public class serverController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly ApiService _apiService;
        private readonly AdminService _adminService;
        private readonly ApplicationConstant _constatnt;
        private readonly IConfiguration _configuration;
        private readonly MessageConstant _messageConstatnt;
        ResponseBody _response = new ResponseBody();
        public serverController(BullionDbContext context,ApiService apiService, ApplicationConstant constatnt, IConfiguration configuration, AdminService adminService, MessageConstant messageConstatnt)
        {
            _apiService = apiService;
            _context = context;
            _constatnt = constatnt;
            _configuration = configuration;
            _adminService = adminService;
            _messageConstatnt = messageConstatnt;
        }
        [HttpPost("pendingOrder")]
        public async Task<IActionResult> pendingOrder([FromBody] List<JsonElement> array)
        {
            try
            {
                foreach (var order in array)
                {
                    double exchange = 0, premium = 0, diff = 0;
                    int tradeType = 0;
                    string user = order.GetProperty("user").GetString();
                    var mainProduct = _apiService.getMainProduct(user, order.GetProperty("symbolId").GetInt32());
                    if (order.GetProperty("tradeType").GetInt32() == 3)
                    {
                        tradeType = 1;
                        diff = order.GetProperty("rate").GetDouble() - Convert.ToDouble(mainProduct[0].GetProperty("ask").ToString());
                        exchange = Convert.ToDouble(mainProduct[0].GetProperty("sell").ToString()) + diff;
                        premium = Convert.ToDouble(mainProduct[0].GetProperty("sp").ToString());
                    }
                    else if (order.GetProperty("tradeType").GetInt32() == 4)
                    {
                        tradeType = 2;
                        diff = order.GetProperty("rate").GetDouble() - Convert.ToDouble(mainProduct[0].GetProperty("bid").ToString());
                        exchange = Convert.ToDouble(mainProduct[0].GetProperty("buy").ToString()) + diff;
                        premium = Convert.ToDouble(mainProduct[0].GetProperty("bp").ToString());
                    }
                    var openOrder = await _context.tblOpenOrder.FirstOrDefaultAsync(oo => oo.id == order.GetProperty("id").GetInt32());
                    int accountId = _context.tblAccount.Where(a => a.clientId == openOrder.clientId && a.loginId == openOrder.loginId).Select(a => a.id).FirstOrDefault();
                    if (openOrder != null)
                    {
                        openOrder.exchange = exchange;
                        openOrder.tradeType = tradeType;
                        openOrder.premium = premium;
                        openOrder.editorderTime = DateTime.Now;
                        openOrder.isLimit=true;
                        var result=await _context.SaveChangesAsync();
                        if (result>0)
                        {
                            
                            if (_configuration.GetSection($"hedge:{user}").Exists() && openOrder.deviceType != "admin")
                            {
                                _response = await _apiService.verifyHedgeDetails(user, tradeType, openOrder.volume, openOrder.symbolId, openOrder.dealNo, openOrder.id);
                            }
                            _constatnt.pushOrderAlert("pass", user, accountId, tradeType, openOrder.dealNo);
                            _messageConstatnt.pushMessageAlert("limitPassOrder", user, accountId, openOrder.dealNo);
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpPost("deletePOSchedule")]
        public async Task<IActionResult> deletePOSchedule([FromBody] int clientId)
        {
            try
            {
                var openOrder = await _context.tblOpenOrder.Where(o => o.clientId == clientId && (o.tradeType==3 || o.tradeType==4)).ToListAsync();
                if (openOrder != null)
                {
                    foreach (var item in openOrder)
                    {
                        item.comment = "Order delete by Schedule.";
                        await _adminService.removeOrder(item.id, "open", "delete");
                    }
                    
                    await _context.SaveChangesAsync();
                    _constatnt.pushPendingOrder();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpGet("pushPendingOrder")]
        public async Task<IActionResult> pushPendingOrder()
        {
            try
            {
                _constatnt.pushPendingOrder();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = "An error occurred", error = ex.Message });
            }
        }
    }
}

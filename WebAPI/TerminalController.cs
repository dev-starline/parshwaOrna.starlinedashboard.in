using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using SL_Bullion.Constant;
using SL_Bullion.Controllers;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using StackExchange.Redis;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static SL_Bullion.WebAPI.bullionController;

namespace SL_Bullion.WebAPI
{
    [Route("api/[controller]")]
    [ApiController]
    public class terminalController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly ApplicationConstant _constatnt;
        private readonly ResponseMessage _message;
        private readonly ApiService _apiService;
        private readonly AdminService _adminService;
        private readonly MessageConstant _messageConstatnt;
        ResponseBody _response = new ResponseBody();
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
        private readonly IConfiguration _configuration;

        public terminalController(BullionDbContext context, ApplicationConstant constatnt, ResponseMessage message, ApiService apiService, AdminService adminService, MessageConstant messageConstatnt, IConfiguration configuration)
        {
            _context = context;
            _constatnt = constatnt;
            _message = message;
            _apiService = apiService;
            _adminService = adminService;
            _messageConstatnt = messageConstatnt;
            _configuration = configuration;
        }
        /// <remarks>
        /// Sample request:
        ///
        ///     {"user": "","name": "","mobile": "","firmName":"","email":"","city":"","gst":""}
        ///
        /// </remarks>
        [HttpPost("register")]
        public async Task<IActionResult> register([FromBody] JsonObject obj)
        {
            try
            {
                int clientId = _constatnt.getClientId(obj["user"].ToString());
                var master = await _context.tblMaster.FirstOrDefaultAsync(m => m.id == clientId && m.isActive);
                bool mobileAlreadyExists = await _context.tblAccount.AnyAsync(x => x.mobile == obj["mobile"].ToString() && x.clientId == clientId);

                if (mobileAlreadyExists)
                {
                    _response.code = 400;
                    _response.message = _message.C125;
                    return Json(_response);
                }

                if (master == null)
                {
                    _response.code = 400;
                    _response.message = "Invalid client.";
                    return Json(_response);
                }

                Account account = new Account
                {
                    clientId = clientId,
                    loginId = "",
                    password = "",
                    name = obj["name"].ToString(),
                    mobile = obj["mobile"].ToString(),
                    firmName = obj["firmName"].ToString(),
                    email = obj["email"].ToString(),
                    city = obj["city"].ToString(),
                    gst = obj["gst"].ToString(),
                    isRegister = true,
                    groupId = _context.tblGroup.Where(g => g.clientId == clientId).Select(g => g.id).FirstOrDefault()
                };

                if (ModelState.IsValid)
                {

                    var existingAccount = await _context.tblAccount.FirstOrDefaultAsync(a => a.clientId == clientId && a.mobile == account.mobile);

                    if (master.isEndUserLogin == true)
                    {
                        account.isRegister = false;
                        account.loginId = master.passwordFormat + master.startLoginId;
                        account.password = account.loginId;
                        _context.Add(account);
                        await _context.SaveChangesAsync();
                        await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE tblMaster SET startLoginId = startLoginId+1 WHERE id = {clientId}");
                        _constatnt.pushAccountDetails(account.id, account.groupId);
                        _response.message = $"Register done, Your loginId = {account.loginId} and password = {account.password}";
                        _messageConstatnt.pushMessageAlert("register", obj["user"].ToString(), account.id, clientId);
                    }
                    else
                    {
                        // Simple register flow (no OTP, no EndUserLogin)
                        _context.Add(account);
                        await _context.SaveChangesAsync();
                        _response.message = _message.C109;
                        _messageConstatnt.pushMessageAlert("userRegister", obj["user"].ToString(), account.id, clientId);
                    }
                }
            }
            catch (Exception ex)
            {
                _response.code = 500;
                _response.message = $"Error: {ex.Message}";
            }

            return Json(_response);
        }

        /// <remarks>
        /// Sample request:
        ///
        ///     {"user": "","otp":""}
        ///
        /// </remarks>

        /// --------  //////

        // VerifiedOtp generation logic removed from registration as of 27/09/2025  //

        //[HttpPost("verifiedOtp")]
        //public async Task<IActionResult> verifyOtp([FromBody] JsonObject obj)
        //{
        //    try
        //    {
        //        int clientId = _constatnt.getClientId(obj["user"].ToString());
        //        string otp = obj["otp"]?.ToString();
        //        var account = await _context.tblAccount.FirstOrDefaultAsync(a => a.clientId == clientId && a.otp == otp);

        //        if (account != null)
        //        {
        //            account.isOtp = true;
        //            await _context.SaveChangesAsync();
        //            _response.message = _message.C126;
        //        }
        //        else
        //        {
        //            _response.code = 400;
        //            _response.message = _message.C127;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Better error handling
        //        _response.code = 500;
        //        _response.message = $"Error: {ex.Message}";
        //    }

        //    return Json(_response);
        //}

        /// <remarks>
        /// Sample request:
        ///
        ///     {"user": "","loginId": "","password": "" , "mac": ""}
        ///
        /// </remarks>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] JsonObject obj)
        {
            try
            {
                int clientId = _constatnt.getClientId(obj["user"].ToString());

                if (!_context.tblContact.Any(a => a.clientId == clientId && a.isLogin))
                {
                    _response.code = 400;
                    _response.message = _message.C107;
                    return Json(_response);
                }
                var userName = await _context.tblMaster.Where(_ => _.id == clientId).Select(_ => _.userName).FirstOrDefaultAsync();
                var user = await _context.tblAccount.FirstOrDefaultAsync(c => c.clientId == clientId && c.loginId == obj["loginId"].ToString() && c.password == obj["password"].ToString());
                string mac = obj["mac"]?.ToString();
                if (userName == null)
                {
                    _response.code = 400;
                    _response.message = _message.C138;
                    return Json(_response);
                }

                if (!_context.tblAccount.Any(a => a.clientId == clientId && a.loginId == obj["loginId"].ToString()))
                {
                    _response.code = 400;
                    _response.message = _message.C105;
                    return Json(_response);
                }

                if (user == null)
                {
                    _response.code = 400;
                    _response.message = _message.C108; 
                    return BadRequest(_response);
                }

                if (user.endDate < DateTime.Now.Date)
                {
                    _response.code = 400;
                    _response.message = _message.C110;
                    return Json(_response);
                }

                if (user != null)
                {
                    user.mac = mac;
                    await _context.SaveChangesAsync();
                }

                if (_context.tblAccount.Any(a => a.clientId == clientId && a.loginId == obj["loginId"].ToString() && !a.isRegister && a.isActive && a.password == obj["password"].ToString()))
                {
                    var account = await _context.tblAccount.Where(a => a.clientId == clientId && a.loginId == obj["loginId"].ToString())
                                             .Select(a => new
                                             {
                                                 user = _context.tblMaster.Where(m => m.id == clientId).Select(m => m.userName).FirstOrDefault(),
                                                 name = a.name,
                                                 loginId = a.loginId,
                                                 accountId = a.id,
                                                 groupId = a.groupId,
                                                 margin = a.margin,
                                                 usedMargin = _context.tblOpenOrder.Where(o => o.clientId == clientId && o.loginId == a.loginId).Sum(o => o.margin),
                                                 mac = mac,
                                                 mobile = a.mobile
                                             }).FirstOrDefaultAsync();


                    var token = _constatnt.generateToken(account.user, account.loginId, account.accountId, account.groupId);

                    var singleLoginUsers = _configuration.GetSection("singleLogin").Get<string[]>();
                    if (singleLoginUsers.Contains(obj["user"]?.ToString()))
                    {
                        _constatnt.pushSingleLoginDetails(user.id);
                    }

                    var responseObject = new
                    {
                        name = account.name,
                        token = token.Result,
                        accountId = account.accountId,
                        groupId = account.groupId,
                        margin = account.margin,
                        usedMargin = account.usedMargin,
                        mac = account.mac,
                        mobile = account.mobile,
                        loginId = account.loginId
                    };
                    _response.data = responseObject;
                }
                else
                {
                    _response.code = 400;
                    if (!_context.tblAccount.Any(a => a.clientId == clientId && a.loginId == obj["loginId"].ToString()))
                    {
                        _response.message = _message.C105;
                    }
                    
                    else if (!_context.tblAccount.Any(a => a.clientId == clientId && a.loginId == obj["loginId"].ToString() && !a.isRegister))
                    {
                        _response.message = _message.C106;
                    }
                    else if (!_context.tblAccount.Any(a => a.clientId == clientId && a.loginId == obj["loginId"].ToString() && a.isActive))
                    {
                        _response.message = _message.C107;
                    }
                    else
                    {
                        _response.message = _message.C108;
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }

            return Json(_response);
        }
        /// <remarks>
        /// Sample request:
        ///     [Authorize]
        ///     
        ///     {"oldPassword":"","newPassword:""}
        ///
        /// </remarks>
        [Authorize]
        [HttpPost("changePassword")]
        public async Task<IActionResult> changePassword([FromBody] JsonObject obj)
        {
            try
            {
                int accountId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value);
                if (_context.tblAccount.Any(a => a.id == accountId && a.password == obj["oldPassword"].ToString()))
                {
                    var account = await _context.tblAccount.FirstOrDefaultAsync(a => a.id == accountId);
                    if (account != null)
                    {
                        account.password = obj["newPassword"].ToString();
                        await _context.SaveChangesAsync();
                        _response.message = _message.C121;
                    }
                }
                else
                {
                    _response.code = 400;
                    _response.message = _message.C120;
                }

            }
            catch (Exception)
            {
                throw;
            }

            return Json(_response);
        }

        /// <remarks>
        /// Sample request:
        ///     [Authorize]
        ///
        ///     {"symbolId": "int","tradeType": "int","rate": "double","volume": "double","deviceType": "android:ios:web", "comment": ""}
        ///
        /// </remarks>
        [Authorize]
        [HttpPost("placeOrder")]
        public async Task<IActionResult> placeOrder([FromBody] JsonObject obj)
        {
            try
            {
                _response.code = 400;
                string user = User.Claims.FirstOrDefault(c => c.Type == "user")?.Value;
                int clientId = _constatnt.getClientId(user);
                string loginId = User.Claims.FirstOrDefault(c => c.Type == "loginId")?.Value;
                var comment = obj["comment"]?.ToString();
                JsonObject orderDetails = _apiService.verifyOrder((int)obj["tradeType"], user, (int)obj["symbolId"], clientId, loginId, (double)obj["rate"], (double)obj["volume"]);
                if (!ValidateOrderConditions(orderDetails, obj))
                {
                    return Json(_response);
                }
                OpenOrder order = new OpenOrder();
                order.clientId = clientId;
                order.loginId = loginId;
                order.dealNo = _context.tblMaster.Where(m => m.id == clientId).Select(m => m.startDealNo).FirstOrDefault();
                order.symbolId = (int)obj["symbolId"];
                order.symbolName = (string)orderDetails?["symbolName"];
                order.source = (string)orderDetails?["source"];
                order.rateType = (string)orderDetails?["rateType"];
                order.volume = (double)obj["volume"];
                order.tradeType = (int)obj["tradeType"];
                order.rate = (double)orderDetails?["rate"];
                order.exchange = (double)orderDetails?["exchange"];
                order.total = (decimal)orderDetails?["total"];
                order.tax = (decimal)orderDetails?["tax"];
                order.premium = (double)orderDetails?["premium"];
                order.premiumLimit = (double)orderDetails?["premiumLimit"];
                order.margin = (double)orderDetails?["margin"];
                order.ip = HttpContext.Connection.RemoteIpAddress.ToString();
                order.deviceType = obj["deviceType"].ToString();
                order.comment = string.IsNullOrWhiteSpace(comment) ? null : comment;

                if (ModelState.IsValid)
                {
                    _context.Add(order);
                    await _context.SaveChangesAsync();
                    JsonObject afterOrder = await _apiService.updateOrder(user, (int)obj["tradeType"], (int)obj["symbolId"], (double)obj["volume"], order.dealNo, (int)orderDetails["accountId"], order.deviceType, order.id);

                    if (afterOrder.ContainsKey("code") && (int)afterOrder["code"] == 400)
                    {
                        _response.code = 400;
                        _response.message = afterOrder["message"].ToString();
                    }
                    else
                    {
                        _response.code = 200;
                        _response.message = _message.C112;
                    }
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occurred." + ex.Message);
            }

            return Json(_response);
        }
        private bool ValidateOrderConditions(JsonObject orderDetails, JsonObject obj)
        {
            if (!(bool)orderDetails["isVolumeValid"])
            {
                _response.message = _message.C128;
                return false;
            }
            if (!(bool)orderDetails["isTrade"])
            {
                _response.message = _message.C110;
                return false;
            }
            if (!(bool)orderDetails["isSymbolTrade"])
            {
                _response.message = _message.C111;
                return false;
            }
            if ((bool)orderDetails["offQuotes"])
            {
                _response.message = _message.C118;
                return false;
            }
            if (!(bool)orderDetails["session"])
            {
                _response.message = _message.C119;
                return false;
            }
            if (Convert.ToDouble(orderDetails["remainingStock"].ToString()) < (double)obj["volume"])
            {
                _response.message = _message.C113;
                return false;
            }
            if ((decimal)orderDetails["remainingMargin"] < Convert.ToDecimal(orderDetails["margin"].ToString()))
            {
                _response.message = _message.C114;
                return false;
            }
            if (!(bool)orderDetails["tradeAccess"])
            {
                _response.message = ((int)obj["tradeType"] == 1 || (int)obj["tradeType"] == 3) ? _message.C115 : _message.C116;
                return false;
            }
            if ((bool)orderDetails["freezLevel"])
            {
                _response.message = _message.C117;
                return false;
            }


            return true;
        }
        [Authorize]
        [HttpGet("orderList")]
        public async Task<JsonResult> order(string orderType, string fromDate, string toDate, string? searchData)
        {
            try
            {
                DateTime fromDateValue;
                DateTime toDateValue;
                string dateFormat = "dd/MM/yyyy";
                int clientId = _constatnt.getClientId(User.Claims.FirstOrDefault(c => c.Type == "user")?.Value);
                string loginId = User.Claims.FirstOrDefault(c => c.Type == "loginId")?.Value;
                if (!DateTime.TryParseExact(fromDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDateValue) ||
               !DateTime.TryParseExact(toDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out toDateValue))
                {
                    _response.code = 400;
                    _response.message = _message.C103;
                    return Json(_response);
                }
                toDateValue = toDateValue.Date.Add(new TimeSpan(23, 59, 59));
                var order = await _apiService.getOrder(clientId, loginId, orderType, fromDateValue, toDateValue, searchData);
                if (order != null)
                {
                    if (orderType == "open")
                    {
                        var account = await _context.tblAccount.Where(a => a.clientId == clientId && a.loginId == loginId)
                                             .Select(a => new
                                             {
                                                 margin = a.margin,
                                                 usedMargin = _context.tblOpenOrder.Where(o => o.clientId == clientId && o.loginId == a.loginId).Sum(o => o.margin)
                                             }).FirstOrDefaultAsync();
                        _response.message = JsonSerializer.Serialize(account);
                    }
                    _response.data = order;

                }
                else
                {
                    _response.code = 400;
                    _response.message = _message.C101;
                }

            }
            catch (Exception)
            {
                throw;
            }

            return Json(_response);
        }
        /// <remarks>
        /// Sample request:
        ///
        ///     infoType=profile || trade || symbol
        ///
        /// </remarks>
        [Authorize]
        [HttpGet("terminalInfo")]
        public async Task<JsonResult> terminalInfo(string infoType, int? symbolId)
        {
            try
            {
                int accountId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value);
                string loginId = User.Claims.FirstOrDefault(c => c.Type == "loginId")?.Value;
                dynamic info = null;
                if (loginId != null)
                {
                    if (infoType == "profile")
                    {
                        info = await _apiService.getProfileInfo(accountId);
                    }
                    else if (infoType == "trade")
                    {
                        info = await _apiService.getTradeInfo(symbolId, accountId);
                    }
                    else if (infoType == "symbol")
                    {
                        info = await _apiService.getSymbolInfo(symbolId);
                    }
                }
                _response.data = info;


            }
            catch (Exception)
            {
                throw;
            }

            return Json(_response);
        }
        /// <remarks>
        /// Sample request:
        ///
        ///     id=int,rate=double
        ///
        /// </remarks>
        [Authorize]
        [HttpPut("updatePendingOrder")]
        public async Task<IActionResult> updatePendingOrder([FromBody] JsonObject obj)
        {
            try
            {
                string user = User.Claims.FirstOrDefault(c => c.Type == "user")?.Value;
                int accountId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value);
                var openOrder = await _context.tblOpenOrder.FirstOrDefaultAsync(oo => oo.id == (int)obj["id"] && (oo.tradeType == 3 || oo.tradeType == 4));
                if (openOrder == null)
                {
                    _response.code = 400;
                    _response.message = _message.C122;
                    return Json(_response);
                }
                JsonObject orderDetails = _apiService.verifyOrder(openOrder.tradeType, user, openOrder.symbolId, openOrder.clientId, openOrder.loginId, (double)obj["rate"], openOrder.volume);
                if (!(bool)orderDetails?["isTrade"])
                {
                    _response.message = _message.C110;
                    return Json(_response);
                }
                if (!(bool)orderDetails?["isSymbolTrade"])
                {
                    _response.message = _message.C111;
                    return Json(_response);
                }
                if ((bool)orderDetails?["offQuotes"])
                {
                    _response.message = _message.C118;
                    return Json(_response);
                }
                if (!(bool)orderDetails?["session"])
                {
                    _response.message = _message.C119;
                    return Json(_response);
                }
                //if (!((decimal)orderDetails?["remainingMargin"] >= Convert.ToDecimal(orderDetails?["margin"].ToString())))
                //{
                //    _response.message = _message.C114;
                //    return Json(_response);
                //}
                if (!(bool)orderDetails?["tradeAccess"])
                {
                    if ((int)obj["tradeType"] == 1 || (int)obj["tradeType"] == 3)
                    {
                        _response.message = _message.C115;
                    }
                    else
                    {
                        _response.message = _message.C116;
                    }

                    return Json(_response);
                }
                if ((bool)orderDetails?["freezLevel"])
                {
                    _response.message = _message.C117;
                    return Json(_response);
                }

                if (ModelState.IsValid)
                {
                    openOrder.rate = (double)obj["rate"];
                    openOrder.total = (decimal)orderDetails?["total"];
                    openOrder.tax = (decimal)orderDetails?["tax"];
                    openOrder.editorderTime = DateTime.Now;
                    await _context.SaveChangesAsync();
                    _constatnt.pushPendingOrder();
                    _constatnt.pushOrderAlert("update", user, accountId, openOrder.tradeType, openOrder.dealNo);
                    _response.message = _message.C123;
                }

            }
            catch (Exception)
            {
                throw;
            }

            return Json(_response);
        }
        /// <remarks>
        /// Sample request:
        ///
        ///     id=int
        ///
        /// </remarks>
        [Authorize]
        [HttpDelete("deletePendingOrder")]
        public async Task<IActionResult> deletePendingOrder(int id)
        {
            try
            {
                string user = User.Claims.FirstOrDefault(c => c.Type == "user")?.Value;
                int accountId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value);
                var openOrder = await _context.tblOpenOrder.FindAsync(id);
                if (openOrder != null)
                {
                    openOrder.comment = "Order delete by enduser.";
                    _constatnt.pushOrderAlert("delete", user, accountId, openOrder.tradeType, openOrder.dealNo);
                    await _adminService.removeOrder(id, "open", "delete");
                    await _context.SaveChangesAsync();
                    _constatnt.pushPendingOrder();

                    _response.message = _message.C124;
                }
                else
                {
                    _response.code = 400;
                    _response.message = _message.C122;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return Json(_response);
        }


        [HttpGet("getOpenOrdersbyLoginId")]
        public async Task<JsonResult> getOpenOrdersbyLoginId(string loginId)
        {
            try
            {
                var query = from o in _context.tblOpenOrder
                            where o.clientId == HttpContext.Session.GetInt32("clientId") && (string.IsNullOrEmpty(loginId) || o.loginId == loginId)
                            join a in _context.tblAccount
                                on new { o.clientId, o.loginId } equals new { a.clientId, loginId = a.loginId } into accountGroup
                            from a in accountGroup.DefaultIfEmpty()
                            join g in _context.tblGroup
                                on new { a.clientId, groupId = a.groupId } equals new { g.clientId, groupId = g.id } into groupGroup
                            from g in groupGroup.DefaultIfEmpty()
                            select new OpenOrderGetByLoginId
                            {
                                id = o.id,
                                dealNo = o.dealNo,
                                loginId = o.loginId,
                                name = a != null ? a.name : null,
                                firm = a != null ? a.firmName : null,
                                groupName = g != null ? g.name : null,
                                source = o != null ? o.source : null,
                                symbolName = o.symbolName,
                                rateType = o.rateType,
                                tradeType = o.tradeType,
                                tradeTypeView = o.tradeType == 1 && o.isLimit == false ? "Buy" :
                                                o.tradeType == 1 && o.isLimit == true ? "BuyLimit" :
                                                o.tradeType == 2 && o.isLimit == false ? "Sell" :
                                                o.tradeType == 2 && o.isLimit == true ? "SellLimit" :
                                                o.tradeType == 3 ? "BuyLimit" :
                                                o.tradeType == 4 ? "SellLimit" : "Buy",
                                volume = o.volume,
                                margin = o.margin,
                                exchange = o.exchange,
                                rate = o.rate,
                                differenceRate = o.rate - o.exchange,
                                total = o.total,
                                tax = o.tax,
                                deviceType = o.deviceType,
                                orderTime = o.orderTime,
                                editorderTime = o.editorderTime,
                                ip = o.ip,
                                isHedge = o.isHedge,
                                comment = o.comment,
                                isLimit = o.isLimit
                            };

                var orders = await query.OrderByDescending(o => o.editorderTime).ToListAsync();
                _response.data = orders;
            }
            catch (Exception)
            {
                throw;
            }

            return Json(_response);
        }

        // GET: api/<CoinTradeApiController>
        [Authorize]
        [HttpPost("GetCloseOrderCoinDetails")]
        public async Task<IActionResult> GetCloseOrderCoinDetails(CoinOrderFilterParam Obj)
        {
            try
            {
                if (!ModelState.IsValid)
                {// ❌ Collect error messages
                    var errors = ModelState.Values
                                           .SelectMany(v => v.Errors)
                                           .Select(e => e.ErrorMessage)
                                           .ToList();

                    // ✅ Save to DB or proceed
                    return BadRequest(new { Errors = errors });

                }

                //var ObjUser = JsonConvert.DeserializeObject<dynamic>(Obj);
                var authToken = HttpContext.Request.Headers["Authorization"];
                var ObjUser = await _constatnt.DecodeToken(authToken.ToString().Split(" ")[1]);
                string LoginID = ObjUser.loginId;
                var id = await _context.tblMaster.Where(x => x.userName == ObjUser.user).FirstOrDefaultAsync();
                int ClientID = (id == null) ? 0 : id.id;
                string UserName = ObjUser.user;
                DateTime Fromdate = DateTime.ParseExact(Obj.FromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                DateTime Todate = DateTime.ParseExact(Obj.ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                var fromDate1 = Fromdate.Date.AddSeconds(1);
                var toDate1 = Todate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                var query = await _context.tblCloseOrderCoin
                        .Where(o => o.ClientId == ClientID
                                 && o.LoginID == LoginID.ToString()
                                 && o.ModifiedDate >= fromDate1
                                 && o.ModifiedDate <= toDate1)
                        .OrderByDescending(o => o.ModifiedDate)
                        .GroupBy(o => o.DealNo)
                        .ToListAsync();   // materialize here

                var CloseOrderCoin = query
                    .SelectMany(g => g
                        .OrderByDescending(o => o.LoginID)
                        .Select((o, index) => new CloseOrderDto
                        {
                            OpenOrderID = o.OpenOrderID,
                            ClientId = o.ClientId,
                            DealNo = o.DealNo.ToString(),
                            LoginID = int.Parse(o.LoginID), // safe now
                            UserName = o.UserName,
                            SymbolID = o.SymbolID,
                            SymbolName = o.SymbolName,
                            Source = o.Source,
                            Rate = o.Rate,
                            Exchange = o.Exchange.ToString(),
                            Total = o.Total,
                            IP = o.IP,
                            Mac = o.Mac,
                            Volume = o.Volume,
                            OpenTradeDateTime = o.OpenTradeDateTime,
                            TradeType = o.TradeType,
                            TradeFrom = o.TradeFrom,
                            Comment = o.Comment,
                            ModifiedDate = o.ModifiedDate,
                            FirmName = _context.tblAccount
                                .Where(a => a.clientId == ClientID && a.loginId == o.LoginID.ToString())
                                .Select(a => a.firmName)
                                .FirstOrDefault() ?? "FirmName",
                            ClosePrice = o.ClosePrice,
                            CloseDateTime = o.CloseDateTime,
                            Rank = index + 1
                        })).ToList();

                return Ok(new
                {
                    returnCode = 200,
                    returnMsg = "Success",
                    data = CloseOrderCoin
                });

            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "❌ Unexpected error in GetCoinsOpenOrderDetails");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Error = ex.Message.ToString() });
            }


        }
        [Authorize]
        [HttpPost("GetCoinsOpenOrderDetails")]
        public async Task<IActionResult> GetCoinsOpenOrderDetails(CoinOrderFilterParam Obj)
        {
            try
            {
                if (!ModelState.IsValid)
                {// ❌ Collect error messages
                    var errors = ModelState.Values
                                           .SelectMany(v => v.Errors)
                                           .Select(e => e.ErrorMessage)
                                           .ToList();

                    // ✅ Save to DB or proceed
                    return BadRequest(new { Errors = errors });

                }
                //var ObjUser = JsonConvert.DeserializeObject<dynamic>(Obj);

                var authToken = HttpContext.Request.Headers["Authorization"];
                var ObjUser = await _constatnt.DecodeToken(authToken.ToString().Split(" ")[1]);
                string LoginID = ObjUser.loginId;
                var id = await _context.tblMaster.Where(x => x.userName == ObjUser.user).FirstOrDefaultAsync();
                int ClientID = (id == null) ? 0 : id.id;
                string UserName = ObjUser.user;
                DateTime Fromdate = DateTime.ParseExact(Obj.FromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                DateTime Todate = DateTime.ParseExact(Obj.ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                var fromDate1 = Fromdate.Date.AddSeconds(1);
                var toDate1 = Todate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                //var OpenOrderCoin = await _context.tblCloseOrderCoin
                //                        .Where(m =>
                //                            m.ClientId == ClientID &&
                //                            (m.ModifiedDate >= Fromdate && m.ModifiedDate <= Todate) &&
                //                            m.LoginID == LoginID
                //                         )
                //                        .ToListAsync();
                var OpenOrderCoin = await (
                            from o in _context.tblOpenOrderCoin
                            where o.ClientId == ClientID
                                  && (o.LoginID == LoginID)
                                  && o.ModifiedDate >= fromDate1
                                  && o.ModifiedDate <= toDate1
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
                                OpenTradeDateTime = o.OpenTradeDateTime, // SQL style 113
                                TradeType = o.TradeType,
                                TradeFrom = o.TradeFrom,
                                Comment = o.Comment,
                                ModifiedDate = o.ModifiedDate
                            }
                        ).ToListAsync();
                return Ok(new
                {
                    returnCode = 200,
                    returnMsg = "Success",
                    data = OpenOrderCoin
                });

            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "❌ Unexpected error in GetCoinsOpenOrderDetails");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Error = ex.Message.ToString() });
            }


        }
        [Authorize]
        [HttpPost("GetDeleteOrderCoinDetails")]
        public async Task<IActionResult> GetDeleteOrderCoinDetails(CoinOrderFilterParam Obj)
        {
            try
            {
                if (!ModelState.IsValid)
                {// ❌ Collect error messages
                    var errors = ModelState.Values
                                           .SelectMany(v => v.Errors)
                                           .Select(e => e.ErrorMessage)
                                           .ToList();

                    // ✅ Save to DB or proceed
                    return BadRequest(new { Errors = errors });

                }
                var authToken = HttpContext.Request.Headers["Authorization"];
                var ObjUser = await _constatnt.DecodeToken(authToken.ToString().Split(" ")[1]);
                string LoginID = ObjUser.loginId;
                var id = await _context.tblMaster.Where(x => x.userName == ObjUser.user).FirstOrDefaultAsync();
                int ClientID = (id == null) ? 0 : id.id;
                string UserName = ObjUser.user;
                DateTime Fromdate = DateTime.ParseExact(Obj.FromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                DateTime Todate = DateTime.ParseExact(Obj.ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                var fromDate1 = Fromdate.Date.AddSeconds(1);
                var toDate1 = Todate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                var openOrderHistoryDto = await (
                            from o in _context.tblOpenOrderCoinHistory
                            where o.ClientId == ClientID
                               && (o.LoginID == LoginID.ToString())
                               && o.DeleteDate >= fromDate1
                               && o.DeleteDate <= toDate1
                            orderby o.ModifiedDate descending
                            select new OpenOrderHistoryDto
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
                                TradeType = o.TradeType,
                                TradeFrom = o.TradeFrom,
                                Comment = o.Comment,
                                ModifiedDate = o.ModifiedDate,
                                FirmName = _context.tblAccount
                                    .Where(a => a.clientId == ClientID && a.loginId == o.LoginID.ToString())
                                    .Select(a => a.firmName)
                                    .FirstOrDefault() ?? "FirmName",
                                DeleteDate = o.DeleteDate
                            }
                        ).ToListAsync();
                return Ok(new
                {
                    returnCode = 200,
                    returnMsg = "Success",
                    data = openOrderHistoryDto
                });
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "❌ Unexpected error in GetCoinsOpenOrderDetails");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Error = ex.Message.ToString() });
            }

        }
        // POST api/<CoinTradeApiController>
        [Authorize]
        [HttpPost("InsertCoinOpenOrderDetailWithRegID")]
        public async Task<object> InsertCoinOpenOrderDetailWithRegID([FromBody] TradeModel tradeModel)
        {
            string str = "", ReturnCode = "200", strMsg = "", IP = "", Mac = "", STRMSGTradeType = "", STRMSGRate = "", Title = "";

            try
            {
                IP = HttpContext.Connection.RemoteIpAddress.ToString();
                var authToken = HttpContext.Request.Headers["Authorization"];
                _response.code = 400;
                string user = User.Claims.FirstOrDefault(c => c.Type == "user")?.Value;
                int clientId = _constatnt.getClientId(user);
                string loginId = User.Claims.FirstOrDefault(c => c.Type == "loginId")?.Value;
                //var comment = obj["comment"]?.ToString();
                JsonObject orderDetails = await _apiService.verifyCoinOrder(tradeModel, clientId, loginId, authToken.ToString());
                if (!ValidateCoinOrderConditions(orderDetails))
                {
                    return Json(_response);
                }

                var openOrderCoin = new OpenOrderCoin
                {
                    LoginID = orderDetails["loginid"].ToString(),
                    UserName = orderDetails["UserName"].ToString(),
                    SymbolID = tradeModel.SymbolId,
                    SymbolName = orderDetails["SymbolName"].ToString(),
                    Rate = (double)orderDetails["Rate"],
                    IP = IP,
                    Mac = Mac,
                    Volume = tradeModel.Volume,
                    OpenTradeDateTime = DateTime.Now,
                    TradeType = tradeModel.TradeType,
                    TradeFrom = tradeModel.TradeFrom,
                    Comment = "",
                    DealNo = long.Parse(orderDetails["DealNo"].ToString()),
                    ClientId = (int)orderDetails["ClientId"],
                    Source = orderDetails["Source"].ToString(),
                    Exchange = (double)orderDetails["Exchange"],
                    ModifiedDate = DateTime.Now
                };
                await _context.tblOpenOrderCoin.AddAsync(openOrderCoin);
                await _constatnt.verifyCoinDetails(user, loginId, int.Parse(tradeModel.TradeType), tradeModel.Volume, (int)orderDetails["coinId"], (int)orderDetails["DealNo"]);
                await _context.SaveChangesAsync();
                // EF Core automatically populates the identity column
                int newId = openOrderCoin.OpenOrderID;

                if (newId > 0)
                {
                    await _context.tblMaster
                        .Where(b => b.id == (int)orderDetails["ClientId"])
                        .ExecuteUpdateAsync(setters => setters
                        .SetProperty(b => b.lastCoinDealNo, b => b.lastCoinDealNo + 1));

                }
                if (tradeModel.TradeType == "1")
                {
                    STRMSGTradeType = "Buy";
                    STRMSGRate = orderDetails["Rate"].ToString();
                    Title = "Buy New Order";
                }
                strMsg = "Your trade has been successful.";
                _constatnt.pushCoinOrderAlert("open", orderDetails["UserName"].ToString(), int.Parse(orderDetails["loginid"].ToString()), int.Parse(tradeModel.TradeType), int.Parse(orderDetails["DealNo"].ToString()));
                //object data = new { ReturnCode, ReturnMsg = strMsg, LoginId = orderDetails["loginid"], code = str };
                _response.code = 200;
                _response.message = _message.C112;
                return _response;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private bool ValidateCoinOrderConditions(JsonObject orderDetails)
        {
            // Assume Coinstarttime and Coinendtime are of type TimeSpan
            TimeSpan coinStartTime = TimeSpan.Parse(orderDetails["Coinstarttime"].ToString()); // example
            TimeSpan coinEndTime = TimeSpan.Parse(orderDetails["Coinendtime"].ToString().ToString()); // example

            // Current time of day
            TimeSpan now = DateTime.Now.TimeOfDay;
            if (!(bool)orderDetails["GlobleCoinTradeOn"])
            {
                _response.message = _message.C132;
                return false;
            }
            if (!(bool)orderDetails["Status"])
            {
                _response.message = _message.C133;
                return false;
            }
            if (!(bool)orderDetails["IsCoinStock"])
            {
                _response.message = _message.C137;
                return false;
            }
            if (((double)orderDetails["Volume"] > (double)orderDetails["RemainingVolume"]))
            {
                _response.message = _message.C134;
                return false;
            }
            if (!(now >= coinStartTime && now <= coinEndTime))
            {
                _response.message = _message.C135;
                return false;
            }

            return true;
        }
    }

    internal class OpenOrderGetByLoginId
    {
        public int id { get; set; }
        public int dealNo { get; set; }
        public string loginId { get; set; }
        public string name { get; set; }
        public string firm { get; set; }
        public string source { get; set; }
        public string groupName { get; set; }
        public string symbolName { get; set; }
        public string rateType { get; set; }
        public int tradeType { get; set; }
        public string tradeTypeView { get; set; }
        public double volume { get; set; }
        public double margin { get; set; }
        public double exchange { get; set; }
        public double rate { get; set; }
        public double differenceRate { get; set; }
        public decimal total { get; set; }
        public decimal tax { get; set; }
        public string deviceType { get; set; }
        public DateTime orderTime { get; set; }
        public DateTime editorderTime { get; set; }
        public string ip { get; set; }
        public bool isHedge { get; set; }
        public string comment { get; set; }
        public bool isLimit { get; set; }
    }
}

using Microsoft.Data.SqlClient.DataClassification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SL_Bullion.Constant
{
    public class MessageConstant
    {
        string smsNodeUrl = "";
        private readonly BullionDbContext _context;
        private readonly IConfiguration _config;
        private readonly ApplicationConstant _constatnt;
        private static readonly HttpClient client = new HttpClient();
        public MessageConstant(BullionDbContext context, IConfiguration config, ApplicationConstant constatnt)
        {
            _context = context;
            _config = config;
            smsNodeUrl = config.GetSection("smsNodeUrl").Value;
            _constatnt = constatnt;
        }
        internal void pushMessageAlert(string type, string user, int accountId, int typeId)
        {
            try
            {
                if (_config.GetSection("message").Get<string[]>().Contains($"{user}"))
                {
                    var result = new object();
                    switch (type)
                    {
                        case "executeOrder":
                        case "pendingOrder":
                            result = getOpenOrderInfo(type, user, accountId, typeId);
                            break;
                        case "userRegister":
                            result = getregister(type, user, accountId, typeId);
                            break;
                        case "closeOrder":
                            result = getCloseOrderInfo(type, user, accountId, typeId);
                            break;
                        case "userApproove":
                            result = getuserApproove(type, user, accountId, typeId);
                            break;
                        case "limitPlaceOrder":
                            result = getOpenOrderInfo(type, user, accountId, typeId);
                            break;
                        case "limitPassOrder":
                            result = getOpenOrderInfo(type, user, accountId, typeId);
                            break;
                        case "sendlogin":
                            result = getuserApproove(type, user, accountId, typeId);
                            break;
                        case "orderHedge":
                        case "orderHedgeError":
                            result = getOrderHedgeInfo(type, user, accountId, typeId);
                            break;

                        default:
                            break;
                    }
                    string jsonString = JsonSerializer.Serialize(result);
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var response = client.PostAsync(smsNodeUrl + "/smsDetails", content);
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        private object getuserApproove(string type, string user, int accountId, int dealNo)
        {
            int clientId = _constatnt.getClientId(user);
            var result = _context.tblAccount.Where(oo => oo.clientId == dealNo && oo.id == accountId)
                                .Select(oo => new
                                {
                                    name = oo.name,
                                    type = type,
                                    user = user,
                                    mobile = oo.mobile,
                                    loginId = oo.loginId,
                                    password = oo.password,
                                    accountData = _context.tblAccount.Where(a => a.id == accountId).Select(a => new { a.mobile, a.name, a.firmName, a.loginId }).FirstOrDefault(),
                                    userData = _context.tblMaster.Where(m => m.userName == user).Select(m => new { m.mobile, m.firmName }).FirstOrDefault()
                                }).SingleOrDefault<object>();
            return result;
        }

        private object getregister(string type, string user, int accountId, int dealNo)
        {
            int clientId = _constatnt.getClientId(user);
            var result = _context.tblAccount.Where(oo => oo.clientId == clientId && oo.id == accountId)
                                .Select(oo => new
                                {
                                    name = oo.name,
                                    type = type,
                                    user = user,
                                    mobile = oo.mobile,
                                    accountData = _context.tblAccount.Where(a => a.id == accountId).Select(a => new { a.mobile, a.name, a.firmName, a.loginId }).FirstOrDefault(),
                                    userData = _context.tblMaster.Where(m => m.userName == user).Select(m => new { m.mobile, m.firmName }).FirstOrDefault()
                                }).SingleOrDefault<object>();
            return result;
        }


        private object getOpenOrderInfo(string type, string user, int accountId, int dealNo)
        {
            int clientId = _constatnt.getClientId(user);
            var result = _context.tblOpenOrder.Where(oo => oo.clientId == clientId && oo.dealNo == dealNo)
                                .Select(oo => new
                                {
                                    orderNo = oo.dealNo,
                                    symbolName = oo.symbolName,
                                    source = oo.source,
                                    volume = oo.volume,
                                    tradeTypeView = oo.tradeType == 1 && oo.isLimit == false ? "Buy" : oo.tradeType == 1 && oo.isLimit == true ? "BuyLimit" : oo.tradeType == 2 && oo.isLimit == false ? "Sell" : oo.tradeType == 2 && oo.isLimit == true ? "SellLimit" : oo.tradeType == 3 ? "BuyLimit" : oo.tradeType == 4 ? "SellLimit" : "Buy",
                                    rate = oo.rate,
                                    total = oo.total,
                                    tax = oo.tax,
                                    ip = oo.ip,
                                    orderTime = oo.orderTime,
                                    type = type,
                                    user = user,
                                    accountData = _context.tblAccount.Where(a => a.id == accountId).Select(a => new { a.mobile, a.name, a.firmName, a.loginId }).FirstOrDefault(),
                                    userData = _context.tblMaster.Where(m => m.userName == user).Select(m => new { m.mobile, m.firmName }).FirstOrDefault()
                                }).SingleOrDefault<object>();
            return result;
        }

        private object getCloseOrderInfo(string type, string user, int accountId, int closeOrderId)
        {
            int clientId = _constatnt.getClientId(user);
            var result = _context.tblCloseOrder.Where(a => a.id == closeOrderId)
                                .Select(c => new
                                {
                                    orderNo = c.dealNo,
                                    symbolName = c.symbolName,
                                    source = c.source,
                                    volume = c.volume,
                                    tradeTypeView = c.tradeType == 1 ? "Buy" : c.tradeType == 2 ? "Sell" : "Buy",
                                    orderTime = c.closeTime,
                                    type = type,
                                    user = user,
                                    accountData = _context.tblAccount.Where(a => a.id == accountId).Select(a => new { a.mobile, a.name, a.firmName, a.loginId }).FirstOrDefault(),
                                    userData = _context.tblMaster.Where(m => m.userName == user).Select(m => new { m.mobile, m.firmName }).FirstOrDefault()
                                }).SingleOrDefault<object>();
            return result;
        }
        private object getOrderHedgeInfo(string type, string user, int accountId, int orderNo)
        {
            int clientId = _constatnt.getClientId(user);
            var result = _context.tblOpenOrder.Where(oo => oo.clientId == clientId && oo.dealNo == orderNo)
                                .Select(oo => new
                                {
                                    hedgeSymbol = _context.tblHedgeSymbol.Where(hs => hs.id == _context.tblHedge.Where(h => h.symbolId == oo.symbolId).Select(h => h.hedgeSymbolId).FirstOrDefault()).Select(hs => hs.name).FirstOrDefault(),
                                    volume = oo.volume,
                                    symbolName = oo.symbolName,
                                    rate = oo.exchange,
                                    diff = (oo.rate - oo.exchange),
                                    type = type,
                                    user = user,
                                    time = DateTime.Now,
                                    accountData = _context.tblAccount.Where(a => a.id == accountId).Select(a => new { a.mobile, a.name, a.firmName, a.loginId }).FirstOrDefault(),
                                    userData = _context.tblMaster.Where(m => m.userName == user).Select(m => new { m.mobile, m.firmName }).FirstOrDefault()
                                }).SingleOrDefault<object>();
            return result;
        }

        internal void pushMessageAlertOtr(string type, string user, int clientId, string mobile)
        {
            try
            {
                if (_config.GetSection("message").Get<string[]>().Contains($"{user}"))
                {
                    var result = new object();
                    switch (type)
                    {
                        case "loginotp":
                            result = getOtp(type, user, clientId, mobile);
                            break;

                        default:
                            break;
                    }
                    string jsonString = JsonSerializer.Serialize(result);
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var response = client.PostAsync(smsNodeUrl + "/smsDetails", content);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private object getOtp(string type, string user, int clientId, string mobile)
        {
            var result = _context.tblOtr.Where(oo => oo.clientId == clientId && oo.mobile == mobile)
                                .Select(oo => new
                                {
                                    clientId = oo.clientId,
                                    mobile = oo.mobile,
                                    otp = oo.otp,
                                    type = type,
                                    user = user
                                }).SingleOrDefault<object>();
            return result;
        }
    }
}

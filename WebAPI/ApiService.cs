using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Elfie.Model;
using Microsoft.DotNet.MSIdentity.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using StackExchange.Redis;
using System.Buffers;
using System.Data;
using System.Globalization;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static SL_Bullion.WebAPI.bullionController;
using static StackExchange.Redis.Role;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SL_Bullion.WebAPI
{
    public class ApiService
    {
        private readonly BullionDbContext _context;
        private readonly ApplicationConstant _constatnt;
        private readonly MessageConstant _messageConstatnt;
        private readonly IConfiguration _configuration;
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
        private readonly ILogger<ApiService> _logger;
        ResponseBody _response = new ResponseBody();
        public ApiService(BullionDbContext context, ApplicationConstant constatnt, MessageConstant messageConstant, IConfiguration configuration, ILogger<ApiService> logger)
        {
            _context = context;
            _constatnt = constatnt;
            _messageConstatnt = messageConstant;
            _configuration = configuration;
            _logger = logger;
        }

        internal async Task<JsonObject> updateOrder(string user, int tradeType, int symbolId, double volume, int orderNo, int accountId, string deviceType, int orderId)
        {
            JsonObject afterOrder = new JsonObject();
            var master = await _context.tblMaster.FirstOrDefaultAsync(m => m.userName == user);
            if (master != null)
            {
                master.startDealNo += 1;
                await _context.SaveChangesAsync();
            }
            var symbol = await _context.tblSymbol.FirstOrDefaultAsync(s => s.id == symbolId);

            if (symbol != null)
            {
                   symbol.useStock = tradeType switch
                    {
                        1 => symbol.useStock + (int)volume,
                        3 => symbol.useStock + (int)volume,
                        2 => symbol.useStock - (int)volume,
                        4 => symbol.useStock - (int)volume,
                        _ => throw new ArgumentException("Invalid tradeType value.")
                    };
                            
            }
            if (tradeType == 3 || tradeType == 4)
            {
                _constatnt.pushPendingOrder();
                _messageConstatnt.pushMessageAlert("limitPlaceOrder", user, accountId, orderNo);
            }
            else
            {
                if (_configuration.GetSection($"hedge:{user}").Exists() && deviceType != "admin")
                {
                    var response = await verifyHedgeDetails(user, tradeType, volume, symbol.id, orderNo, orderId);

                    if (response.code != 200)
                    {
                        afterOrder["code"] = response.code;
                        afterOrder["message"] = response.message;
                        LogOrderFailure(accountId, symbolId, user, volume, 0, afterOrder["message"].ToString());
                        

                    }
                }
                _messageConstatnt.pushMessageAlert("executeOrder", user, accountId, orderNo);
            }
            _constatnt.pushOrderAlert("open", user, accountId, tradeType, orderNo);
            return afterOrder;
        }

       
        
        private async Task<ResponseBody> updateHedgeData(int orderId, string exchange)
        {
            var openOrder = await _context.tblOpenOrder.FindAsync(orderId);
            if (openOrder != null)
            {
                var symbol = _context.tblSymbol.Where(s => s.id == openOrder.symbolId).Select(s => new
                {
                    division = s.division,
                    multiply = s.multiply
                }).FirstOrDefault();
                if (!openOrder.isLimit)
                {
                    double diff = Convert.ToDouble(exchange) - openOrder.exchange;
                    openOrder.rate = openOrder.rate + diff;
                    openOrder.total = _constatnt.getTotalRate(openOrder.source, openOrder.rate, symbol.multiply, symbol.division, openOrder.volume);
                    openOrder.tax = _constatnt.getTaxRate(openOrder.total, openOrder.symbolId, openOrder.clientId, openOrder.loginId);
                }
                openOrder.isHedge = true;
                openOrder.exchange = Convert.ToDouble(exchange);
                await _context.SaveChangesAsync();
            }
            return _response;
        }

        internal async Task<ResponseBody> verifyHedgeDetails(string user, int tradeType, double volume, int symbolId, int orderNo, int orderId)
        {
            try
            {
                int clientId = _constatnt.getClientId(user);
                bool isHedge = _context.tblContact.Where(c => c.clientId == clientId).Select(c => c.isHedge).FirstOrDefault();

                if (isHedge)
                {
                    var hedgeInfo = await _context.tblHedge.FirstOrDefaultAsync(h => h.symbolId == symbolId);
                    if (hedgeInfo.status)
                    {
                        var hedgeConfig = _configuration.GetSection($"hedge:{user}");
                        string hedgeSymbol = _context.tblHedgeSymbol.Where(hs => hs.id == hedgeInfo.hedgeSymbolId).Select(hs => hs.name).FirstOrDefault();
                        double lotSize = volume / Convert.ToDouble(hedgeInfo.division);
                        if (hedgeConfig["name"] == "meta")
                        {
                            string type = tradeType == 1 ? "BUY" : "SELL";
                            var metaRequest = new metaRequest
                            {
                                symbol = hedgeSymbol,
                                volume = lotSize,
                                type = type,
                                user = user,
                                comment = orderNo.ToString()
                            };

                            string requestString = JsonSerializer.Serialize(metaRequest);
                            _response = await _constatnt.sendOrderToMeta(requestString, user);

                            if (_response.code == 200)
                            {
                                using JsonDocument doc = JsonDocument.Parse(_response.data.ToString());
                                var root = doc.RootElement;

                                if (root.GetProperty("data").GetProperty("retcode").ToString() == "10009")
                                {
                                    _response = await updateHedgeData(
                                        orderId,
                                        root.GetProperty("data").GetProperty("price").ToString()
                                    );
                                }
                                else
                                {
                                    addHedgeData(clientId, requestString, _response.data.ToString());
                                    await deleteOpenOrder(orderId);
                                    return FailResponse("Trade Not Execute. Please Try Again");
                                }
                            }
                            else
                            {
                                addHedgeData(clientId, requestString, _response.data.ToString());
                                await deleteOpenOrder(orderId);
                                return FailResponse("Trade Not Execute. Please Try Again");
                            }

                            addHedgeData(clientId, requestString, _response.data.ToString());
                            return _response;
                        }

                        else if (hedgeConfig["name"] == "admin")
                        {
                            try
                            {
                                var token = _configuration.GetValue<string>($"hedge:{user}:token");
                                //var result = await _context.tblOpenOrder.Where(OO => OO.dealNo == orderNo).Select(OO => new { OO.deviceType }).FirstOrDefaultAsync();
                                //string TradeFrom = result?.deviceType ?? "Web.Mobile";
                                string TradeFrom = user;
                                int division = hedgeInfo.division;
                                object metaRequest;
                                if (user == "ganeshbullion" || user == "mjbulliongold" || user == "shreeaurum")
                                {
                                    metaRequest = new
                                    {
                                        SymbolId = division.ToString(),
                                        Token = token,
                                        Quantity = volume.ToString(CultureInfo.InvariantCulture),
                                        TradeFrom = TradeFrom,
                                        TradeType = tradeType.ToString(CultureInfo.InvariantCulture),
                                        DeviceToken = "",
                                        BuyLimitPrice = "",
                                        SellLimitPrice = ""
                                    };
                                }
                                else
                                {
                                    metaRequest = new
                                    {
                                        SymbolId = division.ToString(),
                                        Token = token,
                                        Quantity = volume.ToString(CultureInfo.InvariantCulture),
                                        TradeFrom = TradeFrom,
                                        TradeType = tradeType.ToString(CultureInfo.InvariantCulture),

                                    };
                                }

                                string requestJson = System.Text.Json.JsonSerializer.Serialize(metaRequest);
                                var response = await _constatnt.sendOrderToAdmin(requestJson, user);
                                if (response.code == 200)
                                {
                                    if (TryExtractJsonFromSoap(response.data, out string jsonResult))
                                    {
                                        var jsonObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResult);
                                        if (jsonObj != null && jsonObj.TryGetValue("Exchange", out object exchangeValue))
                                        {
                                            _response = await updateHedgeData(orderId, exchangeValue.ToString());
                                            addHedgeData(clientId, requestJson, response.data.ToString());
                                            return _response;
                                        }
                                        else
                                        {
                                            await deleteOpenOrder(orderId);
                                            _response = await updateHedgeData(orderId, jsonObj.ToString());
                                            addHedgeData(clientId, requestJson, response.data.ToString());
                                            return FailResponse("Trade Not Execute. Please Try Again");
                                        }
                                    }
                                }
                                else
                                {
                                    await deleteOpenOrder(orderId);
                                    addHedgeData(clientId, requestJson, response.data.ToString());
                                    return FailResponse("Trade Not Execute. Please Try Again");
                                }
                            }

                            catch (Exception ex)
                            {
                                await deleteOpenOrder(orderId);
                                return FailResponse("Trade Not Execute. Please Try Again");
                            }
                        }

                        else if (hedgeConfig["name"] == "yashvi")
                        {
                            try
                            {
                                var token = _configuration.GetValue<string>($"hedge:{user}:token");
                                var result = await _context.tblOpenOrder.Where(OO => OO.dealNo == orderNo).Select(OO => new { OO.deviceType }).FirstOrDefaultAsync();

                                string tradeFrom = result?.deviceType ?? "Web.Mobile";
                                int division = hedgeInfo.division;

                                var yashviRequest = new
                                {
                                    SymbolId = division.ToString(),
                                    Token = token,
                                    Quantity = volume.ToString(CultureInfo.InvariantCulture),
                                    TradeFrom = tradeFrom,
                                    TradeType = tradeType.ToString(CultureInfo.InvariantCulture)
                                };

                                string requestJson = JsonSerializer.Serialize(yashviRequest);
                                var response = await _constatnt.sendOrderToSmbullion(requestJson, user);

                                if (response.code == 200)
                                {
                                    if (TryExtractJsonFromSoap(response.data, out string jsonResult) &&
                                        TryProcessJsonResult(jsonResult, out string exchange))
                                    {
                                        _response = await updateHedgeData(orderId, exchange);
                                        return _response;
                                    }
                                }

                                await deleteOpenOrder(orderId);
                                return FailResponse("Trade Not Execute. Please Try Again");
                            }
                            catch (Exception)
                            {
                                await deleteOpenOrder(orderId);
                                return FailResponse("Trade Not Execute. Please Try Again");
                            }
                        }


                    }

                }
            }

            catch (Exception ex)
            {

                return FailResponse($"Trade Not Execute. Please Try Again ({ex.Message})");
            }

            return _response;
        }

        private ResponseBody FailResponse(string message)
        {
            return new ResponseBody
            {
                code = 400,
                message = message
            };
        }


        //internal async Task<ResponseBody> verifyHedgeDetails(string user, int tradeType, double volume, int symbolId,int orderNo,int orderId)
        //{
        //    try
        //    {
        //        int clientId = _constatnt.getClientId(user);
        //        bool isHedge = _context.tblContact.Where(c => c.clientId == clientId).Select(c => c.isHedge).FirstOrDefault();
        //        if (isHedge)
        //        {
        //            var hedgeInfo = await _context.tblHedge.FirstOrDefaultAsync(h => h.symbolId == symbolId);
        //            if (hedgeInfo.status)
        //            {
        //                var hedgeConfig = _configuration.GetSection($"hedge:{user}");
        //                string hedgeSymbol = _context.tblHedgeSymbol.Where(hs => hs.id == hedgeInfo.hedgeSymbolId).Select(hs => hs.name).FirstOrDefault();
        //                double lotSize = volume / Convert.ToDouble(hedgeInfo.division);
        //                if (hedgeConfig["name"] == "meta")
        //                {
        //                    string type = "";
        //                    if (tradeType == 1)
        //                    {
        //                        type = "BUY";
        //                    }
        //                    else if (tradeType == 2)
        //                    {
        //                        type = "SELL";
        //                    }
        //                    var _metaRequest = new metaRequest();
        //                    _metaRequest.symbol = hedgeSymbol;
        //                    _metaRequest.volume = lotSize;
        //                    _metaRequest.type = type;
        //                    _metaRequest.user = user;
        //                    _metaRequest.comment = orderNo.ToString();
        //                    string requestString = JsonSerializer.Serialize(_metaRequest);
        //                    _response = await _constatnt.sendOrderToMeta(requestString, user);
        //                    if (_response.code == 200)
        //                    {
        //                        using JsonDocument doc = JsonDocument.Parse(_response.data.ToString());
        //                        var root = doc.RootElement;
        //                        if (root.GetProperty("data").GetProperty("retcode").ToString() == "10009")
        //                        {
        //                            _response = await updateHedgeData(orderId, root.GetProperty("data").GetProperty("price").ToString());
        //                        }
        //                        else
        //                        {
        //                            await deleteOpenOrder(orderId);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        _response.data = _response.message;
        //                        await deleteOpenOrder(orderId);
        //                    }
        //                    addHedgeData(clientId, requestString, _response.data.ToString());
        //                }
        //                else if (hedgeConfig["name"] == "yashvi")
        //                {
        //                    try
        //                    {
        //                        var token = _configuration.GetValue<string>($"hedge:{user}:token");
        //                        var result = await _context.tblOpenOrder.Where(OO => OO.dealNo == orderNo).Select(OO => new { OO.deviceType }).FirstOrDefaultAsync();
        //                        string TradeFrom = result?.deviceType ?? "Web.Mobile";
        //                        int division = hedgeInfo.division;

        //                        var metaRequest = new
        //                        {
        //                            SymbolId = division.ToString(),
        //                            Token = token,
        //                            Quantity = volume.ToString(CultureInfo.InvariantCulture),
        //                            TradeFrom = TradeFrom,
        //                            TradeType = tradeType.ToString(CultureInfo.InvariantCulture)
        //                        };
        //                        string requestJson = JsonSerializer.Serialize(metaRequest);
        //                        var response = await _constatnt.sendOrderToSmbullion(requestJson, user);
        //                        if (response.code == 200)
        //                        {
        //                            if (TryExtractJsonFromSoap(response.data, out string jsonResult))
        //                            {
        //                                if (TryProcessJsonResult(jsonResult, out string exchange))
        //                                {
        //                                    _response = await updateHedgeData(orderId, exchange);
        //                                }
        //                                else
        //                                {
        //                                    await deleteOpenOrder(orderId);
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            await deleteOpenOrder(orderId);
        //                            Console.WriteLine($"❌ Failed to send order. Code: {response.code}, Message: {response.message}");
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        await deleteOpenOrder(orderId);
        //                        Console.WriteLine($"⚠️ An error occurred: {ex.Message}");
        //                    }
        //                }

        //            }
        //            else
        //            {

        //                await deleteOpenOrder(orderId);
        //            }
        //        }
        //        else
        //        {

        //            await deleteOpenOrder(orderId);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        _response.code = 400;
        //        _response.message = ex.Message;
        //    }
        //    return _response;
        //}

        private void addHedgeData(int clientId, string request, string response)
        {
            var hedgeInfo = new HedgeInfo();
            hedgeInfo.clientId = clientId;
            hedgeInfo.request = request;
            hedgeInfo.response = response;
            _context.Add(hedgeInfo);
            _context.SaveChanges();
            int totalRecords = _context.tblHedgeInfo.Where(h => h.clientId == clientId).Count();

            if (totalRecords > 1000)
            {
                var recordsToDelete = _context.tblHedgeInfo.OrderBy(h => h.id).Take(totalRecords - 100).ToList();

                _context.tblHedgeInfo.RemoveRange(recordsToDelete);
                _context.SaveChangesAsync();
            }
        }

        internal JsonObject verifyOrder(int tradeType, string user, int symbolId, int clientId, string loginId, double tradePrice, double volume)
        {
            JsonObject objRate = new JsonObject();
            try
            {
                double rate = 0, commonPremium = 0, symbolPremium = 0;
                decimal total = 0, tax = 0;
                var mainProduct = getMainProduct(user, symbolId);
                objRate.Add("source", mainProduct[0].GetProperty("src").ToString());
                objRate.Add("symbolName", mainProduct[0].GetProperty("name").ToString());
                var account = _context.tblAccount.AsNoTracking().Where(a => a.loginId == loginId && a.clientId == clientId).Select(a => new
                {
                    accountId = a.id,
                    groupId = a.groupId,
                    remainingMargin = a.margin - Convert.ToDecimal(_context.tblOpenOrder.Where(o => o.clientId == clientId && o.loginId == loginId).Sum(o => o.margin)),
                    tradeAccess = a.tradeAccess,
                }).FirstOrDefault();

                var symbol = _context.tblSymbol.AsNoTracking().Where(s => s.id == symbolId).Select(s => new
                {
                    s.identifier,
                    s.division,
                    s.multiply,
                    s.rateType,
                    margin = s.initialMargin,
                    remainingStock = s.stock - s.useStock,
                    isSymbolTrade = s.isTrade,
                    session = _context.tblSymbolSession.Where(ss => ss.symbolId == s.id).Select(ss => ss.session).FirstOrDefault()
                }).FirstOrDefault();
                var contact = _context.tblContact.AsNoTracking().Where(c => c.clientId == clientId).Select(c => new
                {
                    freezOuter = c.freezOuter,
                    freezInner = c.freezInner,
                    offQuotes = c.offQuotes,
                    isTrade = c.isTrade
                }).FirstOrDefault();
                commonPremium = _context.tblGroup.AsNoTracking().Where(g => g.id == account.groupId).Select(g => (tradeType == 1 || tradeType == 3) ? (mainProduct[0].GetProperty("src").ToString() == "gold" ? g.sellPremiumGold : g.sellPremiumSilver) : (tradeType == 2 || tradeType == 4) ? (mainProduct[0].GetProperty("src").ToString() == "gold" ? g.buyPremiumGold : g.buyPremiumSilver) : 0).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(symbol?.identifier) && symbol.identifier.StartsWith("Rate_", StringComparison.OrdinalIgnoreCase))
                {
                    commonPremium = 0;
                }
                symbolPremium = _context.tblGroupSymbol.AsNoTracking().Where(gs => gs.groupId == account.groupId && gs.symbolId == symbolId).Select(gs => (tradeType == 3 || tradeType == 1) ? gs.sellPremium : gs.buyPremium).FirstOrDefault();

                var checkVolume = _context.tblGroupSymbol.Where(g => g.groupId == account.groupId && g.symbolId == symbolId).Select(g => new { g.oneClick, g.inTotal }).FirstOrDefault();

                bool isVolumeValid = volume >= checkVolume.oneClick && volume <= checkVolume.inTotal;
                objRate.Add("isVolumeValid", isVolumeValid);

                DateTime feedTime = DateTime.ParseExact(mainProduct[0].GetProperty("time").ToString(), "hh:mm:ss tt", null);
                TimeSpan timeDifference = DateTime.Now - feedTime;
                bool isOffQuotes = contact.offQuotes == 0 ? false : Math.Abs(timeDifference.TotalSeconds) >= contact.offQuotes;
                Console.WriteLine(Math.Abs(timeDifference.TotalSeconds));
                var session = JsonSerializer.Deserialize<Dictionary<string, string>>(symbol.session);
                var startTime = session[$"start{DateTime.Now.DayOfWeek}"];
                var endTime = session[$"end{DateTime.Now.DayOfWeek}"];
                bool isSession = DateTime.Now.TimeOfDay >= TimeSpan.ParseExact(startTime, "hh\\:mm", null) && DateTime.Now.TimeOfDay <= TimeSpan.ParseExact(endTime, "hh\\:mm", null);

                objRate.Add("offQuotes", isOffQuotes);
                objRate.Add("tradeAccess", true);
                objRate.Add("freezLevel", false);
                objRate.Add("session", isSession);
                objRate.Add("isTrade", contact.isTrade);
                objRate.Add("isSymbolTrade", symbol.isSymbolTrade);
                objRate.Add("accountId", account.accountId);
                if (tradeType == 1)
                {
                    rate = Convert.ToDouble(mainProduct[0].GetProperty("ask").ToString());
                    rate = rate + commonPremium + symbolPremium;
                    objRate.Add("rate", rate);
                    objRate.Add("exchange", Convert.ToDouble(mainProduct[0].GetProperty("sell").ToString()));
                    objRate.Add("premium", Convert.ToDouble(mainProduct[0].GetProperty("sp").ToString()));
                    objRate.Add("premiumLimit", 0.00);
                    if (account.tradeAccess == 3)
                    {
                        objRate["tradeAccess"] = false;
                    }
                }
                else if (tradeType == 2)
                {
                    rate = Convert.ToDouble(mainProduct[0].GetProperty("bid").ToString());
                    rate = rate + commonPremium + symbolPremium;
                    objRate.Add("rate", rate);
                    objRate.Add("exchange", Convert.ToDouble(mainProduct[0].GetProperty("buy").ToString()));
                    objRate.Add("premium", Convert.ToDouble(mainProduct[0].GetProperty("bp").ToString()));
                    objRate.Add("premiumLimit", 0.00);
                    if (account.tradeAccess == 2)
                    {
                        objRate["tradeAccess"] = false;
                    }
                }

                else if (tradeType == 3)
                {
                    rate = Convert.ToDouble(mainProduct[0].GetProperty("ask").ToString());
                    rate = rate + commonPremium + symbolPremium;
                    if (rate <= tradePrice)
                    {
                        objRate["freezLevel"] = true;
                    }
                    else if (rate - contact.freezOuter >= tradePrice && tradePrice >= rate - contact.freezInner)
                    {
                        objRate["freezLevel"] = false;
                        _logger.LogInformation($"info : rate {rate} : freezOuter {contact.freezOuter} : tradePrice {tradePrice} : freezInner {contact.freezInner}");
                    }
                    else
                    {
                        objRate["freezLevel"] = true;
                    }

                    if (account.tradeAccess == 3)
                    {
                        objRate["tradeAccess"] = false;
                    }

                    rate = tradePrice;
                    objRate.Add("rate", rate);
                    objRate.Add("exchange", Convert.ToDouble(mainProduct[0].GetProperty("sell").ToString()));
                    objRate.Add("premium", 0.00);
                    objRate.Add("premiumLimit", Convert.ToDouble(mainProduct[0].GetProperty("sp").ToString()));
                }

                else if (tradeType == 4)
                {
                    rate = Convert.ToDouble(mainProduct[0].GetProperty("bid").ToString());
                    rate = rate + commonPremium + symbolPremium;
                    if (rate >= tradePrice)
                    {
                        objRate["freezLevel"] = true;
                    }
                    else if (rate + contact.freezOuter <= tradePrice && tradePrice <= rate + contact.freezInner)
                    {
                        objRate["freezLevel"] = false;
                    }
                    else
                    {
                        objRate["freezLevel"] = true;
                    }
                    if (account.tradeAccess == 2)
                    {
                        objRate["tradeAccess"] = false;
                    }
                    rate = tradePrice;
                    objRate.Add("rate", rate);
                    objRate.Add("exchange", Convert.ToDouble(mainProduct[0].GetProperty("sell").ToString()));
                    objRate.Add("premium", 0.00);
                    objRate.Add("premiumLimit", Convert.ToDouble(mainProduct[0].GetProperty("bp").ToString()));

                }
                total = _constatnt.getTotalRate(mainProduct[0].GetProperty("src").ToString(), rate, symbol.multiply, symbol.division, volume);
                tax = _constatnt.getTaxRate(total, symbolId, clientId, loginId);
                objRate.Add("total", total);
                objRate.Add("tax", tax);
                objRate.Add("margin", symbol.margin * volume);
                objRate.Add("rateType", symbol.rateType);
                objRate.Add("remainingStock", symbol.remainingStock);
                objRate.Add("remainingMargin", account.remainingMargin);
            }
            catch (Exception)
            {
                throw;
            }
            return objRate;
        }
        internal List<JsonElement> getMainProduct(string user, int symbolId)
        {
            IDatabase db = redis.GetDatabase();
            string mainProduct = db.StringGet(user);
            JsonDocument array = JsonDocument.Parse(mainProduct);
            var product = array.RootElement.EnumerateArray().Where(e => (e.GetProperty("id").GetInt16()) == symbolId).ToList();
            return product;
        }
        internal async Task<object> getOrder(int clientId, string loginId, string orderType, DateTime? fromDate, DateTime? toDate, string? searchData)
        {
            double taxType = 0;
            JsonArray order = new JsonArray();

            var taxRate = await _context.tblOpenOrder.Where(o => o.clientId == clientId && o.loginId == loginId).OrderByDescending(o => o.id).Select(o => new { o.symbolId }).FirstOrDefaultAsync();
            if (taxRate != null)
            {
                taxType = await _context.tblSymbol.Where(s => s.id == taxRate.symbolId && s.isBill).Select(s => s.gstBill).FirstOrDefaultAsync();

            }
            if (orderType == "open")
            {
                var data = await _context.tblOpenOrder.Where(o => o.clientId == clientId && o.loginId == loginId).OrderByDescending(o => o.id).Select(o => new
                {
                    id = o.id,
                    dealNo = o.dealNo,
                    loginId = o.loginId,
                    name = _context.tblAccount.Where(a => a.clientId == o.clientId && a.loginId == o.loginId).Select(a => a.name).FirstOrDefault(),
                    firm = _context.tblAccount.Where(a => a.clientId == o.clientId && a.loginId == o.loginId).Select(a => a.firmName).FirstOrDefault(),
                    symbolName = o.symbolName,
                    tradeType = o.tradeType,
                    tradeTypeView = o.tradeType == 1 && o.isLimit == false ? "Buy" : o.tradeType == 1 && o.isLimit == true ? "BuyLimit" : o.tradeType == 2 && o.isLimit == false ? "Sell" : o.tradeType == 2 && o.isLimit == true ? "SellLimit" : o.tradeType == 3 ? "BuyLimit" : o.tradeType == 4 ? "SellLimit" : "Buy",
                    volume = o.volume,
                    rate = o.rate,
                    total = o.tax,
                    source = o.source,
                    orderTime = o.orderTime.ToUniversalTime()
                }).ToListAsync();
                return data;
            }
            else if (orderType == "close")
            {
                var data = await _context.tblCloseOrder.Where(o => o.clientId == clientId && o.loginId == loginId && o.closeTime >= fromDate && o.closeTime <= toDate).OrderByDescending(o => o.id).Select(o => new
                {
                    id = o.id,
                    dealNo = o.dealNo,
                    loginId = o.loginId,
                    name = _context.tblAccount.Where(a => a.clientId == o.clientId && a.loginId == o.loginId).Select(a => a.name).FirstOrDefault(),
                    firm = _context.tblAccount.Where(a => a.clientId == o.clientId && a.loginId == o.loginId).Select(a => a.firmName).FirstOrDefault(),
                    symbolName = o.symbolName,
                    tradeTypeView = o.tradeType == 1 ? "Buy" : "Sell",
                    volume = o.volume,
                    rate = o.rate,
                    total = Math.Round(o.total + (o.total * Convert.ToDecimal(taxType)) / 100, 2),
                    orderTime = o.orderTime.ToUniversalTime(),
                    source = o.source
                }).ToListAsync();
                return data;
            }
            else if (orderType == "delete")
            {
                var data = await _context.tblDeleteOrder.Where(o => o.clientId == clientId && o.loginId == loginId && o.deleteTime >= fromDate && o.deleteTime <= toDate).OrderByDescending(o => o.id).Select(o => new
                {
                    id = o.id,
                    dealNo = o.dealNo,
                    loginId = o.loginId,
                    name = _context.tblAccount.Where(a => a.clientId == o.clientId && a.loginId == o.loginId).Select(a => a.name).FirstOrDefault(),
                    firm = _context.tblAccount.Where(a => a.clientId == o.clientId && a.loginId == o.loginId).Select(a => a.firmName).FirstOrDefault(),
                    symbolName = o.symbolName,
                    tradeTypeView = o.tradeType == 1 ? "Buy" : o.tradeType == 2 ? "Sell" : o.tradeType == 3 ? "BuyLimit" : "SellLimit",
                    volume = o.volume,
                    rate = o.rate,
                    total = Math.Round(o.total + (o.total * Convert.ToDecimal(taxType)) / 100, 2),
                    orderTime = o.orderTime.ToUniversalTime(),
                    source = o.source,
                }).ToListAsync();
                return data;
            }
            else if (orderType == "unFix")
            {
                var data = await _context.tblUnFixOrder.Where(o => o.clientId == clientId && o.loginId == loginId).OrderByDescending(o => o.id).Select(o => new
                {
                    id = o.id,
                    dealNo = o.dealNo,
                    loginId = o.loginId,
                    name = _context.tblAccount.Where(a => a.clientId == o.clientId && a.loginId == o.loginId).Select(a => a.name).FirstOrDefault(),
                    firm = _context.tblAccount.Where(a => a.clientId == o.clientId && a.loginId == o.loginId).Select(a => a.firmName).FirstOrDefault(),
                    symbolName = o.symbolName,
                    tradeTypeView = o.tradeType == 1 ? "Buy" : "Sell",
                    volume = o.volume,
                    rate = o.rate,
                    total = Math.Round(o.total + (o.total * Convert.ToDecimal(taxType)) / 100, 2),
                    orderTime = o.orderTime.ToUniversalTime(),
                    source = o.source,
                }).ToListAsync();
                return data;
            }

            return order;
        }
        internal async Task<JsonObject> getProfileInfo(int accountId)
        {
            var account = await _context.tblAccount.Where(a => a.id == accountId).Select(a => new
            {
                firmName = a.firmName,
                name = a.name,
                email = a.email,
                city = a.city,
                mobile = a.mobile
            }).FirstOrDefaultAsync();
            if (account != null)
            {
                var info = new JsonObject();
                info["firmName"] = account.firmName;
                info["name"] = account.name;
                info["email"] = account.email;
                info["city"] = account.city;
                info["mobile"] = account.mobile;
                return info;
            }
            return null;
        }
        internal async Task<JsonObject> getTradeInfo(int? symbolId, int accountId)
        {
            int groupId = _context.tblAccount.Where(a => a.id == accountId).Select(a => a.groupId).FirstOrDefault();
            var groupSymbol = await _context.tblGroupSymbol.Where(gs => gs.symbolId == symbolId && gs.groupId == groupId).Select(gs => new
            {
                oneClick = gs.oneClick,
                inTotal = gs.inTotal,
                step = gs.step,
                symbol = _context.tblSymbol.Where(s => s.id == gs.symbolId).Select(a => a.name).FirstOrDefault()
            }).FirstOrDefaultAsync();
            if (groupSymbol != null)
            {
                var info = new JsonObject();
                info["oneClick"] = groupSymbol.oneClick;
                info["inTotal"] = groupSymbol.inTotal;
                info["step"] = groupSymbol.step;
                info["symbol"] = groupSymbol.symbol;
                return info;
            }
            return null;
        }
        internal async Task<JsonDocument> getSymbolInfo(int? symbolId)
        {
            var symbolSession = await _context.tblSymbolSession.Where(a => a.symbolId == symbolId).Select(a => a.session).FirstOrDefaultAsync();
            var info = JsonDocument.Parse(symbolSession);
            return info;
        }
        internal async Task<JsonObject> verifyRateCutOrder(double volume, int premium, int symbolId, string source, double rate)
        {
            try
            {
                JsonObject objRate = new JsonObject();
                var unFixOrder = await _context.tblUnFixOrder.FirstOrDefaultAsync(uo => uo.id == premium);
                objRate.Add("isVolume", true);
                var symbol = await _context.tblSymbol.Where(s => s.id == symbolId).Select(s => new
                {
                    division = s.division,
                    multiply = s.multiply
                }).FirstOrDefaultAsync();
                if (unFixOrder.volume > volume)
                {
                    unFixOrder.volume = unFixOrder.volume - volume;
                    unFixOrder.total = _constatnt.getTotalRate(source, unFixOrder.rate, symbol.multiply, symbol.division, unFixOrder.volume);
                    await _context.SaveChangesAsync();
                }
                else if (unFixOrder.volume == volume)
                {
                    _context.tblUnFixOrder.Remove(unFixOrder);
                }
                else
                {
                    objRate["isVolume"] = false;
                }
                objRate.Add("rate", rate + unFixOrder.rate);
                objRate.Add("total", _constatnt.getTotalRate(source, rate + unFixOrder.rate, symbol.multiply, symbol.division, volume));
                return objRate;
            }
            catch (Exception)
            {

                throw;
            }

        }
        bool TryExtractJsonFromSoap(object? responseData, out string jsonResult)
        {
            jsonResult = string.Empty;
            try
            {
                if (responseData is string data)
                {
                    var soapDoc = XDocument.Parse(data);
                    var resultNode = soapDoc.Descendants().FirstOrDefault(e => e.Name.LocalName == "InsertOpenOrderDetailWithRegIDResult");
                    if (resultNode != null)
                    {
                        jsonResult = resultNode.Value.Trim();
                        return true;
                    }
                    Console.WriteLine("⚠️ Failed to extract JSON from SOAP response.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error extracting SOAP data: {ex.Message}");
            }
            return false;
        }
        bool TryProcessJsonResult(string jsonResult, out string exchange)
        {
            exchange = string.Empty;
            try
            {
                using JsonDocument doc = JsonDocument.Parse(jsonResult);
                var root = doc.RootElement;

                if (root.TryGetProperty("ReturnCode", out var returnCode) && returnCode.GetString() == "200")
                {
                    if (root.TryGetProperty("Exchange", out var exchangeProp))
                    {
                        exchange = exchangeProp.GetString() ?? string.Empty;
                        Console.WriteLine($"✅ Exchange value: {exchange}");
                        return true;
                    }

                    Console.WriteLine("⚠️ Exchange value is missing from response.");
                }
                else
                {
                    Console.WriteLine($"❌ Order failed: {root.GetProperty("ReturnMsg").GetString()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error parsing JSON response: {ex.Message}");
            }
            return false;
        }

        private async Task deleteOpenOrder(int orderId)
        {
            var openOrder = await _context.tblOpenOrder.FindAsync(orderId);
            if (openOrder != null)
            {
                _context.tblOpenOrder.Remove(openOrder);
                await _context.SaveChangesAsync();
            }

        }
        internal async Task<JsonObject> verifyCoinOrder(TradeModel tradeModel, int clientId, string loginId, string authToken)
        {
            JsonObject objRate = new JsonObject();
            try
            {
                string struserDetails, Password = "", UserName = "", SymbolName = "", loginid = "";
                double Total = 0, Exchange = 0, Rate = 0;
                int ClientId = 0;


                var ObjUser = await _constatnt.DecodeToken(authToken.ToString().Split(" ")[1]);
                loginid = ObjUser.loginId;
                ClientId = await _context.tblMaster.Where(x => x.userName == ObjUser.user).Select(o => o.id).FirstOrDefaultAsync();
                UserName = ObjUser.user;
                //ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
                IDatabase db = redis.GetDatabase();
                string value = db.StringGet("coinrate_" + UserName);
                DataTable dtSymbol = JsonStringToDataTable2(JsonSerializer.Deserialize<JsonObject>(value)["products"].ToString());
                DataView dv = new DataView(dtSymbol);
                dv.RowFilter = "id = " + tradeModel.SymbolId + "";
                if (dv.Count > 0)
                {
                    SymbolName = dv[0]["name"].ToString();
                    string ExchangeSymbol = db.StringGet(dv[0]["src"].ToString().ToLower());
                    DataTable dtExchangeSymbol = JsonStringToDataTable(ExchangeSymbol);

                    if (tradeModel.TradeType == "1")
                    {
                        Rate = Convert.ToDouble(dv[0]["Ask"].ToString());
                        Exchange = Convert.ToDouble(dtExchangeSymbol.Rows[0]["Ask"].ToString());
                    }
                    else if (tradeModel.TradeType == "2")
                    {
                        Rate = Convert.ToDouble(dv[0]["Bid"].ToString());
                        Exchange = Convert.ToDouble(dtExchangeSymbol.Rows[0]["Bid"].ToString());
                    }
                }


                var tblMaster = await _context.tblMaster.Where(m => m.id == ClientId).ToListAsync();
                var tblCoin = await _context.tblCoin.Where(m => m.clientId == ClientId).ToListAsync();
                var tblOpenOrderCoin = await _context.tblOpenOrderCoin.Where(m => m.ClientId == ClientId).ToListAsync();
                var tblCloseOrderCoin = await _context.tblCloseOrderCoin.Where(m => m.ClientId == ClientId).ToListAsync();
                var tblAccount = await _context.tblAccount.Where(m => m.clientId == ClientId).ToListAsync();
                var tblContact = await _context.tblContact.Where(m => m.clientId == ClientId).ToListAsync();

                double CoinOpenOrderTotalLost = tblOpenOrderCoin.Where(m => m.SymbolID == tradeModel.SymbolId && m.TradeType == "1").Sum(x => x.Volume)
                                            + tblCloseOrderCoin.Where(m => m.SymbolID == tradeModel.SymbolId && m.TradeType == "1").Sum(x => x.Volume);

                string Source = tblCoin.Where(m => m.id == tradeModel.SymbolId).Select(x => x.source).FirstOrDefault();
                int DealNo = tblMaster.Select(x => x.lastCoinDealNo).FirstOrDefault();
                bool Status = tblAccount.Where(m => m.loginId == loginid.ToString()).FirstOrDefault().isActive;
                bool Stock = tblCoin.Where(m => m.id == tradeModel.SymbolId).Select(x => x.isStock).First();
                double SymbolStocks = tblCoin.Where(m => m.id == tradeModel.SymbolId).Select(x => x.coinStock).FirstOrDefault();
                string Coinstarttime = tblMaster.Select(x => x.coinStartTime).FirstOrDefault();
                string Coinendtime = tblMaster.Select(x => x.coinEndTime).FirstOrDefault();
                double RemainingVolume = 0;
                RemainingVolume = SymbolStocks - CoinOpenOrderTotalLost;
                //bool GlobleCoinTradeOn = tblMaster.Select(x => x.coinTradeOn).FirstOrDefault();
                bool GlobleCoinTradeOn = tblContact.Select(x => x.isCoinTrade).FirstOrDefault();
                int coinId = tblCoin.Where(m => m.id == tradeModel.SymbolId).Select(x => x.coinId).First();

                objRate.Add("loginid", loginid);
                objRate.Add("UserName", UserName);
                objRate.Add("SymbolName", SymbolName);
                objRate.Add("Rate", Rate);
                objRate.Add("DealNo", DealNo);
                objRate.Add("ClientId", ClientId);
                objRate.Add("Source", Source);
                objRate.Add("Exchange", Exchange);
                objRate.Add("GlobleCoinTradeOn", GlobleCoinTradeOn);
                objRate.Add("IsCoinStock", Stock);
                objRate.Add("Status", Status);
                objRate.Add("Volume", tradeModel.Volume);
                objRate.Add("RemainingVolume", RemainingVolume);
                objRate.Add("Coinstarttime", Coinstarttime);
                objRate.Add("Coinendtime", Coinendtime);
                objRate.Add("coinId", coinId);
            }
            catch (Exception)
            {
                throw;
            }
            return objRate;
        }



        //internal async Task LogOrderFailure(int clientId, int symbolId, string loginId,double quantity, double price, string reason)
        //{
        //    var clientInfo = await _context.tblAccount.Where(a => a.loginId == loginId && a.clientId == clientId).Select(a => new { a.name, a.firmName, a.mobile }).FirstOrDefaultAsync();
        //    var symbolName = await _context.tblSymbol.Where(s => s.id == symbolId).Select(s => s.name).FirstOrDefaultAsync();
        //    var log = new OrderFailureLog
        //    {
        //        clientId = clientId,
        //        symbolId = symbolId,
        //        loginId = loginId,
        //        name = clientInfo?.name,
        //        firmName = clientInfo?.firmName,
        //        mobile = clientInfo?.mobile,
        //        quantity = (decimal)quantity,
        //        price = (decimal)price,
        //        reason = reason,
        //        cdate = DateTime.UtcNow
        //    };

        //    _context.tblOrderFailureLog.Add(log);
        //    int rows = await _context.SaveChangesAsync();
        //    _constatnt.pushOrderfaieldAlert(action: "failure",user: clientInfo?.name ?? "", loginId: loginId, reason: reason,symbol: symbolName, quantity,price);
        //}



        internal async Task LogOrderFailure(int clientId, int symbolId, string loginId,double quantity, double price, string reason)
        {           
            var clientInfo = await _context.tblAccount.Where(a => a.loginId == loginId && a.clientId == clientId).Select(a => new { a.name, a.firmName, a.mobile, a.id }).FirstOrDefaultAsync();          
            var symbolName = await _context.tblSymbol.Where(s => s.id == symbolId).Select(s => s.name).FirstOrDefaultAsync();
           
            var log = new OrderFailureLog
            {
                clientId = clientId,
                symbolId = symbolId,
                loginId = loginId,
                name = clientInfo?.name,
                firmName = clientInfo?.firmName,
                mobile = clientInfo?.mobile,
                quantity = (decimal)quantity,
                price = (decimal)price,
                reason = reason,
                cdate = DateTime.UtcNow
            };

            _context.tblOrderFailureLog.Add(log);
            int rows = await _context.SaveChangesAsync();
            // user from tblMaster (not clientInfo.name)
            var userName = await _context.tblMaster.Where(m => m.id == clientId).Select(m => m.userName).FirstOrDefaultAsync();   
            if (clientInfo != null)
            {
                await _constatnt.pushOrderfaieldAlert(action: "failure", user: userName ?? "", accountId: clientInfo.id, reason: reason, symbol: symbolName, quantity: quantity, price: price);
            }
        }





        private DataTable JsonStringToDataTable(string jsonString)
        {
            DataTable dt = new DataTable();
            string[] jsonStringArray = Regex.Split(jsonString.Replace("[", "").Replace("]", ""), "},{");
            List<string> ColumnsName = new List<string>();
            foreach (string jSA in jsonStringArray)
            {
                string[] jsonStringData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");
                foreach (string ColumnsNameData in jsonStringData)
                {
                    try
                    {
                        int idx = ColumnsNameData.IndexOf(":");
                        string ColumnsNameString = ColumnsNameData.Substring(0, idx - 1).Replace("\"", "");
                        if (!ColumnsName.Contains(ColumnsNameString))
                        {
                            ColumnsName.Add(ColumnsNameString);
                        }
                    }
                    catch (Exception e)
                    {
                        //  TraceService(string.Format("Error Parsing Column Name : {0}", ColumnsNameData));

                    }
                }
                break;
            }
            foreach (string AddColumnName in ColumnsName)
            {
                dt.Columns.Add(AddColumnName);
            }
            foreach (string jSA in jsonStringArray)
            {
                string[] RowData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");
                DataRow nr = dt.NewRow();
                foreach (string rowData in RowData)
                {
                    try
                    {
                        int idx = rowData.IndexOf(":");
                        string RowColumns = rowData.Substring(0, idx - 1).Replace("\"", "");
                        string RowDataString = rowData.Substring(idx + 1).Replace("\"", "");
                        nr[RowColumns] = RowDataString;
                    }
                    catch (Exception e)
                    {
                        // TraceService(ex.Message.ToString());
                        continue;
                    }
                }
                dt.Rows.Add(nr);
            }
            return dt;
        }
        private DataTable JsonStringToDataTable2(string json)
        {
            // Parse JSON into a JsonDocument
            var doc = JsonDocument.Parse(json);
            var table = new DataTable();

            // Assume the JSON is an array of objects
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                // Add columns dynamically if not already added
                foreach (var property in element.EnumerateObject())
                {
                    if (!table.Columns.Contains(property.Name))
                    {
                        table.Columns.Add(property.Name);
                    }
                }
            }

            // Add rows
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                var row = table.NewRow();
                foreach (var property in element.EnumerateObject())
                {
                    row[property.Name] = property.Value.ToString();
                }
                table.Rows.Add(row);
            }

            return table;
        }
    }
}

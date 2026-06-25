
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NToastNotify;
using NToastNotify.Helpers;
using NuGet.Common;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using StackExchange.Redis;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Management;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static SL_Bullion.WebAPI.bullionController;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SL_Bullion.Constant
{
    public class ApplicationConstant
    {
        private readonly BullionDbContext _context;
        private readonly IConfiguration _config;
        private readonly IConfiguration _configuration;
        private static readonly HttpClient client = new HttpClient();
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
        ResponseBody _response = new ResponseBody();
        string adminNodeUrl = "", pendingOrderNodeUrl = "", rateDiffNodeUrl = "", deletePONodeUrl = "", secretkey = "StarlineSolution";
        public ApplicationConstant(BullionDbContext context, IConfiguration config,  IConfiguration configuration)
        {
            _context = context;
            _config = config;
            _configuration = configuration;
            adminNodeUrl = config.GetSection("adminNodeUrl").Value;
            pendingOrderNodeUrl = config.GetSection("pendingOrderNodeUrl").Value;
            rateDiffNodeUrl = config.GetSection("rateDiffNodeUrl").Value;
            deletePONodeUrl = config.GetSection("deletePONodeUrl").Value;
        }
        private void setValueRedis(string key, string value)
        {
            IDatabase _db = redis.GetDatabase();
            _db.StringSet(key, value);
        }
        internal void setSymbolRedis()
        {
            var result = _context.tblSymbol
                    .Where(s => s.isView == true || s.isTerminal == true).OrderBy(s => s.clientId).ThenBy(s => s.index)
                    .Select(s => new
                    {
                        id = s.id,
                        user = _context.tblMaster.Where(m => m.id == s.clientId).Select(m => m.userName).FirstOrDefault(),
                        name = s.name,
                        source = s.source,
                        sourceType = s.sourceType,
                        rateType = s.rateType,
                        buyPremium = s.buyPremium,
                        sellPremium = s.sellPremium,
                        division = s.division,
                        multiply = s.multiply,
                        gst = s.gst,
                        buyCommonPremium = s.buyCommonPremium,
                        sellCommonPremium = s.sellCommonPremium,
                        digit = s.digit,
                        isView = s.isView,
                        isTerminal = s.isTerminal,
                        isComment = s.isComment,
                        ind = s.index,
                        identifier = s.identifier,
                        high = s.high,
                        cityId = s.CityId,
                        low = s.low,
                        symbolType = s.symbolType
                    }).ToList();

            string jsonString = JsonSerializer.Serialize(result);
            setValueRedis("symbolDetails", jsonString);
        }
        internal void setCoinSymbolRedis()
        {
            var result = _context.tblCoin
                    .Where(s => s.isView == true).OrderBy(s => s.clientId).ThenBy(s => s.index)
                    .Select(s => new
                    {
                        id = s.id,
                        user = _context.tblMaster.Where(m => m.id == s.clientId).Select(m => m.userName).FirstOrDefault(),
                        name = s.name,
                        source = s.source,
                        rateType = s.rateType,
                        buyPremium = s.buyPremium,
                        sellPremium = s.sellPremium,
                        division = s.division,
                        multiply = s.multiply,
                        buyCommonPremium = s.buyCommonPremium,
                        sellCommonPremium = s.sellCommonPremium,
                        coinId = s.coinId,
                        gst = s.gst,
                        url = s.url
                    })
                    .ToList();

            string jsonString = JsonSerializer.Serialize(result);
            setValueRedis("coinSymbolDetails", jsonString);
        }
        internal void setBankRateRedis()
        {
            var result = _context.tblBankRate
                    .Select(s => new
                    {
                        id = s.id,
                        user = _context.tblMaster.Where(m => m.id == s.clientId).Select(m => m.userName).FirstOrDefault(),
                        premiumGold = s.premiumGold,
                        premiumSilver = s.premiumSilver,
                        spotTypeGold = s.spotTypeGold,
                        spotTypeSilver = s.spotTypeSilver,
                        interBankGold = s.interBankGold,
                        interBankSilver = s.interBankSilver,
                        conversionGold = s.conversionGold,
                        conversionSilver = s.conversionSilver,
                        customDutyGold = s.customDutyGold,
                        customDutySilver = s.customDutySilver,
                        marginGold = s.marginGold,
                        marginSilver = s.marginSilver,
                        gstGold = s.gstGold,
                        gstSilver = s.gstSilver,
                        divisionGold = s.divisionGold,
                        divisionSilver = s.divisionSilver,
                        multiplyGold = s.multiplyGold,
                        multiplySilver = s.multiplySilver
                    })
                    .ToList();

            string jsonString = JsonSerializer.Serialize(result);
            setValueRedis("bankRateDetails", jsonString);
        }
        internal void setCoinBankRateRedis()
        {
            var result = _context.tblCoinBank
                    .Select(s => new
                    {
                        id = s.id,
                        user = _context.tblMaster.Where(m => m.id == s.clientId).Select(m => m.userName).FirstOrDefault(),
                        premiumGold = s.premiumGold,
                        premiumSilver = s.premiumSilver,
                        spotTypeGold = s.spotTypeGold,
                        spotTypeSilver = s.spotTypeSilver,
                        interBankGold = s.interBankGold,
                        interBankSilver = s.interBankSilver,
                        conversionGold = s.conversionGold,
                        conversionSilver = s.conversionSilver,
                        customDutyGold = s.customDutyGold,
                        customDutySilver = s.customDutySilver,
                        marginGold = s.marginGold,
                        marginSilver = s.marginSilver,
                        gstGold = s.gstGold,
                        gstSilver = s.gstSilver,
                        divisionGold = s.divisionGold,
                        divisionSilver = s.divisionSilver,
                        multiplyGold = s.multiplyGold,
                        multiplySilver = s.multiplySilver
                    })
                    .ToList();

            string jsonString = JsonSerializer.Serialize(result);
            setValueRedis("coinBankRateDetails", jsonString);
        }
        internal void setUserRedis()
        {
            var result = _context.tblMaster.Where(s => s.isActive == true)
                    .Select(s => new
                    {
                        user = s.userName
                    }).ToList();
            string jsonString = JsonSerializer.Serialize(result);
            setValueRedis("userDetails", jsonString);
            var resultCoin = _context.tblMaster.Where(s => s.isActive == true && s.isCoin == true)
                   .Select(s => new
                   {
                       user = s.userName
                   }).ToList();
            jsonString = JsonSerializer.Serialize(resultCoin);
            setValueRedis("userCoinDetails", jsonString);
        }


        internal void setCityRedis()
        {
            var result = _context.tblCity
                    .Where(c => c.Id == c.Id).OrderBy(c => c.ClientId)
                    .Select(c => new
                    {
                        id = c.Id,
                        user = _context.tblMaster.Where(m => m.id == c.ClientId).Select(m => m.userName).FirstOrDefault(),
                        name = c.Name,
                    })
                    .ToList();

            string jsonString = JsonSerializer.Serialize(result);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = client.PostAsync(adminNodeUrl + "/cityDetails", content);
        }

        internal void pushPendingOrderDeleteSchedule()
        {
            var result = _context.tblContact
                    .Select(c => new
                    {
                        clientId = c.clientId,
                        deleteTime = c.plDelete
                    })
                    .ToList();

            string jsonString = JsonSerializer.Serialize(result);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = client.PostAsync(deletePONodeUrl + "/pendingOrderDeleteSchedule", content);
        }
        internal void pushRateDifferance()
        {
            var result = _context.tblContact
                    .Select(s => new
                    {
                        user = _context.tblMaster.Where(m => m.id == s.clientId).Select(m => m.userName).FirstOrDefault(),
                        premiumGold = s.goldDifferance,
                        premiumSilver = s.silverDifferance
                    })
                    .ToList();

            string jsonString = JsonSerializer.Serialize(result);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = client.PostAsync(rateDiffNodeUrl + "/rateDiffNotification", content);
        }


        internal void pushContactDetails(int clientId)
        {
            string activeUser = _context.tblMaster.Where(m => m.id == clientId).Select(m => m.userName).FirstOrDefault();
            var result = _context.tblContact
                      .Select(s => new
                      {
                          activeUser = _context.tblMaster.Where(m => m.id == clientId).Select(m => m.userName).FirstOrDefault(),
                          user = _context.tblMaster.Where(m => m.id == s.clientId).Select(m => m.userName).FirstOrDefault(),
                          number1 = s.number1,
                          number2 = s.number2,
                          number3 = s.number3,
                          number4 = s.number4,
                          number5 = s.number5,
                          number6 = s.number6,
                          number7 = s.number7,
                          marqueeTop = s.marqueeTop,
                          whatsAppNo = s.whatsAppNo,
                          marqueeBottom = s.marqueeBottom,
                          address1 = s.address1,
                          address2 = s.address2,
                          address3 = s.address3,
                          email1 = s.email1,
                          email2 = s.email2,
                          bannerWeb = s.bannerWeb,
                          bannerApp = s.bannerApp,
                          isBuy = s.isBuy,
                          isSell = s.isSell,
                          isHigh = s.isHigh,
                          isLow = s.isLow,
                          isRate = s.isRate,
                          isLogin = s.isLogin,
                          isCoin = s.isCoinRate,
                          isTrade = s.isTrade,
                          av = _context.tblMaster.Where(m => m.id == clientId).Select(m => m.versionAndroid).FirstOrDefault(),
                          iv = _context.tblMaster.Where(m => m.id == clientId).Select(m => m.versionIos).FirstOrDefault(),
                          goldCoinHeader = s.goldCoinHeader,
                          silverCoinHeader = s.silverCoinHeader,
                          isGoldCoinHeader = s.isGoldCoinHeader,
                          isSilverCoinHeader = s.isSilverCoinHeader
                      }).ToList();

            string jsonString = JsonSerializer.Serialize(result);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            content.Headers.Add("activeUser", activeUser);
            var response = client.PostAsync(adminNodeUrl + "/contactDetails", content);
        }

        internal void pushReferanceSymbol(int clientId)
        {
            string activeUser = _context.tblMaster.Where(m => m.id == clientId).Select(m => m.userName).FirstOrDefault();
            var result = _context.tblReferanceSymbol
                     .Where(s => s.isMaster == true && s.isView == true)
                     .Select(s => new
                     {
                         user = _context.tblMaster.Where(m => m.id == s.clientId).Select(m => m.userName).FirstOrDefault(),
                         name = s.name,
                         source = s.source
                     })
                     .ToList();
            string jsonString = JsonSerializer.Serialize(result);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            content.Headers.Add("activeUser", activeUser);
            var response = client.PostAsync(adminNodeUrl + "/referanceDetails", content);
        }
        internal void pushAccountDetails(int accountId, int groupId)
        {
            var activeUser = _context.tblAccount
                     .Where(a => a.id == accountId)
                     .Select(a => new
                     {
                         id = a.id,
                         gId = a.groupId,
                         sd = a.startDate,
                         ed = a.endDate,
                         rd = a.endDate.HasValue
                        ? Math.Max(0, (a.endDate.Value.Date - DateTime.Today).Days + 1)
                        : 0,
                         isA = a.isActive
                     }).ToJson();
            var result = _context.tblAccount
                     .Where(a => a.isRegister == false && a.isActive == true)
                     .Select(a => new
                     {
                         id = a.id,
                         gId = a.groupId,
                         sd = a.startDate,
                         ed = a.endDate,
                         rd = a.endDate.HasValue
                        ? Math.Max(0, (a.endDate.Value.Date - DateTime.Today).Days + 1)
                        : 0,
                         isA = a.isActive
                     }).ToList();
            string jsonString = JsonSerializer.Serialize(result);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            content.Headers.Add("activeUser", activeUser);
            content.Headers.Add("groupId", groupId.ToString());
            content.Headers.Add("accountId", accountId.ToString());
            var response = client.PostAsync(adminNodeUrl + "/accountDetails", content);
        }
        internal void pushGroupDetails(int groupId)
        {
            var query = from gs in _context.tblGroupSymbol
                        join g in _context.tblGroup on gs.groupId equals g.id
                        where gs.isView == true
                        select new
                        {
                            gs.groupId,
                            gs.symbolId,
                            gs.buyPremium,
                            gs.sellPremium,
                            gs.oneClick,
                            gs.inTotal,
                            gs.step,
                            g.buyPremiumGold,
                            g.sellPremiumGold,
                            g.buyPremiumSilver,
                            g.sellPremiumSilver,
                            symbolName = _context.tblSymbol.Where(s => s.id == gs.symbolId).Select(s => s.name).FirstOrDefault()
                        };
            var result = query.ToList();
            string jsonString = JsonSerializer.Serialize(result);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            content.Headers.Add("activeUser", groupId.ToString());
            var response = client.PostAsync(adminNodeUrl + "/groupDetails", content);
        }
        internal int getClientId(string user)
        {
            var clientId = _context.tblMaster.Where(s => s.userName == user).Select(c => c.id).FirstOrDefault();
            return clientId;
        }
        internal string encrypt(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(secretkey);
                aesAlg.IV = new byte[16];

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
        internal string decrypt(string cipherText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(secretkey);
                aesAlg.IV = new byte[16];

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        internal async Task<string> generateToken(string user, string loginId, int accountId, int groupId)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim("user",user),
                new Claim("loginId",loginId),
                new Claim("accountId",accountId.ToString()),
                new Claim("groupId",groupId.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(_config["JWT:issuer"], _config["JWT:audience"], claims, expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["JWT:expires"])), signingCredentials: signIn);
            string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenValue;
        }

        internal async Task<string> GenerateOtp()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var randomNumber = new byte[4];
                rng.GetBytes(randomNumber);
                int otp = Math.Abs(BitConverter.ToInt32(randomNumber, 0)) % 9000 + 1000;
                return otp.ToString();
            }
        }



        internal void pushPendingOrder()
        {
            var result = _context.tblOpenOrder.Where(o => o.tradeType == 3 || o.tradeType == 4)
                        .Select(o => new
                        {
                            o.symbolId,
                            o.tradeType,
                            o.id,
                            user = _context.tblMaster.Where(m => m.id == o.clientId).Select(m => m.userName).FirstOrDefault(),
                            rate = o.rate - ((_context.tblGroup.Where(g => g.id == _context.tblAccount.Where(a => a.loginId == o.loginId && a.clientId == o.clientId).Select(a => a.groupId).FirstOrDefault())
                            .Select(g => o.tradeType == 3 ? (o.source == "gold" ? g.sellPremiumGold : g.sellPremiumSilver) : (o.source == "gold" ? g.buyPremiumGold : g.buyPremiumSilver)).FirstOrDefault()) +
                            (_context.tblGroupSymbol.Where(gs => gs.groupId == _context.tblAccount.Where(a => a.loginId == o.loginId && a.clientId == o.clientId).Select(a => a.groupId).FirstOrDefault() && gs.symbolId == o.symbolId)
                            .Select(gs => o.tradeType == 3 ? gs.sellPremium : gs.buyPremium).FirstOrDefault()))

                        }).ToList();
            string jsonString = JsonSerializer.Serialize(result);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = client.PostAsync(pendingOrderNodeUrl + "/pendingOrder", content);
        }


        internal decimal getTotalRate(string source, double rate, double multiply, double division, double volume)
        {
            decimal total = 0;
            if (source == "gold")
            {
                total = Convert.ToDecimal((rate / ((multiply / division) * 10)) * volume);
            }
            else if (source == "silver")
            {
                total = Convert.ToDecimal((rate / (multiply / division)) * volume);
            }
            else
            {
                total = Convert.ToDecimal(rate * volume);
            }

            return total;
        }

        internal decimal getTaxRate(decimal total, int symbolId, int clientId, string loginId)
        {
            decimal tax = 0;
            double taxType = 0;
            var accountType = _context.tblAccount.Where(a => a.clientId == clientId && a.loginId == loginId).Select(a => a.type).FirstOrDefault();

            double gstBill = _context.tblSymbol.Where(s => s.id == symbolId && s.isBill == true).Select(a => a.gstBill).FirstOrDefault();
            decimal gstAmount = Math.Round((total * Convert.ToDecimal(gstBill)) / 100, 2);

            if (accountType == "tds")
            {
                taxType = _context.tblSymbol.Where(s => s.id == symbolId && s.isBill == true).Select(a => a.tdsBill).FirstOrDefault();
                decimal tdsAmount = Math.Round((total * Convert.ToDecimal(taxType)) / 100, 2);
                tax = total + (gstAmount - tdsAmount);
            }
            else if (accountType == "tcs")
            {
                taxType = _context.tblSymbol.Where(s => s.id == symbolId && s.isBill == true).Select(a => a.tcsBill).FirstOrDefault();
                decimal tcsAmount = Math.Round((total * Convert.ToDecimal(taxType)) / 100, 2);
                tax = total + (gstAmount + tcsAmount);
            }
            else
            {
                tax = total + gstAmount;
            }

            return tax;
        }

        internal void pushOrderAlert(string action, string user, int accountId, int actionType, int actionId)
        {
            string message = "";
            bool isAlert = false;
            int clientId = getClientId(user);

            if (!string.IsNullOrEmpty(action))
            {
                var order = GetOrderDetails(action, clientId, actionId);

                if (order != null)
                {
                    string unit = string.Equals(order.source, "gold", StringComparison.OrdinalIgnoreCase) ? "gm" : "Kg";
                    message = $"OrderNo {actionId}: {order.tradeTypeView} {order.symbolName}, {order.volume}{unit} at {order.rate} {order.tax}. Placed by {order.name} from IP {order.ip}.";
                    isAlert = true;
                }
            }

            var obj = new
            {
                action = action,
                user = user,
                accountId = accountId,
                message = message,
                isAlert = isAlert
            };
            string jsonString = JsonSerializer.Serialize(obj);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = client.PostAsync(adminNodeUrl + "/orderDetails", content);
        }
        OpenOrder GetOrderDetails(string action, int clientId, int actionId)
        {
            var query = from o in _context.tblOpenOrder
                        join a in _context.tblAccount
                            on new { o.clientId, o.loginId }
                            equals new { a.clientId, a.loginId } into ac
                        from a in ac.DefaultIfEmpty()
                        where o.clientId == clientId && o.dealNo == actionId
                        select new
                        {
                            o.tradeType,
                            o.isLimit,
                            o.volume,
                            o.rate,
                            o.tax,
                            o.ip,
                            o.symbolName,
                            o.source,
                            Name = a != null ? a.name : null,
                            Firm = a != null ? a.firmName : null
                        };

            var result = query.FirstOrDefaultAsync().Result;

            if (result == null) return null;

            return new OpenOrder
            {
                name = result.Name,
                firm = result.Firm,
                symbolName = result.symbolName,
                tradeTypeView = GetTradeTypeView(action, result.tradeType, result.isLimit),
                volume = result.volume,
                rate = result.rate,
                tax = result.tax,
                ip = result.ip,
                source = result.source
            };
        }
        static string GetTradeTypeView(string action, int tradeType, bool isLimit)
        {
            action = action.ToLowerInvariant();
            return action switch
            {
                "open" or "pass" => tradeType switch
                {
                    1 when !isLimit => "Buy",
                    1 when isLimit => "PassBuyLimit",
                    2 when !isLimit => "Sell",
                    2 when isLimit => "PassSellLimit",
                    3 => "PlaceBuyLimit",
                    4 => "PlaceSellLimit",
                    _ => "Buy"
                },
                "update" => tradeType switch
                {
                    1 when !isLimit => "Buy",
                    1 when isLimit => "PassBuyLimit",
                    2 when !isLimit => "Sell",
                    2 when isLimit => "PassSellLimit",
                    3 => "BuyLimit Updated",
                    4 => "SellLimit Updated",
                    _ => "Buy"
                },
                "delete" => tradeType switch
                {
                    1 when !isLimit => "Buy",
                    1 when isLimit => "BuyLimit",
                    2 when !isLimit => "Sell",
                    2 when isLimit => "PassSellLimit",
                    3 => "BuyLimit delete",
                    4 => "SellLimit delete",
                    _ => "Buy"
                },
                _ => "Buy"
            };
        }

        internal void pushAlert(int clientId, string title, string message, string bit)
        {
            var _alert = new alertBody();
            var result = _context.tblMaster.Where(m => m.id == clientId).Select(m => new { activeUser = m.userName }).FirstOrDefault();
            _alert.user = result.activeUser;
            _alert.title = title;
            _alert.message = message;
            _alert.bit = bit;
            string jsonString = JsonSerializer.Serialize(_alert);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = client.PostAsync(adminNodeUrl + "/alertDetails", content);
            response = client.PostAsync(rateDiffNodeUrl + "/updateNotification", content);
        }
        internal byte[] generateExcelFromList<T>(List<T> table, string sheetName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(sheetName);
                worksheet.Cells["A1"].LoadFromCollection(table, true);
                worksheet.Cells[worksheet.Dimension?.Address].AutoFitColumns();
                using (var headerRange = worksheet.Cells["A1:XFD1"]) // XFD is the maximum column in Excel
                {
                    headerRange.Style.Font.Bold = true; // Apply bold styling
                }
                return package.GetAsByteArray();
            }
        }

        internal async Task<ResponseBody> sendOrderToMeta(string request, string user)
        {
            try
            {
                var content = new StringContent(request, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_config["hedge:" + user + ":baseUrl"] + "placeOrderMeta", content);
                response.EnsureSuccessStatusCode();
                _response.data = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _response.code = 400;
                _response.message = ex.Message;
            }
            return _response;

        }

        internal async Task<ResponseBody> sendOrderToSmbullion(string request, string user)
        {
            try
            {
                var baseUrl = _config[$"hedge:{user}:baseUrl"];
                if (!baseUrl.Contains("?op=InsertOpenOrderDetailWithRegID"))
                {
                    baseUrl = $"{baseUrl}?op=InsertOpenOrderDetailWithRegID";
                }
                string soapAction = "http://tempuri.org/InsertOpenOrderDetailWithRegID";

                string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
                           xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
                           xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
              <soap:Body>
                <InsertOpenOrderDetailWithRegID xmlns=""http://tempuri.org/"">
                  <ObjOrder>{request}</ObjOrder>
                </InsertOpenOrderDetailWithRegID>
              </soap:Body>
            </soap:Envelope>";

                var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
                content.Headers.ContentType.CharSet = "utf-8";
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("SOAPAction", soapAction);
                var response = await client.PostAsync(baseUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed with status code: {(int)response.StatusCode}, Response: {responseBody}");
                }

                return new ResponseBody
                {
                    data = responseBody,
                    code = 200,
                    message = "Success"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SOAP Error: {ex.Message}");
                return new ResponseBody
                {
                    code = 500,
                    message = $"Error: {ex.Message}"
                };
            }
        }

        internal async Task<ClaimType> DecodeToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claimType = new ClaimType();
            // Header
            var alg = jwtToken.Header.Alg;
            Console.WriteLine($"Algorithm: {alg}");

            // Payload (claims)
            foreach (var claim in jwtToken.Claims)
            {
                claimType.user = jwtToken.Claims.FirstOrDefault(c => c.Type == "user")?.Value;
                claimType.loginId = jwtToken.Claims.FirstOrDefault(c => c.Type == "loginId")?.Value;
                claimType.accountId = jwtToken.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
                claimType.groupId = jwtToken.Claims.FirstOrDefault(c => c.Type == "groupId")?.Value;
                //claimTypes.Add(claimType);
                //Console.WriteLine($"{claim.Type}: {claim.Value}");
            }

            return claimType;
        }
        internal void pushCoinOrderAlert(string action, string user, int accountId, int actionType, int actionId)
        {
            string message = "";
            bool isAlert = false;
            int clientId = getClientId(user);

            if (!string.IsNullOrEmpty(action))
            {
                var order = GetCoinOrderDetails(action, clientId, actionId);

                if (order != null)
                {
                    string unit = string.Equals(order.Source, "gold", StringComparison.OrdinalIgnoreCase) ? "gm" : "Kg";
                    message = $"OrderNo {actionId}: {order.TradeType} {order.SymbolName}, {order.Volume}{unit} at {order.Rate} {order.Tax}. Placed by {order.Name} from IP {order.IP}.";
                    isAlert = true;
                }
            }

            var obj = new
            {
                action = action,
                user = user,
                accountId = accountId,
                message = message,
                isAlert = isAlert
            };
            string jsonString = JsonSerializer.Serialize(obj);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = client.PostAsync(adminNodeUrl + "/orderDetails", content);
        }
        OpenOrderCoinDto GetCoinOrderDetails(string action, int clientId, int actionId)
        {
            var query = from o in _context.tblOpenOrderCoin
                        join a in _context.tblAccount
                            on new { o.ClientId, o.LoginID }
                            equals new { ClientId = a.clientId, LoginID = a.loginId } into ac
                        from a in ac.DefaultIfEmpty()
                        where o.ClientId == clientId && o.DealNo == actionId
                        select new
                        {
                            o.TradeType,
                            //o.isLimit,
                            isLimit = false,
                            o.Volume,
                            o.Rate,
                            //o.tax,
                            tax = 0,
                            o.IP,
                            o.SymbolName,
                            o.Source,
                            Name = a != null ? a.name : null,
                            Firm = a != null ? a.firmName : null
                        };

            var result = query.FirstOrDefaultAsync().Result;

            if (result == null) return null;

            return new OpenOrderCoinDto
            {
                Name = result.Name,
                FirmName = result.Firm,
                SymbolName = result.SymbolName,
                TradeType = GetTradeTypeView(action, int.Parse(result.TradeType), result.isLimit),
                Volume = result.Volume,
                Rate = result.Rate,
                Tax = result.tax,
                IP = result.IP,
                Source = result.Source
            };
        }


        internal async Task<ResponseBody> verifyCoinDetails(string user, string loginId, int tradeType, double volume, int symbolId, int orderNo)
        {
            try
            {
                int clientId = getClientId(user);
                var coinConfig = _configuration.GetSection($"coin:{user}");
                var token = _configuration.GetValue<string>($"coin:{user}:token");
                string TradeFrom = user;
                object coinRequest= "";

                if (user == "shreeaurum")
                {
                    coinRequest = new
                    {
                        SymbolId = symbolId.ToString(),
                        Token = token,
                        Quantity = volume.ToString(CultureInfo.InvariantCulture),
                        TradeFrom = TradeFrom,
                        TradeType = tradeType.ToString(CultureInfo.InvariantCulture),
                        DeviceToken = "",
                        BuyLimitPrice = "",
                        SellLimitPrice = ""
                    };
                }

                var logRequest = new
                {
                    UserName = user,
                    loginId = loginId,
                    DealNo = orderNo,
                    SymbolId = symbolId.ToString(),
                    Quantity = volume.ToString(CultureInfo.InvariantCulture),
                    TradeFrom = TradeFrom,
                    TradeType = tradeType.ToString(CultureInfo.InvariantCulture),
                    DeviceToken = "",
                    BuyLimitPrice = "",
                    SellLimitPrice = ""
                };

                string requestJson = System.Text.Json.JsonSerializer.Serialize(coinRequest);
                string requestLogJson = System.Text.Json.JsonSerializer.Serialize(logRequest);



                var response = await sendCoinOrderToAdmin(requestJson, user);



                if (TryExtractJsonFromSoap(response.data, out string jsonResult))
                {
                    var jsonObj = jsonResult;

                    addCoinData(clientId, requestLogJson, jsonResult.ToString(), user);

                    //addCoinData(clientId, requestJson, jsonResult.ToString());

                    //await _context.tblCoinInfo.AddAsync(new CoinInfo
                    //{
                    //    Request = requestLogJson,
                    //    Response = jsonObj.ToString(),
                    //    CreateDate = DateTime.Now,
                    //    ClientId = clientId,
                    //    UserName = user
                    //});
                }
                await _context.SaveChangesAsync();

                if (response.code == 200)
                {
            
                }
                else
                {
                    return FailResponse("Trade Not Execute. Please Try Again");
                }
            }

            catch (Exception ex)
            {
               
                return FailResponse("Trade Not Execute. Please Try Again");
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

        private void addCoinData(int clientId, string request, string response, string user)
        {
            var coinInfo = new CoinInfo();
            coinInfo.ClientId = clientId;
            coinInfo.Request = request;
            coinInfo.Response = response;
            coinInfo.UserName = user;
            _context.Add(coinInfo);
            _context.SaveChanges();
            int totalRecords = _context.tblCoinInfo.Where(h => h.ClientId == clientId).Count();

            if (totalRecords > 1000)
            {
                var recordsToDelete = _context.tblCoinInfo.OrderBy(h => h.Id).Take(totalRecords - 100).ToList();

                _context.tblCoinInfo.RemoveRange(recordsToDelete);
                _context.SaveChangesAsync();
            }
        }

        internal async Task<ResponseBody> sendOrderToAdmin(string request, string user)
        {
            try
            {
                var baseUrl = _config[$"hedge:{user}:baseUrl"];
                if (!baseUrl.Contains("?op=InsertOpenOrderDetailWithRegID"))
                {
                    baseUrl = $"{baseUrl}?op=InsertOpenOrderDetailWithRegID";
                }
                string soapAction = "http://tempuri.org/InsertOpenOrderDetailWithRegID";

                string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                                    <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
                                                   xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
                                                   xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                                      <soap:Body>
                                        <InsertOpenOrderDetailWithRegID xmlns=""http://tempuri.org/"">
                                          <ObjOrder>{request}</ObjOrder>
                                        </InsertOpenOrderDetailWithRegID>
                                      </soap:Body>
                                    </soap:Envelope>";

                var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
                content.Headers.ContentType.CharSet = "utf-8";
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("SOAPAction", soapAction);
                var response = await client.PostAsync(baseUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed with status code: {(int)response.StatusCode}, Response: {responseBody}");
                }


                return new ResponseBody
                {
                    data = responseBody,
                    code = 200,
                    message = "Success"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SOAP Error: {ex.Message}");
                return new ResponseBody
                {
                    code = 500,
                    message = $"Error: {ex.Message}"
                };
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

                    var resultNode = soapDoc
                        .Descendants()
                        .FirstOrDefault(x => x.Name.LocalName == "InsertCoinOpenOrderDetailWithRegIDResult");

                    if (resultNode != null)
                    {
                        jsonResult = resultNode.Value.Trim();
                        return true;
                    }

                    Console.WriteLine("⚠️ Result node not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error extracting SOAP data: {ex.Message}");
            }

            return false;
        }


        internal async Task<ResponseBody> sendCoinOrderToAdmin(string request, string user)
        {
            try
            {
                var baseUrl = _config[$"hedge:{user}:baseUrl"];
                if (!baseUrl.Contains("?op=InsertCoinOpenOrderDetailWithRegID"))
                {
                    baseUrl = $"{baseUrl}?op=InsertCoinOpenOrderDetailWithRegID";
                }
                string soapAction = "http://tempuri.org/InsertCoinOpenOrderDetailWithRegID";

                string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                                    <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
                                                   xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
                                                   xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                                      <soap:Body>
                                        <InsertCoinOpenOrderDetailWithRegID xmlns=""http://tempuri.org/"">
                                          <ObjOrder>{request}</ObjOrder>
                                        </InsertCoinOpenOrderDetailWithRegID>
                                      </soap:Body>
                                    </soap:Envelope>";

                var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
                content.Headers.ContentType.CharSet = "utf-8";
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("SOAPAction", soapAction);
                var response = await client.PostAsync(baseUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed with status code: {(int)response.StatusCode}, Response: {responseBody}");
                }


                return new ResponseBody
                {
                    data = responseBody,
                    code = 200,
                    message = "Success"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SOAP Error: {ex.Message}");
                return new ResponseBody
                {
                    code = 500,
                    message = $"Error: {ex.Message}"
                };
            }
        }



        internal void pushSingleLoginDetails(int userId)
        {
            var activeUser = _context.tblAccount
                     .Where(c => c.id == userId)
                     .Select(c => new
                     {
                         id = c.id,
                         mac = c.mac,
                         isA = c.isActive,
                         mobile = c.mobile,
                         loginId = c.loginId,
                         rd = c.endDate.HasValue ? Math.Max(0, (c.endDate.Value.Date - DateTime.Today).Days + 1) : 0,
                         user = _context.tblMaster.Where(m => m.id == c.clientId).Select(m => m.userName).FirstOrDefault(),
                     }).ToJson();
            var result = _context.tblAccount
                    .Where(c => c.isActive == true && c.isRegister == false)
                     .Select(c => new
                     {
                         id = c.id,
                         mac = c.mac,
                         isA = c.isActive,
                         user = _context.tblMaster.Where(m => m.id == c.clientId).Select(m => m.userName).FirstOrDefault(),
                         mobile = c.mobile,
                         rd = c.endDate.HasValue? Math.Max(0, (c.endDate.Value.Date - DateTime.Today).Days + 1): 0,
                         loginId = c.loginId
                     }).ToList();
            string jsonString = JsonSerializer.Serialize(result);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            content.Headers.Add("activeUser", activeUser);
            content.Headers.Add("accountId", userId.ToString());
            var response = client.PostAsync(adminNodeUrl + "/singleLoginDetails", content);
        }
    }
}

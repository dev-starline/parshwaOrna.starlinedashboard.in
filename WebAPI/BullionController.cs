using System.Data;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using SL_Bullion.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SL_Bullion.WebAPI
{
    [Route("api/[controller]")]
    [ApiController]
    public class bullionController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly ApplicationConstant _constatnt;
        private readonly ResponseMessage _message;
        ResponseBody _response = new ResponseBody();
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly MessageConstant _messageConstatnt;
        private readonly IConfiguration _configuration;
        private readonly BullionService _bullionService;

        public bullionController(BullionDbContext context, ApplicationConstant constatnt, ResponseMessage message, IWebHostEnvironment webHostEnvironment, MessageConstant messageConstatnt, IConfiguration configuration, BullionService bullionService)
        {
            _context = context;
            _constatnt = constatnt;
            _message = message;
            _webHostEnvironment = webHostEnvironment;
            _messageConstatnt = messageConstatnt;
            _configuration = configuration;
            _bullionService = bullionService;
        }

        public class ResponseBody
        {
            public int code { get; set; } = 200;
            public string? message { get; set; }
            public object? data { get; set; }
        }

        private int getClientId(string user)
        {
            var clientId = _context.tblMaster.Where(s => s.userName == user).Select(c => c.id).FirstOrDefault();
            return clientId;
        }

        private async Task<int> getClient(string user)
        {
            var clientId = await _context.tblMaster.Where(s => s.userName == user).Select(c => c.id).FirstOrDefaultAsync();
            return clientId;
        }

        /// <remarks>
        /// Todo:
        ///user:userName of project,type:android or ios
        /// </remarks>
        //[HttpGet("versionAndroidIos")]
        //public JsonResult version(string user, string type)
        //{
        //    try
        //    {
        //        var data = _context.tblMaster.Where(s => s.userName == user).Select(c => new
        //        {
        //            version = (type == "android") ? c.versionAndroid : c.versionIos
        //        }).ToList();
        //        if (data.Count > 0)
        //        {
        //            _response.data = data;
        //        }
        //        else
        //        {
        //            _response.code = 400;
        //            _response.message = _message.C101;
        //        }

        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //    return Json(_response);
        //}

        [HttpGet("bankDetails")]
        public JsonResult bank(string user)
        {
            try
            {
                int clientId = getClientId(user);
                var data = _context.tblBank.Where(b => b.clientId == clientId).ToList();
                if (data.Count > 0)
                {
                    _response.data = data;
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

        [HttpGet("updateDetails")]
        public JsonResult update(string user, string fromDate, string toDate)
        {
            try
            {
                int clientId = getClientId(user);
                DateTime fromDateValue;
                DateTime toDateValue;
                string dateFormat = "dd/MM/yyyy";
                if (!DateTime.TryParseExact(fromDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDateValue) ||
                !DateTime.TryParseExact(toDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out toDateValue))
                {
                    _response.code = 400;
                    _response.message = _message.C103;
                    return Json(_response);
                }
                toDateValue = toDateValue.Date.Add(new TimeSpan(23, 59, 59));
                var data = _context.tblUpdate.Where(b => b.clientId == clientId && b.modifiedDate >= fromDateValue && b.modifiedDate <= toDateValue).OrderByDescending(s => s.modifiedDate).ToList();
                if (data.Count > 0)
                {
                    _response.data = data;
                }
                else
                {
                    _response.code = 400;
                    _response.message = _message.C104;
                }

            }
            catch (Exception)
            {
                throw;
            }

            return Json(_response);
        }

        [HttpPost("feedbackDetails")]
        public JsonResult feedback([FromBody] JsonObject obj)
        {
            try
            {
                int clientId = getClientId(obj["user"].ToString());
                Feedback feedback = new Feedback();
                feedback.clientId = clientId;
                feedback.name = obj["name"].ToString();
                feedback.mobile = obj["mobile"].ToString();
                feedback.email = obj["email"].ToString();
                feedback.subject = obj["subject"].ToString();
                feedback.message = obj["message"].ToString();
                if (ModelState.IsValid)
                {
                    _context.Add(feedback);
                    _context.SaveChanges();
                    _response.message = _message.C102;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return Json(_response);
        }
        [HttpPost("kycDetails")]
        public async Task<IActionResult> kyc([FromForm] Kyc kyc)
        {
            int clientId = getClientId(kyc.user);
            string uploads = Path.Combine(_webHostEnvironment.WebRootPath, "images\\kyc\\", clientId.ToString(), kyc.mobile);
            var zipFileName = Path.Combine(uploads, kyc.name + "_" + kyc.mobile + "_kyc.zip");
            string url = Path.Combine($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}", "images\\kyc\\", clientId.ToString(), kyc.mobile, kyc.name + "_" + kyc.mobile + "_kyc.zip");
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            if (kyc.Files != null)
            {
                if (kyc.Files.Count > 0)
                {
                    using (var zipToCreate = new FileStream(zipFileName, FileMode.Create))
                    {
                        using (var archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
                        {
                            foreach (var file in kyc.Files)
                            {
                                var entry = archive.CreateEntry(file.FileName);

                                using (var entryStream = entry.Open())
                                using (var fileStream = file.OpenReadStream())
                                {
                                    await fileStream.CopyToAsync(entryStream);
                                }
                            }
                        }
                    }
                }
            }
            kyc.url = url;
            kyc.clientId = clientId;

            if (ModelState.IsValid)
            {
                if (_context.tblKyc.Any(k => k.mobile == kyc.mobile))
                {
                    var existingData = await _context.tblKyc.FirstOrDefaultAsync(k => k.mobile == kyc.mobile);
                    existingData.url = kyc.url;
                    _context.Update(existingData);
                }
                else
                {
                    _context.Add(kyc);
                }
                await _context.SaveChangesAsync();
                _response.message = _message.C106;
            }
            return Ok(_response);
        }

        [HttpPost("otrDetails")]
        public async Task<IActionResult> otr([FromBody] JsonObject obj)
        {
            try
            {
                int clientId = await getClient(obj["user"].ToString());
                string user = obj["user"].ToString();
                string name = obj["name"].ToString();
                string mobile = obj["mobile"].ToString();
                string firmname = obj["firmname"].ToString();
                string city = obj["city"].ToString();
                string ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                string otp = GenerateOtp();
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UpsertOtr", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ClientId", clientId);
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Mobile", mobile);
                        cmd.Parameters.AddWithValue("@Firmname", firmname);
                        cmd.Parameters.AddWithValue("@City", city);
                        cmd.Parameters.AddWithValue("@Ip", (object?)ip ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@otp", otp);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                _messageConstatnt.pushMessageAlertOtr("loginotp", user, clientId, mobile);
                _response.message = _message.C129;
            }
            catch (Exception)
            {
                throw;
            }
            return Json(_response);
        }

        private static string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(1000, 9999).ToString();
        }

        /// <remarks>
        /// Sample request:
        ///
        ///     {"user": "","mobile": "","otp":""}
        ///
        /// </remarks>

        [HttpPost("verifyOTP")]
        public async Task<IActionResult> VerifyOTP([FromBody] JsonObject obj)
        {
            var response = new { isSuccess = false, message = string.Empty };

            try
            {
                int clientId = await getClient(obj["user"]?.ToString() ?? throw new ArgumentException("User is required"));
                string mobile = obj["mobile"]?.ToString();
                string otp = obj["otp"]?.ToString();
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("VerifyOtrOtp", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ClientId", clientId);
                        cmd.Parameters.AddWithValue("@Mobile", mobile);
                        cmd.Parameters.AddWithValue("@otp", otp);

                        await conn.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string code = reader["code"].ToString();
                                if (code == "200")
                                {
                                    _response.message = _message.C130;
                                }
                                else
                                {
                                    _response.code = 400;
                                    _response.message = _message.C131;
                                }
                            }
                            else
                            {
                                response = new
                                {
                                    isSuccess = false,
                                    message = "No response from stored procedure"
                                };
                            }
                        }
                    }
                }

                return Ok(_response);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("category")]
        public async Task<IActionResult> category(string user)
        {
            try
            {
                var result = await _bullionService.getCategoryDetails(user);
                _response.data = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.message = ex.Message;
                return Ok(_response);
            }
        }

        [HttpGet("subCategory")]
        public async Task<IActionResult> subCategory(string user)
        {
            try
            {
                var result = await _bullionService.getSubCategoryDetails(user);
                _response.data = result;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.message = ex.Message;
                return Ok(_response);
            }
        }

        [HttpGet("getJewelleryByUser")]
        public async Task<IActionResult> getJewelleryByUser(string user)
        {
            try
            {
                var result = await _bullionService.getJewelleryByUser(user);
                _response.data = result;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.message = ex.Message;
                return Ok(_response);
            }
        }

        [HttpGet("jewellery")]
        public async Task<IActionResult> jewellery(string user, int categoryId, int subCategoryId)
        {
            try
            {
                var result = await _bullionService.getJewelleryDetails(user, categoryId, subCategoryId);
                _response.data = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.message = ex.Message;
                return Ok(_response);
            }
        }

        /// <remarks>
        /// Sample request:
        ///
        ///     {"user": ""}
        ///
        /// </remarks>
        /// 
        [HttpGet("getSlider")]
        public async Task<IActionResult> GetSlider(string user)
        {

            try
            {
                var result = await _bullionService.getSliderDetails(user);
                _response.data = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.message = ex.Message;
                return Ok(_response);
            }
        }
    }
}

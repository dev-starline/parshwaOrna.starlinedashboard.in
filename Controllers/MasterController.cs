using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using NToastNotify;
using SL_Bullion.Constant;
using System.Text.Json.Nodes;

namespace SL_Bullion.Controllers
{
    public class MasterController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        private readonly IConfiguration _configuration;
        private readonly ApplicationConstant _constatnt;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public MasterController(BullionDbContext context, IToastNotification alert, IConfiguration configuration, ApplicationConstant constatnt, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _alert = alert;
            _configuration = configuration;
            _constatnt = constatnt;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("role");
            return RedirectToAction("Login", "Master");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("userName,password")] MasterLogin master)
        {
            if (master.userName == null || master.password == null)
            {
                _alert.AddErrorToastMessage("userName or password is Null.");
            }

            var data = await _context.tblMasterLogin.FirstOrDefaultAsync(m => m.userName == master.userName && m.password == master.password);
            if (data != null)
            {
                HttpContext.Session.SetString("role", "master");
                return RedirectToAction("Client", "Master");
            }
            else
            {
                _alert.AddErrorToastMessage("userName or password is incorrect.");
            }

            return RedirectToAction("Login", "Master");
        }

        public async Task<IActionResult> Client()
        {
            return View(await _context.tblMaster.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("userName,password,firmName,clientName,mobile,city")] Master master)
        {
            if (ModelState.IsValid)
            {
                var data = _context.Add(master);
                await _context.SaveChangesAsync();
                var id = data.Entity.id;

                var dataArray = _configuration.GetSection("referanceSymbol").Get<List<objArray>>();
                foreach (var item in dataArray)
                {
                    ReferanceSymbol symbol = new ReferanceSymbol();
                    symbol.clientId = id;
                    symbol.name = item.name;
                    symbol.source = item.source;

                    _context.Add(symbol);
                    await _context.SaveChangesAsync();
                }
                _constatnt.setUserRedis();
                _alert.AddSuccessToastMessage("User created.");

            }
            return RedirectToAction("Client", "Master");
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var master = await _context.tblMaster.FindAsync(id);
            if (master == null)
            {
                return NotFound();
            }
            return PartialView("Action", master);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,userName,password,isActive,firmName,clientName,mobile,city,domain,symbol,group,versionAndroid,versionIos,passwordFormat,startLoginId,startDealNo,isCoin,isJewellery,isKyc,isOtr,isOtp,isMeta,privacyPolicyFile,isEndUserLogin,createDate,modifiedDate,isCity,isCoinTrade,isCategory,totalSlider,isSlider")] Master master)
        {
            if (id != master.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(master);
                    await _context.SaveChangesAsync();
                    _constatnt.setUserRedis();
                    saveFile(master.privacyPolicyFile, master.userName);
                    _alert.AddSuccessToastMessage("User edited.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MasterExists(master.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Client));
            }
            return View(master);
        }
        private void saveFile(IFormFile? file, string userName)
        {
            if (file != null)
            {
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images\\privacyPolicy\\");
                string fileExtension = Path.GetExtension(file.FileName);
                string filePath = Path.Combine(uploadFolder, userName + fileExtension);
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
            }
        }
        public JsonResult getReferance(int id)
        {
            var referance = _context.tblReferanceSymbol.Where(s => s.clientId == id).ToList();
            return Json(referance);
        }
        [HttpPost]
        public async Task<IActionResult> updateReferance([FromBody] JsonObject obj)
        {
            ReferanceSymbol referance = await _context.tblReferanceSymbol.FindAsync((int)obj?["id"]);
            if (referance == null)
            {
                return NotFound();
            }
            referance.isMaster = (bool)obj["isView"];
            referance.name = obj["name"].ToString();
            if (TryValidateModel(referance))
            {
                try
                {
                    var data = _context.Update(referance);
                    await _context.SaveChangesAsync();
                    var isSuccess = data.Entity.id;
                    _alert.AddSuccessToastMessage("Referance product updated.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MasterExists(referance.id))
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

        private bool MasterExists(int id)
        {
            return _context.tblMaster.Any(e => e.id == id);
        }
    }

    internal class objArray
    {
        public string source { get; set; }
        public string name { get; set; }
    }
}

using System.Drawing.Printing;
using System.Globalization;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NToastNotify;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SL_Bullion.Controllers
{
    public class AccountController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly ApplicationConstant _constatnt;
        private readonly IToastNotification _alert;
        private readonly MessageConstant _messageConstatnt;
        private readonly IConfiguration _config;
        private const int PageSize = 20;

        public AccountController(BullionDbContext context, ApplicationConstant constatnt, IToastNotification alert, MessageConstant messageConstatnt, IConfiguration configuration)
        {
            _context = context;
            _constatnt = constatnt;
            _alert = alert;
            _messageConstatnt = messageConstatnt;
            _config = configuration;
        }

        public async Task<IActionResult> List(int page = 1)
        {
            var data = await getPaginationDetails(page, PageSize, "list");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)data.TotalItems / PageSize);
            return View(data);
        }

        private async Task<PaginationViewModel<Account>> getPaginationDetails(int pageNumber, int pageSize, string searchTerm)
        {
            int? clientId = HttpContext.Session.GetInt32("clientId");
            if (clientId == null) return new PaginationViewModel<Account> { Items = new List<Account>() };

            IQueryable<Account> query = _context.tblAccount.AsNoTracking().Where(a => !a.isRegister && a.clientId == clientId);

            if (!string.IsNullOrWhiteSpace(searchTerm) && searchTerm != "list")
            {
                query = query.Where(a => a.loginId.Contains(searchTerm) || a.name.Contains(searchTerm) || a.firmName.Contains(searchTerm) || a.mobile.Contains(searchTerm));
            }

            int totalItems = await query.CountAsync();
            var items = await query.OrderByDescending(a => a.id).Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(a => new
                {
                    a.id,
                    a.isActive,
                    a.loginId,
                    a.name,
                    a.firmName,
                    a.mobile,
                    a.city,
                    a.tradeAccess,
                    groupName = _context.tblGroup.Where(g => g.id == a.groupId).Select(g => g.name).FirstOrDefault()
                }).ToListAsync();

            var accounts = items.Select(a => new Account
            {
                id = a.id,
                isActive = a.isActive,
                loginId = a.loginId,
                name = a.name,
                firmName = a.firmName,
                mobile = a.mobile,
                city = a.city,
                tradeAccessView = a.tradeAccess switch { 1 => "Full", 2 => "Buy", _ => "Sell" },
                groupName = a.groupName
            }).ToList();

            return new PaginationViewModel<Account>
            {
                Items = accounts,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        public JsonResult viewAddModel()
        {
            var clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
            var account = _context.tblMaster.Where(m => m.id == clientId)
                .Select(m => new
                {
                    loginId = m.passwordFormat + m.startLoginId,
                    group = _context.tblGroup.Where(g => g.clientId == clientId).Select(g => new { groupId = g.id, groupName = g.name }).ToList()
                }).FirstOrDefault();
            return Json(account);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,clientId,loginId,password,name,firmName,mobile,email,city,groupId,tradeAccess,isActive,gst,margin,modifiedDate,startDate,endDate")] Account account)
        {
            if (string.IsNullOrWhiteSpace(account.name))
            {
                _alert.AddWarningToastMessage("Name is required.");
                return RedirectToAction(nameof(List));
            }

            account.clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
            var userName = await _context.tblMaster.Where(_ => _.id == account.clientId).Select(_ => _.userName).FirstOrDefaultAsync();
            bool mobileAlreadyExists = await _context.tblAccount.AnyAsync(x => x.mobile == account.mobile && x.clientId == account.clientId);

            if (userName == null)
            {
                _alert.AddWarningToastMessage("User not exists.");
                return RedirectToAction(nameof(List));
            }

            if (mobileAlreadyExists)
            {
                _alert.AddWarningToastMessage("Mobile number already exist.");
                return RedirectToAction(nameof(List));
            }

            if (ModelState.IsValid)
            {
                var data = _context.Add(account);
                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE tblMaster SET startLoginId = startLoginId+1 WHERE id = {HttpContext.Session.GetInt32("clientId")}");
                _constatnt.pushAccountDetails(data.Entity.id, getGroupId(data.Entity.id));

                //if (_config.GetSection("singleLogin").Get<string[]>().Contains($"{clientId}"))
                //{
                //    _constatnt.pushSingleLoginDetails(id);
                //}

                return RedirectToAction(nameof(List));
            }
            return View(account);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            getGroup();
            string loginId = _context.tblAccount.Where(a => a.id == id).Select(a => a.loginId).FirstOrDefault();
            ViewBag.usedMargin = _context.tblOpenOrder.Where(o => o.clientId == HttpContext.Session.GetInt32("clientId").GetValueOrDefault() && o.loginId == loginId).Sum(o => o.margin);
            var account = await _context.tblAccount.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return PartialView("Action", account);
        }
        [NonAction]
        private void getGroup()
        {
            var group = _context.tblGroup.Where(g => g.clientId == HttpContext.Session.GetInt32("clientId").GetValueOrDefault()).Select(g => new { groupId = g.id, groupName = g.name }).ToList();
            ViewBag.group = new SelectList(group, "groupId", "groupName");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,clientId,loginId,password,name,firmName,mobile,email,city,groupId,tradeAccess,isActive,gst,margin,type,modifiedDate,startDate,endDate")] Account account)
        {
            if (string.IsNullOrWhiteSpace(account.name))
            {
                _alert.AddWarningToastMessage("Name is required.");
                return RedirectToAction(nameof(List));
            }

            if (id != account.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int groupId = getGroupId(id);
                    account.clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                    _constatnt.pushAccountDetails(id, groupId);
                    _constatnt.pushSingleLoginDetails(id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(List));
            }
            return View(account);
        }
        [HttpPost]
        public async Task<IActionResult> updateAmount([FromBody] JsonObject obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var account = _context.tblAccount.Where(a => a.clientId == HttpContext.Session.GetInt32("clientId") && a.loginId == obj["loginId"].ToString()).FirstOrDefault();
                    var user = _context.tblMaster.Where(m => m.id == account.clientId).Select(m => m.userName).FirstOrDefault();
                    if (account != null)
                    {
                        if (obj["type"]?.ToString() == "add")
                        {
                            account.margin += Convert.ToDecimal(obj["amount"]?.ToString());
                        }
                        if (obj["type"]?.ToString() == "withdraw")
                        {
                            account.margin -= Convert.ToDecimal(obj["amount"]?.ToString());
                        }
                        if (obj["type"]?.ToString() == "resend")
                        {
                            _messageConstatnt.pushMessageAlert("sendlogin", user, account.id, account.clientId);
                        }
                        await _context.SaveChangesAsync();
                        var freeMargin = account.margin - Convert.ToDecimal(ViewBag.usedMargin);
                        return Json(new { margin = account.margin, freeMargin = freeMargin });
                    }
                }
                catch (DbUpdateConcurrencyException)
                {

                }
            }

            return BadRequest();
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.tblOpenOrder.Any(o => o.loginId == _context.tblAccount.Where(a => a.id == id).Select(a => a.loginId).FirstOrDefault()))
            {
                _alert.AddWarningToastMessage("User position open in openorder,Not deletable.");
                return RedirectToAction(nameof(List));
            }
            var account = await _context.tblAccount.FindAsync(id);
            if (account != null)
            {
                _context.tblAccount.Remove(account);
            }

            await _context.SaveChangesAsync();
            _constatnt.pushAccountDetails(id, getGroupId(id));
            return RedirectToAction(nameof(List));
        }
        public async Task<IActionResult> exportToExcel()
        {
            int clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
            var items = await _context.tblAccount.Where(a => a.isRegister == false && a.clientId == HttpContext.Session.GetInt32("clientId")).Select(a => new
            {
                IsActive = a.isActive,
                LoginId = a.loginId,
                Name = a.name,
                FirmName = a.firmName,
                Mobile = a.mobile,
                City = a.city,
                TradeAccess = a.tradeAccess == 1 ? "Full" : a.tradeAccess == 2 ? "Buy" : "Sell",
                GroupName = _context.tblGroup.Where(g => g.id == a.groupId).Select(g => g.name).FirstOrDefault()
            }).ToListAsync();
            if (items == null || !items.Any())
            {
                return NotFound("No data available to export.");
            }
            var fileContent = _constatnt.generateExcelFromList(items, "AccountData");
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"AccountData_{DateTime.Now}.xlsx");
        }
        public async Task<IActionResult> search(string search)
        {
            var data = await getPaginationDetails(1, 20, search);
            return View("List", data);
        }

        private bool AccountExists(int id)
        {
            return _context.tblAccount.Any(e => e.id == id);
        }
        private int getGroupId(int id)
        {
            return _context.tblAccount.Where(a => a.id == id).Select(a => a.groupId).FirstOrDefault();
        }
    }
}

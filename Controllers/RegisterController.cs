using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using System.Text.Json.Nodes;

namespace SL_Bullion.Controllers
{
    public class RegisterController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly ApplicationConstant _constatnt;
        private readonly MessageConstant _messageConstatnt;

        public RegisterController(BullionDbContext context, ApplicationConstant constatnt, MessageConstant messageConstatnt)
        {
            _context = context;
            _constatnt = constatnt;
            _messageConstatnt = messageConstatnt;
        }
        public async Task<IActionResult> List(int page = 1)
        {
            int pageSize = 20;
            var data = await getPaginationDetails(page, pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = data.TotalPages;
            return View(data);
        }

        private async Task<PaginationViewModel<Account>> getPaginationDetails(int pageNumber, int pageSize)
        {
            var query = _context.tblAccount.Where(a => a.isRegister && a.clientId == HttpContext.Session.GetInt32("clientId"));
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var items = await query.OrderByDescending(a => a.modifiedDate).Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(a => new Account
                {
                    id = a.id,
                    name = a.name,
                    firmName = a.firmName,
                    mobile = a.mobile,
                    city = a.city,
                    modifiedDate = a.modifiedDate
                }).ToListAsync();
            return new PaginationViewModel<Account>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        [HttpPost, ActionName("Approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveConfirmed(int id)
        {
            var account = await _context.tblAccount.FindAsync(id);
            var user = _context.tblMaster.Where(m => m.id == account.clientId).Select(m => m.userName).FirstOrDefault();
            if (account != null)
            {
                account.isRegister = false;
                account.loginId = _context.tblMaster.Where(m => m.id == HttpContext.Session.GetInt32("clientId").GetValueOrDefault()).Select(m => m.passwordFormat + m.startLoginId).FirstOrDefault();
                account.password = GenerateRandomPassword(6);
                _context.Update(account);
                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE tblMaster SET startLoginId = startLoginId+1 WHERE id = {HttpContext.Session.GetInt32("clientId")}");
                _constatnt.pushAccountDetails(id, getGroupId(id));
                _messageConstatnt.pushMessageAlert("userApproove", user, account.id, account.clientId);
            }
            return RedirectToAction(nameof(List));
        }

        private string GenerateRandomPassword(int length)
        {
            const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numbers = "0123456789";
            const string allChars = upperCase + numbers;
            Random random = new Random();

            char[] password = new char[length];

            password[0] = upperCase[random.Next(upperCase.Length)];
            password[1] = upperCase[random.Next(upperCase.Length)];
            password[2] = numbers[random.Next(numbers.Length)];
            password[3] = numbers[random.Next(numbers.Length)];

            for (int i = 4; i < length; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }

            for (int i = 0; i < length; i++)
            {
                int randomIndex = random.Next(i, length);
                char temp = password[i];
                password[i] = password[randomIndex];
                password[randomIndex] = temp;
            }
            return new string(password);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.tblAccount.FindAsync(id);
            if (account != null)
            {
                _context.tblAccount.Remove(account);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
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

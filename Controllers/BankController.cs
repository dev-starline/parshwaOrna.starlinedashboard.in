using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.DAL;
using SL_Bullion.Models;

namespace SL_Bullion.Controllers
{
    public class BankController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;

        public BankController(BullionDbContext context, IToastNotification alert)
        {
            _context = context;
            _alert = alert;
        }
        public async Task<IActionResult> List()
        {
            getBankLogo();
            return View(await _context.tblBank.Where(s => s.clientId == HttpContext.Session.GetInt32("clientId")).ToListAsync());
        }
        [NonAction]
        private void getBankLogo()
        {
            var bankLogo = _context.tblBankLogo.Select(bl => new { text = bl.text, value = bl.value }).ToList();
            ViewBag.bankLogo = new SelectList(bankLogo, "value", "text");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,clientId,accountName,bankName,accountNumber,ifscCode,branchName,bankLogo,bankLogoUrl,modifiedDate")] Bank bank)
        {
            if (string.IsNullOrWhiteSpace(bank.accountName) || string.IsNullOrWhiteSpace(bank.bankName) || string.IsNullOrWhiteSpace(bank.accountNumber) || string.IsNullOrWhiteSpace(bank.ifscCode) || string.IsNullOrWhiteSpace(bank.branchName))
            {
                _alert.AddErrorToastMessage("Please fill in all required fields: Account Name, Bank Name, Account Number, IFSC Code, and Branch Name.");
                return RedirectToAction(nameof(List));
            }
            if (ModelState.IsValid)
            {
                bank.clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
                bank.bankLogoUrl = $"https://starlinetechno.in/BankImages/{bank.bankLogo}.jpg";
                _context.Add(bank);
                await _context.SaveChangesAsync();
                _alert.AddSuccessToastMessage("Bank created.");
                return RedirectToAction(nameof(List));
            }
            return View(bank);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            getBankLogo();
            var bank = await _context.tblBank.FindAsync(id);
            if (bank == null)
            {
                return NotFound();
            }
            return PartialView("Action", bank);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,clientId,accountName,bankName,accountNumber,ifscCode,branchName,bankLogo,bankLogoUrl,modifiedDate")] Bank bank)
        {
            if (id != bank.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bank.clientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
                    bank.bankLogoUrl = $"https://starlinetechno.in/BankImages/{bank.bankLogo}.jpg";
                    _context.Update(bank);
                    await _context.SaveChangesAsync();
                    _alert.AddSuccessToastMessage("Bank Edited.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BankExists(bank.id))
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
            else
            {
                _alert.AddWarningToastMessage("Please fill all required fields.");
                return RedirectToAction(nameof(List));
            }
            //return View(bank);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bank = await _context.tblBank.FindAsync(id);
            if (bank != null)
            {
                _context.tblBank.Remove(bank);
            }

            await _context.SaveChangesAsync();
            _alert.AddSuccessToastMessage("Bank deleted.");
            return RedirectToAction(nameof(List));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bank = await _context.tblBank
                .FirstOrDefaultAsync(m => m.id == id);
            if (bank == null)
            {
                return NotFound();
            }

            return View(bank);
        }

        // GET: Bank/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bank = await _context.tblBank
                .FirstOrDefaultAsync(m => m.id == id);
            if (bank == null)
            {
                return NotFound();
            }

            return View(bank);
        }

        // GET: Bank/Create
        public IActionResult Create()
        {
            return View();
        }

       
        

        // GET: Bank/Delete/5
       

        // POST: Bank/Delete/5
        

        private bool BankExists(int id)
        {
            return _context.tblBank.Any(e => e.id == id);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.DAL;
using System.Text;

namespace SL_Bullion.Controllers
{
    public class KycController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        public KycController(BullionDbContext context, IToastNotification alert)
        {
            _context = context;
            _alert = alert;
        }
        public async Task<IActionResult> List()
        {
            return View(await _context.tblKyc.Where(k => k.clientId == HttpContext.Session.GetInt32("clientId")).ToListAsync());
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kyc = await _context.tblKyc.FindAsync(id);
            if (kyc != null)
            {
                _context.tblKyc.Remove(kyc);
            }

            await _context.SaveChangesAsync();
            _alert.AddSuccessToastMessage("kyc deleted.");
            return RedirectToAction(nameof(List));
        }
        [HttpPost, ActionName("generateCsv")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> csv(int id)
        {
            var data = _context.tblKyc.Where(k => k.id == id).ToList();
            var csv = new StringBuilder();
            csv.AppendLine("name,mobile,companyName,companyAddress,partnerName,partnerMobile,officeMobile1,officeMobile2,residenceAddress,mail,bankName,branchName,accountNumber,ifsc,gstNumber,panNumber,reference,modifiedDate");

            foreach (var item in data)
            {
                csv.AppendLine($"{item.name},{item.mobile},{item.companyName},{item.companyAddress},{item.partnerName},{item.partnerMobile},{item.officeMobile1}" +
                    $",{item.officeMobile2},{item.residenceAddress},{item.mail},{item.bankName},{item.branchName},{item.accountNumber},{item.ifsc}" +
                    $",{item.gstNumber},{item.panNumber},{item.reference},{item.modifiedDate}");
            }

            var csvBytes = Encoding.UTF8.GetBytes(csv.ToString());
            var csvStream = new MemoryStream(csvBytes);

            // Return the CSV file
            return File(csvStream, "text/csv", "data.csv");
        }
    }
}

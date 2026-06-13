using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SL_Bullion.Controllers
{
    public class CityController : Controller
    {
        private readonly BullionDbContext _context;
        private readonly IToastNotification _alert;
        private readonly ApplicationConstant _constatnt;

        public CityController(BullionDbContext context, IToastNotification alert, ApplicationConstant constatnt)
        {
            _context = context;
            _alert = alert;
            _constatnt = constatnt;
        }

        public IActionResult List()
        {
            var clientId = HttpContext.Session.GetInt32("clientId") ?? 0;

            var cities = _context.tblCity.Where(c => c.ClientId == clientId).ToList();

            return View(cities);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClientId,Name,CreatedDate")] City city)
        {
            if (ModelState.IsValid)
            {
                city.ClientId = HttpContext.Session.GetInt32("clientId").GetValueOrDefault();
                city.CreatedDate = DateTime.Now;
                var data = _context.Add(city);
                await _context.SaveChangesAsync();
                _constatnt.setCityRedis();
                var isSuccess = data.Entity.Id;
                if (isSuccess > 0)
                {
                    _alert.AddSuccessToastMessage("City created.");
                }
                return RedirectToAction(nameof(List));
            }
            return View(city);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var clientId = HttpContext.Session.GetInt32("clientId") ?? 0;

            //var symbolsWithCity = await _context.tblSymbol.Where(s => s.CityId == id && s.clientId == clientId).ToListAsync();

            //if (symbolsWithCity.Any())
            //{
            //    foreach (var symbol in symbolsWithCity)
            //    {
            //        symbol.CityId = 0;
            //    }
            //    _context.tblSymbol.UpdateRange(symbolsWithCity);
            //    await _context.SaveChangesAsync();
            //    _constatnt.setCityRedis();
            //}

            var city = await _context.tblCity.FindAsync(id);
            if (city != null)
            {
                _context.tblCity.Remove(city);
            }

            await _context.SaveChangesAsync();
            _constatnt.setCityRedis();
            _alert.AddSuccessToastMessage("City deleted.");
            return RedirectToAction(nameof(List));
        }
    }
}

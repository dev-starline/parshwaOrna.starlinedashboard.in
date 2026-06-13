using Microsoft.AspNetCore.Mvc;
using SL_Bullion.DAL;

namespace SL_Bullion.Controllers
{
    public class BankRatesController : Controller
    {
        private readonly BullionDbContext _context;

        public BankRatesController(BullionDbContext context)
        {
            _context = context;
        }
        
    }
}

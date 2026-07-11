using Microsoft.AspNetCore.Mvc.Rendering;

namespace SL_Bullion.Models
{
    public class SymbolViewModel
    {
        public List<SymbolList> Symbols { get; set; }
        public List<SelectListItem> Cities { get; set; }
        public int? CityId { get; set; }
    }
}

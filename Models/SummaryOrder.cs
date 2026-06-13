using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SL_Bullion.Models
{
    public class SummaryOrder
    {

        [Required]
        public int clientId { get; set; }

        [Required]
        public int symbolId { get; set; }
        [Required]
        public string symbolName { get; set; }

        [Required]
        public double volume { get; set; }

        [Required]
        public int tradeType { get; set; }
        [Required]
        public double rate { get; set; }
        [Required]
        public double buyCount { get; set; }

        public double exchange { get; set; }

        public decimal total { get; set; } = 0;

        public decimal sellCount { get; set; } = 0;
        public decimal buyVolume { get; set; } = 0;
        public decimal sellVolume { get; set; } = 0;
        public double buyAvg { get; set; } = 0;
        public double sellAvg { get; set; } = 0;
        public decimal netCount { get; set; } = 0;
        public decimal netGram { get; set; } = 0;


        public DateTime orderTime { get; set; } = DateTime.Now;

        [NotMapped]
        public string? name;
        [NotMapped]
        public string? firm;
        [NotMapped]
        public string? tradeTypeView;
        [NotMapped]
        public DateTime? fromDate;
        [NotMapped]
        public DateTime? toDate;
    }

    public class MetalTradeSummary
    {
        public DateTime Date { get; set; }
        public int NoOfTrades { get; set; }
        public decimal TotalBuyGold { get; set; }
        public decimal TotalBuySilver { get; set; }
        public double AverageBuyGold { get; set; }
        public double AverageSellGold { get; set; }
        public double AverageBuySilver { get; set; }
        public double AverageSellSilver { get; set; }
    }
    public class TradeSummaryResponse
    {
        public List<SummaryOrder> SymbolSummary { get; set; }
        public List<MetalTradeSummary> MetalSummary { get; set; }
        public List<SummaryOrder> SummarySymbol { get; set; }
    }
}

namespace SL_Bullion.Models
{
    public class CoinCloseOrderListDto
    {
        public int OpenOrderID { get; set; }
        public int ClientId { get; set; }
        public long DealNo { get; set; }
        public string LoginID { get; set; }
        public string UserName { get; set; }
        public int SymbolID { get; set; }
        public string SymbolName { get; set; }
        public string Source { get; set; }
        public double Rate { get; set; }
        public double Exchange { get; set; }
        public double Total { get; set; }
        public string IP { get; set; }
        public string Mac { get; set; }
        public double Volume { get; set; }

        // Dates formatted as string (since your SQL uses CONVERT to varchar)
        public DateTime OpenTradeDateTime { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? CloseDateTime { get; set; }

        public string TradeType { get; set; }
        public string TradeFrom { get; set; }
        public string? Comment { get; set; }

        public string FirmName { get; set; } = "FirmName"; // default fallback
        public double? ClosePrice { get; set; }

        // Computed rank
        public int Rank { get; set; }
        public string Name { get; set; }
    }
}

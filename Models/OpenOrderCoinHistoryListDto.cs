namespace SL_Bullion.Models
{
    public class OpenOrderCoinHistoryListDto
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

        // Dates are formatted in SQL as VARCHAR, but in C# you can keep them as DateTime
        // and format them later when needed.
        public DateTime OpenTradeDateTime { get; set; }
        public string TradeType { get; set; }
        public string TradeFrom { get; set; }
        public string Comment { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string FirmName { get; set; } = "FirmName"; // default fallback

        public double? ClosePrice { get; set; } // if needed later
        public DateTime? CloseDateTime { get; set; } // if needed later
        public DateTime? DeleteDate { get; set; }
        public int Rank { get; set; }
        public string Name { get; set; }

    }
}

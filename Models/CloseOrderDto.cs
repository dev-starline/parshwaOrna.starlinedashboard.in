namespace SL_Bullion.Models
{
    public class CloseOrderDto
    {
        public int OpenOrderID { get; set; }
        public int ClientId { get; set; }
        public string DealNo { get; set; }
        public int LoginID { get; set; }
        public string UserName { get; set; }
        public int SymbolID { get; set; }
        public string SymbolName { get; set; }
        public string Source { get; set; }
        public double Rate { get; set; }
        public string Exchange { get; set; }
        public double Total { get; set; }
        public string IP { get; set; }
        public string Mac { get; set; }
        public double Volume { get; set; }
        public DateTime OpenTradeDateTime { get; set; }
        public string TradeType { get; set; }
        public string TradeFrom { get; set; }
        public string Comment { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string FirmName { get; set; }
        public double? ClosePrice { get; set; }
        public DateTime CloseDateTime { get; set; }
        public int Rank { get; set; }
    }
}

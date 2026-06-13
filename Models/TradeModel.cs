namespace SL_Bullion.Models
{
    public class TradeModel
    {
        public int SymbolId { get; set; }
        public double Volume { get; set; }   // maps from ObjTrade["Quantity"]
        //public string Token { get; set; } = string.Empty;
        public string TradeFrom { get; set; } = string.Empty;
        public string TradeType { get; set; } = string.Empty;
        public string DeviceToken { get; set; } = string.Empty;
        public double BuyLimitPrice { get; set; }
        public double SellLimitPrice { get; set; }

    }
}

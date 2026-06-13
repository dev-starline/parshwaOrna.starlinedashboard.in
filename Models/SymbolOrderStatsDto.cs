namespace SL_Bullion.Models
{
    public class SymbolOrderStatsDto
    {
        public int SymbolID { get; set; }

        // CoinsName is returned as string in SQL, but you had ISNULL(...,0).
        // Better to keep it as string and handle nulls in C#.
        public string SymbolName { get; set; } = string.Empty;

        public int BuyCount { get; set; }
        public int SellCount { get; set; }

        public double BuyVolume { get; set; }
        public double SellVolume { get; set; }

        public double BuyAvg { get; set; }
        public double SellAvg { get; set; }

        public int NetCount { get; set; }
        public double NetGrams { get; set; }

    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SL_Bullion.Models
{
    public class OpenOrderCoin
    {
        [Key]
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
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]

        public double Total { get; set; }
        public string IP { get; set; }
        public string Mac { get; set; }
        public double Volume { get; set; }
        public DateTime OpenTradeDateTime { get; set; }
        public string TradeType { get; set; }
        public string TradeFrom { get; set; }
        public string Comment { get; set; }
        public DateTime ModifiedDate { get; set; }

    }
}

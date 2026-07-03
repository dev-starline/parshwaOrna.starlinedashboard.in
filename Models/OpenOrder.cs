using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SL_Bullion.Models
{
    public class OrderBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]
        public int clientId { get; set; }
        [Required]
        public string loginId { get; set; }
        [Required]
        public int dealNo { get; set; }
        [Required]
        public int symbolId { get; set; }
        [Required]
        public string symbolName { get; set; }
        public string source { get; set; }
        public string rateType { get; set; }
        [Required]
        public double volume { get; set; }
        [Required]
        public int tradeType { get; set; }
        [Required]
        public double rate { get; set; }
        public double exchange { get; set; }
        public double differenceRate { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal total { get; set; } = 0;
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal tax { get; set; } = 0;
        public double premium { get; set; }
        public double premiumLimit { get; set; }
        public double margin { get; set; }
        public string ip { get; set; } = "";
        public string deviceType { get; set; } = "";
        public string? comment { get; set; } = "";
        public bool isLimit { get; set; } = false;
        public bool isHedge { get; set; } = false;
        public DateTime orderTime { get; set; } = DateTime.Now;
        public DateTime editorderTime { get; set; } = DateTime.Now;
        [NotMapped]
        public string? name;
        [NotMapped]
        public string? firm;
        [NotMapped]
        public string? mobile;
        [NotMapped]
        public string? tradeTypeView;
        [NotMapped]
        public DateTime? fromDate;
        [NotMapped]
        public DateTime? toDate;

    }
    public class OpenOrder : OrderBase
    {
        
    }
}

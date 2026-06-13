using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SL_Bullion.Models
{
    public class CoinBank
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]
        public int clientId { get; set; }
        public double premiumGold { get; set; } = 0;
        public double premiumSilver { get; set; } = 0;
        public string spotTypeGold { get; set; } = "INRSpot";
        public string spotTypeSilver { get; set; } = "INRSpot";
        public double interBankGold { get; set; } = 0;
        public double interBankSilver { get; set; } = 0;
        public double conversionGold { get; set; } = 1;
        public double conversionSilver { get; set; } = 1;
        public double customDutyGold { get; set; } = 0;
        public double customDutySilver { get; set; } = 0;
        public double marginGold { get; set; } = 0;
        public double marginSilver { get; set; } = 0;
        public double gstGold { get; set; } = 0;
        public double gstSilver { get; set; } = 0;
        public double divisionGold { get; set; } = 1;
        public double divisionSilver { get; set; } = 1;
        public double multiplyGold { get; set; } = 1;
        public double multiplySilver { get; set; } = 1;
        public DateTime modifiedDate { get; set; } = DateTime.Now;
    }
}

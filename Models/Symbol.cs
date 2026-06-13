using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SL_Bullion.Models
{
    

    public class Symbol
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]
        public int clientId { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string source { get; set; }
        [Required]
        public string sourceType { get; set; } = "C_BA";
        [Required]
        public string symbolType { get; set; } = "1";
        [Required]
        public bool isView { get; set; } = true;
        public bool isTerminal { get; set; } = true;
        public bool isTrade { get; set; } = true;
        public string rateType { get; set; } = "mcx";
        [RegularExpression("^--|-?[0-9]+$", ErrorMessage = "Invalid format. Only -- or digits (0-9) are allowed.")]
        [Required]
        public string buyPremium { get; set; } = "0";
        [RegularExpression("^--|-?[0-9]+$", ErrorMessage = "Invalid format. Only -- or digits (0-9) are allowed.")]
        [Required]
        public string sellPremium { get; set; } = "0";
        public double division { get; set; } = 1;
        public double multiply { get; set; } = 1;
        public double gst { get; set; } = 0;

        public double buyCommonPremium { get; set; } = 0;
        public double sellCommonPremium { get; set; } = 0;
        public int stock { get; set; } = 0;
        public int high { get; set; } = 0;
        public int low { get; set; } = 0;
        public int useStock { get; set; } = 0;
        public int initialMargin { get; set; } = 0;
        public int index { get; set; } = 0;
        public int digit { get; set; } = 0;
        public bool isBill { get; set; } = false;
        public double gstBill { get; set; } = 0;
        public double tcsBill { get; set; } = 0;
        public double tdsBill { get; set; } = 0;
        public DateTime createDate { get; set; } = DateTime.Now;
        public DateTime modifiedDate { get; set; }
        public DateTime changePremiumDate { get; set; }
        public string identifier { get; set; } = "0";
        [NotMapped]
        public string remainingStock
        {
            get
            {
                int remaining = stock - useStock;
                return $"{remaining}";
            }
        }
      
        public int CityId { get; set; } = 0;
    }
    
}

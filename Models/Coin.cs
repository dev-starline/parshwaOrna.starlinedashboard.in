using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SL_Bullion.Models
{
    public class Coin
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
        public bool isView { get; set; } = true;
        [Required]
        public bool isStock { get; set; } = true;
        public string rateType { get; set; } = "mcx";
        [RegularExpression("^--|-?[0-9]+$", ErrorMessage = "Invalid format. Only -- or digits (0-9) are allowed.")]
        public string buyPremium { get; set; } = "0";
        [RegularExpression("^--|-?[0-9]+$", ErrorMessage = "Invalid format. Only -- or digits (0-9) are allowed.")]
        public string sellPremium { get; set; } = "0";
        public double division { get; set; } = 1;
        public double multiply { get; set; } = 1;
        public double buyCommonPremium { get; set; } = 0;
        public double sellCommonPremium { get; set; } = 0;
        public int index { get; set; } = 0;
        public DateTime createDate { get; set; } = DateTime.Now;
        public DateTime modifiedDate { get; set; }
        public double coinStock { get; set; } = 0;
        [NotMapped]
        public double remainingStock { get; set; }
        public double gst { get; set; } = 0;
        public int coinId { get; set; } = 0;

        public string? url { get; set; }
        [NotMapped]
        public IFormFile? image { get; set; }
    }
}

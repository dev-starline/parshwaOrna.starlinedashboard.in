using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SL_Bullion.Models
{
    public class Group
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]
        public int clientId { get; set; }
        [Required]
        public string name { get; set; }
        public double buyPremiumGold { get; set; } = 0;
        public double sellPremiumGold { get; set; } = 0;
        public double buyPremiumSilver { get; set; } = 0;
        public double sellPremiumSilver { get; set; } = 0;
        public bool isTrade { get; set; } = true;
        public bool isEnable { get; set; } = true;
    }

}

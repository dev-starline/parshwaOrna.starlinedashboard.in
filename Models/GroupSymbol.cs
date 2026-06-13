using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SL_Bullion.Models
{
    public class GroupSymbol
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]
        public int groupId { get; set; }
        [Required]
        public int symbolId { get; set; }
        public bool isView { get; set; } = false;
        public double buyPremium { get; set; } = 0;
        public double sellPremium { get; set; } = 0;
        public double oneClick { get; set; } = 1;
        public double inTotal { get; set; } = 10;
        public double step { get; set; } = 0;
    }
}

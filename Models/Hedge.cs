using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SL_Bullion.Models
{
    public class Hedge
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]
        public int clientId { get; set; }
        [Required]
        public int symbolId { get; set; }
        public int hedgeSymbolId { get; set; } = 0;
        public int division { get; set; } = 1;
        public bool status { get; set; } = false;
        [NotMapped]
        public string? symbolName;

    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SL_Bullion.Models
{
    public class ReferanceSymbol
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]
        public int clientId { get; set; }
        [Required]
        public string source { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public bool isMaster { get; set; } = true;
        [Required]
        public bool isView { get; set; } = false;
    }
}

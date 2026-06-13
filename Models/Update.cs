using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SL_Bullion.Models
{
    public class Update
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]
        public int clientId { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public string? description { get; set; }
        public DateTime modifiedDate { get; set; } = DateTime.Now;
    }
}

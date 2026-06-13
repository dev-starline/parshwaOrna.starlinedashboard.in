using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SL_Bullion.Models
{
    public class HedgeInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public int clientId { get; set; }
        public string request { get; set; }
        public string response { get; set; }
        public DateTime createDate { get; set; } = DateTime.Now;
    }
}

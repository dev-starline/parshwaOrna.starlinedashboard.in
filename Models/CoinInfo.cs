using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SL_Bullion.Models
{
    [Table("tblCoinInfo")]
    public class CoinInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? Request { get; set; }

        public string? Response { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;

        public int ClientId { get; set; }

        public string? UserName { get; set; }
    }
}
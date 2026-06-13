using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SL_Bullion.Models
{
    public class Otr
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]
        public int clientId { get; set; }
        public string? name { get; set; }
        public string? mobile { get; set; }
        public string? firmname { get; set; }
        public string? city { get; set; }
        public string ip { get; set; }
        public DateTime modifiedDate { get; set; } = DateTime.Now;
        public string? otp { get; set; } = "0";
        public bool? isOtpVerify { get; set; } = false;
    }
}

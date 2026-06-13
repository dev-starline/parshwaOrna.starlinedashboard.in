using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SL_Bullion.Models
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]
        public int clientId { get; set; }
        [Required]
        public string loginId { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        public string name { get; set; }
        public string? firmName { get; set; }
        public string? mobile { get; set; }
        public string? email { get; set; }
        public string? city { get; set; }
        //public string? otp { get; set; }
        [Required]
        public int groupId { get; set; }
        public int tradeAccess { get; set; } = 1;
        public string type { get; set; } = "none";
        public bool isRegister { get; set; } = false;
        public bool isActive { get; set; } = true;
        //public bool isOtp { get; set; } = true;
        public string? gst { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal margin { get; set; } = 0;
        [NotMapped]
        public decimal amount { get; set; } = 0;
        public DateTime modifiedDate { get; set; } = DateTime.Now;
        [NotMapped]
        public string? tradeAccessView;
        [NotMapped]
        public string? groupName;
        public string? mac { get; set; }

        public DateTime? startDate { get; set; } = DateTime.UtcNow;
        public DateTime? endDate { get; set; } = DateTime.UtcNow.AddDays(15);
    }
}

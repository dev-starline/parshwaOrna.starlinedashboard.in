using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SL_Bullion.Models
{
    public class Master
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]
        public string userName { get; set; }
        [Required]
        public string password { get; set; }
        public bool isActive { get; set; } = true;
        [Required]
        public string firmName { get; set; }
        public string clientName { get; set; }
        public string mobile { get; set; }
        public string city { get; set; }
        public string? domain { get; set; }
        public int symbol { get; set; } = 10;
        public int group { get; set; } = 4;
        public string versionAndroid { get; set; } = "1.0";
        public string versionIos { get; set; } = "1.0";
        public string? passwordFormat { get; set; }
        public int startLoginId { get; set; } = 1000;
        public int startDealNo { get; set; } = 1000;
        public bool isMeta { get; set; } = false;
        public bool isEndUserLogin { get; set; } = false;
        public bool isOtp { get; set; } = false;
        public bool isCoin { get; set; }=false;
        public bool isJewellery { get; set; }= false;
        public bool isKyc { get; set; } = false;
        public bool isOtr { get; set; } = false;
        public bool isCity { get; set; } = false;
        [NotMapped]
        public IFormFile? privacyPolicyFile { get; set; }
        public DateTime createDate { get; set; } = DateTime.Now;
        public DateTime modifiedDate { get; set; }
        public int lastCoinDealNo { get; set; }  // maps [lastCoinDealNo]
        public bool coinTradeOn { get; set; } = false;      // maps [coinTradeOn]
        public string coinStartTime { get; set; } = "00:00";    // maps [coinStartTime]    
        public string coinEndTime { get; set; } = "00:00";      // maps [coinEndTime]
        public bool isCoinTrade { get; set; } = false;
        public bool isCategory { get; set; }
        public int? totalSlider { get; set; } = 0;
        public bool isSlider { get; set; } = false;
    }
}

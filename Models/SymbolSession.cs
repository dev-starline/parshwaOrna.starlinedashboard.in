using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SL_Bullion.Models
{
    public class SymbolSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]
        public int symbolId { get; set; }
        [Required]
        public string session { get; set; } = "{\"startSunday\":\"09:01\",\"endSunday\":\"23:00\",\"startMonday\":\"09:01\",\"endMonday\":\"23:00\",\"startTuesday\":\"09:01\",\"endTuesday\":\"23:00\",\"startWednesday\":\"09:01\",\"endWednesday\":\"23:00\",\"startThursday\":\"09:01\",\"endThursday\":\"23:00\",\"startFriday\":\"09:01\",\"endFriday\":\"23:00\",\"startSaturday\":\"09:01\",\"endSaturday\":\"23:00\",\"symbolId\":\"2\"}";
    }
}

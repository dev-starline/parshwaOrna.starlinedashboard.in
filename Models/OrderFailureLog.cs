using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SL_Bullion.Models
{

    public class OrderFailureLog
    {
        public int id { get; set; }

        public int clientId { get; set; }

        public string name { get; set; }

        public string loginId { get; set; }

        public string firmName { get; set; }

        public string mobile { get; set; }

        public int symbolId { get; set; }

        public decimal quantity { get; set; }

        public decimal price { get; set; }

        public string reason { get; set; }

        public DateTime cdate { get; set; } = DateTime.Now;
    }




    public class OrderFailureLogViewModel
    {
        public int id { get; set; }

        public int clientId { get; set; }

        public string loginId { get; set; }

        public string name { get; set; }

        public string firmName { get; set; }

        public string mobileNumber { get; set; }

        public string symbolName { get; set; }

        public decimal quantity { get; set; }

        public decimal price { get; set; }

        public string reason { get; set; }

        public DateTime createdAt { get; set; }
    }
}

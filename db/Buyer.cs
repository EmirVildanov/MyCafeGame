using System.ComponentModel.DataAnnotations;

namespace MyCafe.db
{
    public class Buyer
    {
        [Key]
        public string buyer_name {get; set;}
        public int purchase_cost { get; set; }
    }
}
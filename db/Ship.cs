using System.ComponentModel.DataAnnotations;

namespace MyCafe.db
{
    public class Ship
    {
        [Key]
        public string ship_name { get; set; }
        public int carrying_capacity_in_ton { get; set; }
    }
}
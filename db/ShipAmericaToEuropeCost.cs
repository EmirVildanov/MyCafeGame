using System.ComponentModel.DataAnnotations;

namespace MyCafe.db
{
    public class ShipAmericaToEuropeCost
    {
        [Key]
        public int cost_id { get; set; }
        public string ship_name { get; set; }
        public string american_port_name { get; set; }
        public string european_port_name { get; set; }
        public int cost { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace MyCafe.db
{
    public class Plantation
    {
        [Key]
        public int plantation_id { get; set; }
        public string country { get; set; }
        public string manager { get; set; }
        public string port_name { get; set; }
    }
}
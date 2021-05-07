using System.ComponentModel.DataAnnotations;

namespace MyCafe.db
{
    public class AmericanPort
    {
        [Key]
        public string american_port_name { get; set; }
    }
}
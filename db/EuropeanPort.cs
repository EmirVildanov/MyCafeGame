using System.ComponentModel.DataAnnotations;

namespace MyCafe.db
{
    public class EuropeanPort
    {
        [Key]
        public string european_port_name { get; set; }
    }
}
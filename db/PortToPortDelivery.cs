using System;
using System.ComponentModel.DataAnnotations;

namespace MyCafe.db
{
    public class PortToPortDelivery : Delivery
    {
        [Key]
        public int port_to_port_delivery_id { get; set; }
        public int delivery_cost_id { get; set; }
        public string buyer_name { get; set; }
        public DateTime date { get; set; }
    }
}
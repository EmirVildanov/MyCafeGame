using System;
using System.ComponentModel.DataAnnotations;
using MyCafe.db;

namespace MyCafe
{
    public class PlantationToPortDelivery : Delivery
    {
        [Key]
        public int plantation_to_port_delivery_id { get; set; }
        public int plantation_id { get; set; }
        public int coffee_amount_in_ton { get; set; }
        public DateTime date { get; set; }
    }
}
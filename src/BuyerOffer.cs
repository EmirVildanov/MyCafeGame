using System;
using System.Linq;
using MyCafe.db;

namespace MyCafe
{
    public class BuyerOffer
    {
        private Random _random = new();
        public Buyer Buyer;
        public int WantedAmount;
        public string BuyerOfferString { get; }
        public EuropeanPort EuropeanPort;
        public DateTime Date;

        public BuyerOffer(Buyer buyer, int wantedAmount, DateTime date)
        {
            var db = new CafeContext();
            var europeanPorts = db.european_port.ToList();
            var currentPortIndex = _random.Next(europeanPorts.Count);
            var currentPort = europeanPorts[currentPortIndex];
            EuropeanPort = currentPort;
            Buyer = buyer;
            WantedAmount = wantedAmount;
            BuyerOfferString = $"{buyer.buyer_name} offer you ${buyer.purchase_cost}k per ton from {EuropeanPort.european_port_name} for {WantedAmount} coffee tons";
            Date = date;
        }
    }
}
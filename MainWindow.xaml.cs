using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MyCafe.db;
using MyCafe.src;

namespace MyCafe
{
    public partial class MainWindow : Window
    {
        public int Money = 300;
        private Random _random = new();
        public ObservableCollection<Plantation> plantations;
        public List<AmericanPort> AmericanPorts;
        public List<EuropeanPort> EuropeanPorts;
        public List<ShipAmericaToEuropeCost> ShipAmericaToEuropeCosts;
        public List<Ship> Ships;
        private List<Tuple<Plantation, int>> _plantationsInfo;
        private List<Tuple<AmericanPort, int>> _americanPortsInfo;
        private List<Buyer> _buyers;
        private ObservableCollection<BuyerOffer> _buyerOffers = new();
        public ObservableCollection<PlantationToPortDelivery> PlantationToPortDeliveries;
        public ObservableCollection<PortToPortDelivery> PortToPortDeliveries;

        public MainWindow()
        {
            InitializeComponent();
            using var db = new CafeContext();
            plantations = new ObservableCollection<Plantation>(db.plantation.ToList());
            PlantationToPortDeliveries =
                new ObservableCollection<PlantationToPortDelivery>(db.plantation_to_port_delivery.ToList());
            PortToPortDeliveries = new ObservableCollection<PortToPortDelivery>(db.port_to_port_delivery.ToList());
            AmericanPorts = db.american_port.ToList();
            EuropeanPorts = db.european_port.ToList();
            ShipAmericaToEuropeCosts = db.ship_america_to_europe_costs.ToList();
            Ships = db.ship.ToList();
            _buyers = db.buyer.ToList();

            MoneyTextBlock.Text = $"Your money: ${Money}k dollars";

            PlantationsList.ItemsSource = plantations;
            PlantationToPortDeliveriesList.ItemsSource = PlantationToPortDeliveries;
            PortToPortDeliveriesList.ItemsSource = PortToPortDeliveries;

            BuyerOffersList.ItemsSource = _buyerOffers;

            Task.Run(() => StartOfferingBuyers());
        }

        private void StartOfferingBuyers()
        {
            while (true)
            {
                var currentTimeSleepInterval = _random.Next(5, 10);
                Thread.Sleep(currentTimeSleepInterval * 1000);
                GetNewOffer();
            }
        }

        private void GetNewOffer()
        {
            if (Thread.CurrentThread != Dispatcher.Thread)
            {
                Dispatcher.Invoke(
                    DispatcherPriority.Normal, (ThreadStart) GetNewOffer);
            }
            else
            {
                var currentBuyerIndex = _random.Next(_buyers.Count);
                var wantedAmount = _random.Next(100, 300);
                var currentBuyer = _buyers[currentBuyerIndex];
                _buyerOffers.Add(new BuyerOffer(currentBuyer, wantedAmount, DateTime.Now));

                var temporaryOffersList = new ObservableCollection<BuyerOffer>();
                foreach (var offer in _buyerOffers)
                {
                    if ((int) (DateTime.Now - offer.Date).TotalSeconds <= 30)
                    {
                        temporaryOffersList.Add(offer);
                    }
                }

                _buyerOffers = temporaryOffersList;
                BuyerOffersList.ItemsSource = _buyerOffers;

                BuyerOffersList.Items.Refresh();
            }
        }

        private void ButtonBuyNewPlantation_OnClick(object sender, RoutedEventArgs e)
        {
            if (Money < Constants.PlantationBuyCost)
            {
                MessageBox.Show("You don't have enough money to buy new plantation");
                return;
            }

            var popup = new PlantationPurchasePopup(this);
            ShowPopupOnTheCenterOfTheScreen(popup);
            using var db = new CafeContext();
        }

        private void ButtonSellPlantation_OnClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button) sender;
            if (cmd.DataContext is not Plantation) return;
            using var db = new CafeContext();
            var removingPlantation = (Plantation) cmd.DataContext;
            if (removingPlantation.plantation_id <= 10)
            {
                MessageBox.Show("You can not sell plantation with id less than 11");
                return;
            }

            foreach (var plantation in plantations.Where(plantation =>
                plantation.plantation_id == removingPlantation.plantation_id))
            {
                db.Remove(plantation);
                db.SaveChanges();
                plantations.Remove(plantation);
                BuyerOffersList.Items.Refresh();
                Money += Constants.PlantationSellCost;
                MoneyTextBlock.Text = $"Your money: ${Money}k dollars";
                break;
            }
        }

        private void ButtonSendToPort_OnClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button) sender;
            if (cmd.DataContext is not Plantation) return;
            var sendingPlantation = (Plantation) cmd.DataContext;
            var plantationAmericanPortName = sendingPlantation.port_name;
            var biggestDeliveryId = PlantationToPortDeliveries
                .Select(currentDelivery => currentDelivery.plantation_to_port_delivery_id).Prepend(0)
                .Max();
            var coffeeAmount = Constants.TestSendingCoffeeAmountInTon;
            var delivery = new PlantationToPortDelivery
            {
                plantation_to_port_delivery_id = biggestDeliveryId + 1,
                plantation_id = sendingPlantation.plantation_id,
                coffee_amount_in_ton = coffeeAmount,
                date = DateTime.Now
            };
            var db = new CafeContext();
            db.Add(delivery);
            db.SaveChanges();
            PlantationToPortDeliveries.Add(delivery);
            PlantationToPortDeliveriesList.Items.Refresh();
            MessageBox.Show($"You've sent {coffeeAmount} coffee tons from {sendingPlantation.country} to port in {plantationAmericanPortName}");
        }

        private void ButtonAcceptBuyerOffer_OnClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button) sender;
            if (cmd.DataContext is not BuyerOffer) return;
            var buyerOffer = (BuyerOffer) cmd.DataContext;
            var wantedEuropeanPort = buyerOffer.EuropeanPort;
            var wantedCoffeeAmount = buyerOffer.WantedAmount;
            var suitableShipCost = ShipAmericaToEuropeCosts.Find(cost =>
                cost.european_port_name == wantedEuropeanPort.european_port_name &&
                FindShipByName(cost.ship_name).carrying_capacity_in_ton > wantedCoffeeAmount);
            if (suitableShipCost == null)
            {
                MessageBox.Show("Sorry. You can not accept this offer. Suitable ship was not found.");
                return;
            }

            var suitableAmericanPortName = suitableShipCost.american_port_name;
            var biggestDeliveryId = PortToPortDeliveries
                .Select(currentDelivery => currentDelivery.port_to_port_delivery_id).Prepend(0)
                .Max();
            var delivery = new PortToPortDelivery
            {
                port_to_port_delivery_id = biggestDeliveryId + 1,
                delivery_cost_id = suitableShipCost.cost_id,
                buyer_name = buyerOffer.Buyer.buyer_name,
                date = DateTime.Now
            };
            var db = new CafeContext();
            db.Add(delivery);
            db.SaveChanges();
            PortToPortDeliveries.Add(delivery);
            PortToPortDeliveriesList.Items.Refresh();
            Money += wantedCoffeeAmount * buyerOffer.Buyer.purchase_cost;
            MoneyTextBlock.Text = $"Your money: ${Money}k dollars";
            MessageBox.Show($"You've sent {wantedCoffeeAmount} coffee tons from {suitableAmericanPortName} to {wantedEuropeanPort.european_port_name}");
        }

        private Ship FindShipByName(string name)
        {
            return Ships.Find(ship => ship.ship_name == name);
        }

        private static void ShowPopupOnTheCenterOfTheScreen(Window popup)
        {
            var curApp = Application.Current;
            var mainWindow = curApp.MainWindow;
            if (mainWindow != null)
            {
                popup.Left = mainWindow.Left + mainWindow.Width / 2;
                popup.Top = mainWindow.Top + mainWindow.Height / 2;
                popup.ShowDialog();
            }
            else
            {
                MessageBox.Show("Something went wrong. Window can't be displayed");
            }
        }
    }
}
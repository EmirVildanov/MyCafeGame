using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MyCafe.db;
using MyCafe.src;

namespace MyCafe
{
    public partial class PlantationPurchasePopup : Window
    {
        private MainWindow _mainWindow;
        public PlantationPurchasePopup(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void ButtonBuyAccept_OnClick(object sender, RoutedEventArgs e)
        {
            var db = new CafeContext();
            var existedPlantations = _mainWindow.plantations;
            var existedAmericanPortsName = _mainWindow.AmericanPorts.Select(port => port.american_port_name);
            var biggestPlantationId = existedPlantations.Select(plantation => plantation.plantation_id).Prepend(0).Max();
            var chosenCountryObject = (ComboBoxItem) CountryComboBox.SelectedItem;
            var chosenCountry = chosenCountryObject.Content.ToString();
            var chosenManagerName = ManagerTextBox.Text;
            if (string.IsNullOrWhiteSpace(chosenManagerName))
            {
                MessageBox.Show("Enter the manager name");
                return;
            }
            var chosenPortNameObject = (ComboBoxItem) PortComboBox.SelectedItem;
            var chosenPortName = chosenPortNameObject.Content.ToString();
            if (!existedAmericanPortsName.Contains(chosenPortName))
            {
                MessageBox.Show("There is no port with such name");
                Close();
            }
            var buyingPlantation = new Plantation
            {
                plantation_id = biggestPlantationId + 1, country = chosenCountry, manager = chosenManagerName,
                port_name = chosenPortName
            };
            db.plantation.Add(buyingPlantation);
            db.SaveChanges();
            _mainWindow.plantations.Add(buyingPlantation);
            _mainWindow.BuyerOffersList.Items.Refresh();
            _mainWindow.Money -= Constants.PlantationBuyCost;
            _mainWindow.MoneyTextBlock.Text = $"Your money: ${_mainWindow.Money}k dollars";
            Close();
        }
    }
}
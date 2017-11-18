using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Finance.Database;
using Finance.Database.Model;
using Finance.Managers;
using Finance.Resources;
using Finance.Settings;
using Microsoft.Phone.Shell;

namespace Finance
{
    public partial class CurrenciesPage
    {
        private ObservableCollection<Currency> _currencies;
        private Currency _currency;

        public CurrenciesPage()
        {
            InitializeComponent();
            Initialize();
            BuildLocalizedApplicationBar();
            AnimationManager.TurnstileTransition(this);
        }

        private void Initialize()
        {
            _currencies = new ObservableCollection<Currency>();
            CurrenciesListBox.ItemsSource = _currencies;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ProgressManager.ShowMessage(AppResources.Loading);
            switch (AppSettings.NumberOfDigits)
            {
                case 0:
                    ZeroDigitsRadioButton.IsChecked = true;
                    break;
                case 1:
                    OneDigitRadioButton.IsChecked = true;
                    break;
                default:
                    TwoDigitsRadioButton.IsChecked = true;
                    break;
            }
            AutoUpdateSwitch.IsChecked = AppSettings.AutoUpdateCurrencyRates;
            LastUpdateTextBlock.Text = AppSettings.CurrencyRatesLastUpdate == ""
                ? AppResources.NeverSmall
                : AppSettings.CurrencyRatesLastUpdate;
            await ShowCurrencies();
            ProgressManager.HideMessage(AppResources.Loading);
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            ProgressManager.ShowMessage(AppResources.Saving);

            _currency = CurrencyListPicker.SelectedItem as Currency;

            if (ZeroDigitsRadioButton.IsChecked == true) AppSettings.NumberOfDigits = 0;
            if (OneDigitRadioButton.IsChecked == true) AppSettings.NumberOfDigits = 1;
            if (TwoDigitsRadioButton.IsChecked == true) AppSettings.NumberOfDigits = 2;

            AppSettings.AutoUpdateCurrencyRates = AutoUpdateSwitch.IsChecked == true;

            var order = 1;
            foreach (var currency in _currencies)
            {
                currency.Order = order;
                order++;
            }
            await AppDatabase.UpdateCurrenciesAsync(_currencies);

            ProgressManager.HideMessage(AppResources.Saving);
        }

        private async Task ShowCurrencies()
        {
            var currencies = await AppDatabase.SelectCurrenciesAsync();
            currencies = currencies.OrderBy(currency => currency.Order).ToList();
            if (_currency == null)
            {
                _currency =
                    currencies.FirstOrDefault(currency => currency.CharCode == AppSettings.CurrentCurrencyCharCode);
                CurrencyListPicker.ItemsSource = currencies;
                if (_currency != null) CurrencyListPicker.SelectedIndex = currencies.IndexOf(_currency);
            }
            _currencies.Clear();
            foreach (var currency in currencies)
                _currencies.Add(currency);
            NoDataTextBlock.Visibility = _currencies.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            var updateButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/sync.png", UriKind.Relative),
                Text = AppResources.UpdateSmall
            };
            updateButton.Click += UpdateButton_OnClick;
            ApplicationBar.Buttons.Add(updateButton);
        }

        private async void UpdateButton_OnClick(object sender, EventArgs eventArgs)
        {
            CurrencyManager.UpdateCurrencies();

            LastUpdateTextBlock.Text = AppSettings.CurrencyRatesLastUpdate == ""
                ? AppResources.NeverSmall
                : AppSettings.CurrencyRatesLastUpdate;

            ProgressManager.ShowMessage(AppResources.Loading);
            await ShowCurrencies();
            ProgressManager.HideMessage(AppResources.Loading);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (CurrencyListPicker.SelectedItem == null)
            {
                MessageBox.Show(
                    AppResources.AccountCurrencyEmptyMessageText,
                    AppResources.AccountCurrencyEmptyMessageTitle,
                    MessageBoxButton.OK);
                e.Cancel = true;
            }
            else
            {
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
                else
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }

        private async void CurrencyListPicker_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedCurrency = CurrencyListPicker.SelectedItem as Currency;
            if (selectedCurrency != null)
            {
                _currency = await AppDatabase.SelectCurrencyAsync(AppSettings.CurrentCurrencyCharCode);
                await CurrencyManager.ChangeMainCurrencyAsync(_currency, selectedCurrency);
                AppSettings.IsCurrencySelected = true;
                ProgressManager.ShowMessage(AppResources.Loading);
                await ShowCurrencies();
                ProgressManager.HideMessage(AppResources.Loading);
            }
        }
    }
}
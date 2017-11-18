using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Finance.Database;
using Finance.Database.Model;
using Finance.Enums;
using Finance.Managers;
using Finance.Resources;
using Finance.Validation;
using Microsoft.Phone.Shell;

namespace Finance
{
    public partial class AccountPage
    {
        private Account _account;
        private Currency _currency;

        public AccountPage()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
            AnimationManager.SlideTransition(this);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ProgressManager.ShowMessage(AppResources.Loading);
            if (_currency == null)
            {
                var currencies = await AppDatabase.SelectCurrenciesAsync();
                CurrencyListPicker.ItemsSource = currencies.OrderBy(currency => currency.Order);
            }
            if (_account == null)
            {
                if (NavigationContext.QueryString.ContainsKey("AccountId"))
                {
                    var id = Convert.ToInt32(NavigationContext.QueryString["AccountId"]);
                    _account = await AppDatabase.SelectAccountAsync(id);

                    NameTextBox.Text = _account.Name;
                    BalanceTextBox.Text = _account.Balance.ToString("#0.00", CultureInfo.InvariantCulture);

                    var currencies = await AppDatabase.SelectCurrenciesAsync();
                    currencies = currencies.OrderBy(currency => currency.Order).ToList();
                    _currency = currencies.FirstOrDefault(currency => currency.CharCode == _account.CurrencyCharCode);
                    CurrencyListPicker.ItemsSource = currencies;
                    CurrencyListPicker.SelectedIndex = currencies.IndexOf(_currency);
                }
            }
            ProgressManager.HideMessage(AppResources.Loading);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _currency = CurrencyListPicker.SelectedItem as Currency;
            base.OnNavigatedFrom(e);
        }

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            var saveButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/save.png", UriKind.Relative),
                Text = AppResources.SaveSmall
            };
            saveButton.Click += SaveButton_Click;
            ApplicationBar.Buttons.Add(saveButton);
            var cancelButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/cancel.png", UriKind.Relative),
                Text = AppResources.CancelSmall
            };
            cancelButton.Click += (sender, args) => NavigationService.GoBack();
            ApplicationBar.Buttons.Add(cancelButton);
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            if (Validator.EmptyTextBox(NameTextBox, true)) return;
            if (!Validator.EvaluateTextBox(BalanceTextBox, true)) return;

            _currency = CurrencyListPicker.SelectedItem as Currency;
            if (_currency == null)
            {
                MessageBox.Show(AppResources.AccountCurrencyEmptyMessageText,
                    AppResources.AccountCurrencyEmptyMessageTitle, MessageBoxButton.OK);
                return;
            }

            ProgressManager.ShowMessage(AppResources.Saving);

            if (_account == null)
            {
                _account = await Account.CreateAsync();
                _account.Name = NameTextBox.Text;
                _account.Balance = Convert.ToDouble(BalanceTextBox.Text, CultureInfo.InvariantCulture);
                _account.Group = UseGroup.Used.ToString();
                _account.CurrencyCharCode = _currency.CharCode;
                await AppDatabase.InsertAccountAsync(_account);
            }
            else
            {
                _account.Name = NameTextBox.Text;
                _account.Balance = Convert.ToDouble(BalanceTextBox.Text, CultureInfo.InvariantCulture);
                _account.CurrencyCharCode = _currency.CharCode;
                await AppDatabase.UpdateAccountAsync(_account);
            }

            ProgressManager.HideMessage(AppResources.Saving);
            NavigationService.GoBack();
        }

        private void NameTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BalanceTextBox.Focus();
        }

        private void BalanceTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BalanceTextBox.IsEnabled = false;
                BalanceTextBox.IsEnabled = true;
                Validator.EvaluateTextBox(BalanceTextBox);
                CurrencyListPicker.Focus();
            }
        }
    }
}
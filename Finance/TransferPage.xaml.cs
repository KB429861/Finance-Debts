using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Finance.Database;
using Finance.Database.Model;
using Finance.Enums;
using Finance.Global;
using Finance.Managers;
using Finance.Resources;
using Finance.Validation;
using Microsoft.Phone.Shell;

namespace Finance
{
    public partial class TransferPage
    {
        private Account _account1;
        private Account _account2;
        private Currency _currency;
        private Transaction _transaction;

        public TransferPage()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
            AnimationManager.SlideTransition(this);
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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ProgressManager.ShowMessage(AppResources.Loading);

            var accounts = await AppDatabase.SelectAccountsAsync(UseGroup.Used);
            accounts = accounts.OrderBy(account => account.Order).ToList();
            if (accounts.Count > Account1ListPicker.Items.Count)
                Account1ListPicker.ItemsSource = accounts;
            if (accounts.Count > Account2ListPicker.Items.Count)
                Account2ListPicker.ItemsSource = accounts;

            if (_currency == null)
            {
                var currencies = await AppDatabase.SelectCurrenciesAsync();
                CurrencyListPicker.ItemsSource = currencies.OrderBy(currency => currency.Order);
            }

            if (_transaction == null)
            {
                if (NavigationContext.QueryString.ContainsKey("TransactionId"))
                {
                    var transactionId = Convert.ToInt32(NavigationContext.QueryString["TransactionId"]);
                    _transaction = await AppDatabase.SelectTransactionAsync(transactionId);

                    if (_transaction != null)
                    {
                        _account1 = accounts.FirstOrDefault(account => account.AccountId == _transaction.Account1Id);
                        Account1ListPicker.ItemsSource = accounts;
                        Account1ListPicker.SelectedIndex = accounts.IndexOf(_account1);

                        _account2 = accounts.FirstOrDefault(account => account.AccountId == _transaction.Account2Id);
                        Account2ListPicker.ItemsSource = accounts;
                        Account2ListPicker.SelectedIndex = accounts.IndexOf(_account2);

                        var currencies = await AppDatabase.SelectCurrenciesAsync();
                        currencies = currencies.OrderBy(currency => currency.Order).ToList();
                        _currency =
                            currencies.FirstOrDefault(currency => currency.CharCode == _transaction.CurrencyCharCode);
                        CurrencyListPicker.ItemsSource = currencies;
                        CurrencyListPicker.SelectedIndex = currencies.IndexOf(_currency);

                        TransferDatePicker.Value = _transaction.Date;
                        TransferTimePicker.Value = _transaction.Date;

                        AmountTextBox.Text = _transaction.Amount.ToString("#0.00", CultureInfo.InvariantCulture);
                        DescriptionTextBox.Text = _transaction.Description;
                    }
                }
            }
            ProgressManager.HideMessage(AppResources.Loading);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _account1 = Account1ListPicker.SelectedItem as Account;
            _account2 = Account2ListPicker.SelectedItem as Account;
            _currency = CurrencyListPicker.SelectedItem as Currency;
            base.OnNavigatedFrom(e);
        }

        private void AddAccountButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AccountPage.xaml", UriKind.Relative));
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            _account1 = Account1ListPicker.SelectedItem as Account;
            if (_account1 == null)
            {
                MessageBox.Show(AppResources.AccountEmptyMessageText, AppResources.AccountEmptyMessageTitle,
                    MessageBoxButton.OK);
                Account1ListPicker.Focus();
                return;
            }

            _account2 = Account2ListPicker.SelectedItem as Account;
            if (_account2 == null)
            {
                MessageBox.Show(AppResources.AccountEmptyMessageText, AppResources.AccountEmptyMessageTitle,
                    MessageBoxButton.OK);
                Account2ListPicker.Focus();
                return;
            }

            _currency = CurrencyListPicker.SelectedItem as Currency;
            if (_currency == null)
            {
                MessageBox.Show(AppResources.AccountCurrencyEmptyMessageText,
                    AppResources.AccountCurrencyEmptyMessageTitle, MessageBoxButton.OK);
                CurrencyListPicker.Focus();
                return;
            }

            if (!Validator.EvaluateTextBox(AmountTextBox, true)) return;

            ProgressManager.ShowMessage(AppResources.Saving);

            var currencies = await AppDatabase.SelectCurrenciesAsync();

            if (_transaction != null)
            {
                var account1 = await AppDatabase.SelectAccountAsync(_transaction.Account1Id);
                var account2 = await AppDatabase.SelectAccountAsync(_transaction.Account2Id);
                if (account1 != null && account2 != null)
                {
                    var transactionCurrency =
                        currencies.FirstOrDefault(currency => currency.CharCode == _transaction.CurrencyCharCode);
                    var account1Currency =
                        currencies.FirstOrDefault(currency => currency.CharCode == account1.CurrencyCharCode);
                    var account2Currency =
                        currencies.FirstOrDefault(currency => currency.CharCode == account2.CurrencyCharCode);
                    var value1 = CurrencyManager.ConvertToCurrency(transactionCurrency, account1Currency,
                        _transaction.Amount);
                    var value2 = CurrencyManager.ConvertToCurrency(transactionCurrency, account2Currency,
                        _transaction.Amount);
                    account1.Balance += value1;
                    account2.Balance -= value2;
                    await AppDatabase.UpdateAccountAsync(account1);
                    await AppDatabase.UpdateAccountAsync(account2);
                    await AppDatabase.DeleteTransactionAsync(_transaction);
                    _transaction = null;
                }
            }
            if (_transaction == null)
                _transaction = await Transaction.CreateAsync();

            _transaction.Account1Id = _account1.AccountId;
            _transaction.Account2Id = _account2.AccountId;
            _transaction.CurrencyCharCode = _currency.CharCode;
            _transaction.Type = TransactionType.TRANSFER.ToString();
            _transaction.Description = DescriptionTextBox.Text;

            var transactionCurrency2 =
                currencies.FirstOrDefault(currency => currency.CharCode == _transaction.CurrencyCharCode);
            var accountCurrency1 = currencies.FirstOrDefault(currency => currency.CharCode == _account1.CurrencyCharCode);
            var accountCurrency2 = currencies.FirstOrDefault(currency => currency.CharCode == _account2.CurrencyCharCode);

            var amount = Convert.ToDouble(AmountTextBox.Text, CultureInfo.InvariantCulture);
            var val1 = CurrencyManager.ConvertToCurrency(transactionCurrency2, accountCurrency1, amount);
            var val2 = CurrencyManager.ConvertToCurrency(transactionCurrency2, accountCurrency2, amount);
            _transaction.Amount = amount;
            _account1.Balance -= val1;
            _account2.Balance += val2;

            if (TransferDatePicker.Value != null)
                _transaction.Date = TransferDatePicker.Value.Value.Date;
            if (TransferTimePicker.Value != null)
                _transaction.Date += TransferTimePicker.Value.Value.TimeOfDay;

            await AppDatabase.UpdateAccountAsync(_account1);
            await AppDatabase.UpdateAccountAsync(_account2);
            await AppDatabase.InsertTransactionAsync(_transaction);

            ProgressManager.HideMessage(AppResources.Saving);
            NavigationService.GoBack();
        }

        private void DescriptionTextBox_OnTap(object sender, GestureEventArgs e)
        {
            DescriptionTextBox.IsReadOnly = AppGlobal.IsTrial;
            if (AppGlobal.IsTrial)
                AppGlobal.ShowTrialMessage();
        }

        private void AmountTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Validator.EvaluateTextBox(AmountTextBox);
                DescriptionTextBox.Focus();
            }
        }

        private void DescriptionTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DescriptionTextBox.IsReadOnly = AppGlobal.IsTrial;
                if (AppGlobal.IsTrial)
                    AppGlobal.ShowTrialMessage();
                else
                    DescriptionTextBox.IsEnabled = false;
                DescriptionTextBox.IsEnabled = true;
            }
        }
    }
}
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
    public partial class DebtPage
    {
        private Account _account;
        private Currency _currency;
        private Person _person;
        private Transaction _transaction;
        private string _transactionType;

        public DebtPage()
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

            var people = await AppDatabase.SelectPeopleAsync();
            people = people.OrderBy(person => person.Order).ToList();
            if (people.Count > PersonListPicker.Items.Count)
                PersonListPicker.ItemsSource = people;
            if (_person == null)
            {
                if (NavigationContext.QueryString.ContainsKey("PersonId"))
                {
                    var personId = Convert.ToInt32(NavigationContext.QueryString["PersonId"]);
                    _person = people.FirstOrDefault(person => person.PersonId == personId);
                    PersonListPicker.SelectedIndex = people.IndexOf(_person);
                }
            }

            var accounts = await AppDatabase.SelectAccountsAsync(UseGroup.Used);
            accounts = accounts.OrderBy(account => account.Order).ToList();
            if (accounts.Count > AccountsListPicker.Items.Count)
                AccountsListPicker.ItemsSource = accounts;

            if (_currency == null)
            {
                var currencies = await AppDatabase.SelectCurrenciesAsync();
                CurrencyListPicker.ItemsSource = currencies.OrderBy(currency => currency.Order);
            }
            if (_transactionType == null)
            {
                if (NavigationContext.QueryString.ContainsKey("Type"))
                {
                    _transactionType = NavigationContext.QueryString["Type"];
                    if (_transactionType == TransactionType.LEND.ToString())
                        GiveRadioButton.IsChecked = true;
                    if (_transactionType == TransactionType.BORROW.ToString())
                        TakeRadioButton.IsChecked = true;
                }
            }
            if (_transaction == null)
            {
                if (NavigationContext.QueryString.ContainsKey("TransactionId"))
                {
                    var transactionId = Convert.ToInt32(NavigationContext.QueryString["TransactionId"]);
                    _transaction = await AppDatabase.SelectTransactionAsync(transactionId);

                    _person = people.FirstOrDefault(person => person.PersonId == _transaction.PersonId);
                    PersonListPicker.ItemsSource = people;
                    PersonListPicker.SelectedIndex = people.IndexOf(_person);

                    _account = accounts.FirstOrDefault(account => account.AccountId == _transaction.AccountId);
                    if (!accounts.Contains(_account)) accounts.Add(_account);
                    accounts = accounts.OrderBy(account => account.Order).ToList();
                    AccountsListPicker.ItemsSource = accounts;
                    AccountsListPicker.SelectedIndex = accounts.IndexOf(_account);

                    var currencies = await AppDatabase.SelectCurrenciesAsync();
                    currencies = currencies.OrderBy(currency => currency.Order).ToList();
                    _currency = currencies.FirstOrDefault(currency => currency.CharCode == _transaction.CurrencyCharCode);
                    CurrencyListPicker.ItemsSource = currencies;
                    CurrencyListPicker.SelectedIndex = currencies.IndexOf(_currency);

                    if (_transaction != null)
                    {
                        TransactionDatePicker.Value = _transaction.Date;
                        TransactionTimePicker.Value = _transaction.Date;

                        if (_transaction.Amount >= 0)
                        {
                            _transactionType = TransactionType.BORROW.ToString();
                            TakeRadioButton.IsChecked = true;
                            AmountTextBox.Text = _transaction.Amount.ToString("#0.00", CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            _transactionType = TransactionType.LEND.ToString();
                            GiveRadioButton.IsChecked = true;
                            AmountTextBox.Text = (-_transaction.Amount).ToString("#0.00", CultureInfo.InvariantCulture);
                        }

                        DescriptionTextBox.Text = _transaction.Description;
                    }
                }
            }
            ProgressManager.HideMessage(AppResources.Loading);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _person = PersonListPicker.SelectedItem as Person;
            _account = AccountsListPicker.SelectedItem as Account;
            _currency = CurrencyListPicker.SelectedItem as Currency;

            if (GiveRadioButton.IsChecked == true)
                _transactionType = TransactionType.LEND.ToString();
            else if (TakeRadioButton.IsChecked == true)
                _transactionType = TransactionType.BORROW.ToString();

            base.OnNavigatedFrom(e);
        }

        private void AddPersonButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/PersonPage.xaml", UriKind.Relative));
        }

        private void AddAccountButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AccountPage.xaml", UriKind.Relative));
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            _person = PersonListPicker.SelectedItem as Person;
            if (_person == null)
            {
                MessageBox.Show(AppResources.PersonEmptyMessageText, AppResources.PersonEmptyMessageTitle,
                    MessageBoxButton.OK);
                PersonListPicker.Focus();
                return;
            }

            _account = AccountsListPicker.SelectedItem as Account;
            if (_account == null)
            {
                MessageBox.Show(AppResources.AccountEmptyMessageText, AppResources.AccountEmptyMessageTitle,
                    MessageBoxButton.OK);
                AccountsListPicker.Focus();
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
                var account = await AppDatabase.SelectAccountAsync(_transaction.AccountId);
                if (account != null)
                {
                    var transactionCurrency =
                        currencies.FirstOrDefault(currency => currency.CharCode == _transaction.CurrencyCharCode);
                    var accountCurrency =
                        currencies.FirstOrDefault(currency => currency.CharCode == account.CurrencyCharCode);
                    var value = CurrencyManager.ConvertToCurrency(transactionCurrency, accountCurrency,
                        _transaction.Amount);
                    account.Balance -= value;
                    await AppDatabase.UpdateAccountAsync(account);
                    await AppDatabase.DeleteTransactionAsync(_transaction);
                    _transaction = null;
                }
            }
            if (_transaction == null)
                _transaction = await Transaction.CreateAsync();

            _account = await AppDatabase.SelectAccountAsync(_account.AccountId);

            if (GiveRadioButton.IsChecked == true)
                _transactionType = TransactionType.LEND.ToString();
            else if (TakeRadioButton.IsChecked == true)
                _transactionType = TransactionType.BORROW.ToString();

            _transaction.PersonId = _person.PersonId;
            _transaction.AccountId = _account.AccountId;
            _transaction.CurrencyCharCode = _currency.CharCode;
            _transaction.Type = _transactionType;
            _transaction.Description = DescriptionTextBox.Text;

            var transactionCurrency2 =
                currencies.FirstOrDefault(currency => currency.CharCode == _transaction.CurrencyCharCode);
            var accountCurrency2 = currencies.FirstOrDefault(currency => currency.CharCode == _account.CurrencyCharCode);

            var amount = Convert.ToDouble(AmountTextBox.Text, CultureInfo.InvariantCulture);
            var value2 = CurrencyManager.ConvertToCurrency(transactionCurrency2, accountCurrency2, amount);

            if (_transactionType == TransactionType.LEND.ToString())
            {
                _account.Balance -= value2;
                _transaction.Amount = -amount;
            }
            else if (_transactionType == TransactionType.BORROW.ToString())
            {
                _account.Balance += value2;
                _transaction.Amount = amount;
            }

            if (TransactionDatePicker.Value != null)
                _transaction.Date = TransactionDatePicker.Value.Value.Date;
            if (TransactionTimePicker.Value != null)
                _transaction.Date += TransactionTimePicker.Value.Value.TimeOfDay;

            await AppDatabase.UpdateAccountAsync(_account);
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
                {
                    DescriptionTextBox.IsEnabled = false;
                    DescriptionTextBox.IsEnabled = true;
                }
            }
        }
    }
}
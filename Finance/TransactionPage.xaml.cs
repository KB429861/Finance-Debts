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
    public partial class TransactionPage
    {
        private Account _account;
        private Category _category;
        private Currency _currency;
        private Transaction _transaction;
        private string _transactionType;

        public TransactionPage()
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

            if (!string.IsNullOrWhiteSpace(_transactionType))
            {
                var categories = await AppDatabase.SelectCategoriesAsync(_transactionType);
                if (categories.Count > CategoriesListPicker.Items.Count)
                    CategoriesListPicker.ItemsSource = categories.OrderBy(category => category.Order);
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
                    if (_transactionType == TransactionType.INCOME.ToString())
                        TransactionPivot.Header = AppResources.IncomesSmall.ToUpperInvariant();
                    if (_transactionType == TransactionType.EXPENCE.ToString())
                        TransactionPivot.Header = AppResources.ExpencesSmall.ToUpperInvariant();
                    var categories = await AppDatabase.SelectCategoriesAsync(_transactionType);
                    CategoriesListPicker.ItemsSource = categories.OrderBy(category => category.Order);
                }
            }
            if (_transaction == null)
            {
                if (NavigationContext.QueryString.ContainsKey("TransactionId"))
                {
                    var transactionId = Convert.ToInt32(NavigationContext.QueryString["TransactionId"]);
                    _transaction = await AppDatabase.SelectTransactionAsync(transactionId);

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
                            _transactionType = TransactionType.INCOME.ToString();
                            TransactionPivot.Header = AppResources.IncomesSmall.ToUpperInvariant();
                            AmountTextBox.Text = _transaction.Amount.ToString("#0.00", CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            _transactionType = TransactionType.EXPENCE.ToString();
                            TransactionPivot.Header = AppResources.ExpencesSmall.ToUpperInvariant();
                            AmountTextBox.Text = (-_transaction.Amount).ToString("#0.00", CultureInfo.InvariantCulture);
                        }

                        var categories = await AppDatabase.SelectCategoriesAsync(_transaction.Type);
                        categories = categories.OrderBy(category => category.Order).ToList();
                        _category = categories.FirstOrDefault(category => category.CategoryId == _transaction.CategoryId);
                        CategoriesListPicker.ItemsSource = categories;
                        CategoriesListPicker.SelectedIndex = categories.IndexOf(_category);

                        DescriptionTextBox.Text = _transaction.Description;
                    }
                }
            }
            ProgressManager.HideMessage(AppResources.Loading);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _category = CategoriesListPicker.SelectedItem as Category;
            _account = AccountsListPicker.SelectedItem as Account;
            _currency = CurrencyListPicker.SelectedItem as Currency;
            base.OnNavigatedFrom(e);
        }

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/CategoryPage.xaml?Type=" + _transactionType, UriKind.Relative));
        }

        private void AddAccountButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AccountPage.xaml", UriKind.Relative));
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            _category = CategoriesListPicker.SelectedItem as Category;
            if (_category == null)
            {
                MessageBox.Show(AppResources.CategoryEmptyMessageText, AppResources.CategoryEmptyMessageTitle,
                    MessageBoxButton.OK);
                CategoriesListPicker.Focus();
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

            _transaction.CategoryId = _category.CategoryId;
            _transaction.AccountId = _account.AccountId;
            _transaction.CurrencyCharCode = _currency.CharCode;
            _transaction.Type = _transactionType;
            _transaction.Description = DescriptionTextBox.Text;

            var transactionCurrency2 =
                currencies.FirstOrDefault(currency => currency.CharCode == _transaction.CurrencyCharCode);
            var accountCurrency2 = currencies.FirstOrDefault(currency => currency.CharCode == _account.CurrencyCharCode);

            var amount = Convert.ToDouble(AmountTextBox.Text, CultureInfo.InvariantCulture);
            var value2 = CurrencyManager.ConvertToCurrency(transactionCurrency2, accountCurrency2, amount);

            if (_transactionType == TransactionType.EXPENCE.ToString())
            {
                _account.Balance -= value2;
                _transaction.Amount = -amount;
            }
            else if (_transactionType == TransactionType.INCOME.ToString())
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
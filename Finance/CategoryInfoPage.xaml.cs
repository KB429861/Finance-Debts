using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Finance.Database;
using Finance.Database.Model;
using Finance.Enums;
using Finance.Managers;
using Finance.Other;
using Finance.Resources;
using Microsoft.Phone.Shell;

namespace Finance
{
    public partial class CategoryInfoPage
    {
        private Category _category;
        private ObservableCollection<Group<Transaction>> _transactions;

        public CategoryInfoPage()
        {
            InitializeComponent();
            Initialize();
            AnimationManager.TurnstileTransition(this);
        }

        private void Initialize()
        {
            _transactions = new ObservableCollection<Group<Transaction>>();
            TransactionsLongListSelector.ItemsSource = _transactions;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ProgressManager.ShowMessage(AppResources.Loading);
            if (_category == null)
            {
                var id = 1;
                if (NavigationContext.QueryString.ContainsKey("CategoryId"))
                    id = Convert.ToInt32(NavigationContext.QueryString["CategoryId"]);
                _category = await AppDatabase.SelectCategoryAsync(id);
                var categories = await AppDatabase.SelectCategoriesAsync(_category.Type);
                categories = categories.OrderBy(category => category.Order).ToList();
                _category = categories.FirstOrDefault(category => category.CategoryId == id);
                CategoryListPicker.ItemsSource = categories;
                CategoryListPicker.SelectedIndex = categories.IndexOf(_category);
            }
            UpdateHeader(_category);
            await ShowTransactions();
            UpdateApplicationBar();
            ProgressManager.HideMessage(AppResources.Loading);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _category = CategoryListPicker.SelectedItem as Category;
            base.OnNavigatedFrom(e);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            TransactionsLongListSelector.SelectedItems.Clear();
        }

        private DateTime? QueryFromDate()
        {
            if (NavigationContext.QueryString.ContainsKey("FromDate"))
                return Convert.ToDateTime(NavigationContext.QueryString["FromDate"]);
            return null;
        }

        private DateTime? QueryToDate()
        {
            if (NavigationContext.QueryString.ContainsKey("ToDate"))
                return Convert.ToDateTime(NavigationContext.QueryString["ToDate"]);
            return null;
        }

        private void UpdateHeader(Category category)
        {
            if (category.Type == TransactionType.EXPENCE.ToString())
                TransactionsPivotItem.Header = AppResources.ExpencesSmall.ToUpperInvariant();
            else if (category.Type == TransactionType.INCOME.ToString())
                TransactionsPivotItem.Header = AppResources.IncomesSmall.ToUpperInvariant();
        }

        private async Task ShowTransactions()
        {
            var transactions = await AppDatabase.SelectTransactionsAsync(_category);
            var startDateTime = QueryFromDate();
            var endDateTime = QueryToDate();
            if (startDateTime != null && endDateTime != null)
                transactions =
                    transactions.Where(
                        transaction =>
                            transaction.Date.Date >= startDateTime.Value.Date &&
                            transaction.Date.Date <= endDateTime.Value.Date).ToList();

            foreach (var transaction in transactions)
                transaction.Group = transaction.Date.Date.ToLongDateString();

            _transactions.Clear();
            foreach (
                var item in
                    Group<Transaction>.GetItemGroups(
                        transactions.OrderByDescending(transaction => transaction.Date.Date),
                        transaction => transaction.Group)) _transactions.Add(item);

            NoDataTextBlock.Visibility = _transactions.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateApplicationBar()
        {
            if (TransactionsLongListSelector.SelectedItems.Count == 0)
                BuildSelectApplicationBar();
            if (TransactionsLongListSelector.SelectedItems.Count == 1)
                BuildEditApplicationBar();
            if (TransactionsLongListSelector.SelectedItems.Count > 1)
                BuildDeleteApplicationBar();
        }

        private void BuildSelectApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            var selectButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("Toolkit.Content/ApplicationBar.Select.png", UriKind.Relative),
                Text = AppResources.ChooseSmall
            };
            selectButton.Click += SelectButton_OnClick;
            ApplicationBar.Buttons.Add(selectButton);
        }

        private void BuildEditApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            var editButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/edit.png", UriKind.Relative),
                Text = AppResources.EditSmall
            };
            editButton.Click += EditButton_OnClick;
            ApplicationBar.Buttons.Add(editButton);
            var deleteButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/delete.png", UriKind.Relative),
                Text = AppResources.DeleteSmall
            };
            deleteButton.Click += DeleteButton_OnClick;
            ApplicationBar.Buttons.Add(deleteButton);
        }

        private void BuildDeleteApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            var deleteButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/delete.png", UriKind.Relative),
                Text = AppResources.DeleteSmall
            };
            deleteButton.Click += DeleteButton_OnClick;
            ApplicationBar.Buttons.Add(deleteButton);
        }

        private void SelectButton_OnClick(object sender, EventArgs e)
        {
            TransactionsLongListSelector.EnforceIsSelectionEnabled =
                !TransactionsLongListSelector.EnforceIsSelectionEnabled;
        }

        private void EditButton_OnClick(object sender, EventArgs e)
        {
            var transaction = (Transaction) TransactionsLongListSelector.SelectedItems[0];
            NavigationService.Navigate(new Uri("/TransactionPage.xaml?TransactionId=" + transaction.TransactionId,
                UriKind.Relative));
        }

        private async void DeleteButton_OnClick(object sender, EventArgs e)
        {
            var result = MessageBox.Show(AppResources.TransactionsDeleteMessageText,
                AppResources.TransactionsDeleteMessageTitle, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                ProgressManager.ShowMessage(AppResources.Deleting);
                foreach (Transaction transaction in TransactionsLongListSelector.SelectedItems)
                {
                    var account = await AppDatabase.SelectAccountAsync(transaction.AccountId);
                    var transactionCurrency = await AppDatabase.SelectCurrencyAsync(transaction.CurrencyCharCode);
                    var accountCurrency = await AppDatabase.SelectCurrencyAsync(account.CurrencyCharCode);
                    var value = CurrencyManager.ConvertToCurrency(transactionCurrency, accountCurrency,
                        transaction.Amount);
                    account.Balance -= value;
                    await AppDatabase.UpdateAccountAsync(account);
                    RemoveTransaction(transaction);
                    await AppDatabase.DeleteTransactionAsync(transaction);
                }
                TransactionsLongListSelector.SelectedItems.Clear();
                ProgressManager.HideMessage(AppResources.Deleting);
            }
        }

        private void RemoveTransaction(Transaction transaction)
        {
            foreach (Collection<Transaction> list in TransactionsLongListSelector.ItemsSource)
            {
                if (list.Contains(transaction))
                {
                    list.Remove(transaction);
                    break;
                }
            }
        }

        private void CategoryListPicker_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AnimationManager.OpacityAnimation(ContentGrid, 1, 0);
            AnimationManager.ContentVerticalAnimation(ContentGrid, 0, 800, async (o, args) =>
            {
                _category = CategoryListPicker.SelectedItem as Category;
                ProgressManager.ShowMessage(AppResources.Loading);
                if (_category != null)
                    await ShowTransactions();
                ProgressManager.HideMessage(AppResources.Loading);
                AnimationManager.OpacityAnimation(ContentGrid, 0, 1);
                AnimationManager.ContentVerticalAnimation(ContentGrid, 800, 0);
            });
        }

        private void LongListMultiSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateApplicationBar();
        }
    }
}
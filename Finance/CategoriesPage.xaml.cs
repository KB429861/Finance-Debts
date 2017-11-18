using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Finance.Database;
using Finance.Database.Model;
using Finance.Enums;
using Finance.Managers;
using Finance.Resources;
using Microsoft.Phone.Shell;

namespace Finance
{
    public partial class CategoriesPage
    {
        private ObservableCollection<Category> _expenceCategories;
        private ObservableCollection<Category> _incomeCategories;

        public CategoriesPage()
        {
            InitializeComponent();
            Initialize();
            BuildLocalizedApplicationBar();
            AnimationManager.TurnstileTransition(this);
        }

        private void Initialize()
        {
            _expenceCategories = new ObservableCollection<Category>();
            ExpenceCategoriesListBox.ItemsSource = _expenceCategories;

            _incomeCategories = new ObservableCollection<Category>();
            IncomeCategoriesListBox.ItemsSource = _incomeCategories;
        }

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            var addButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/add.png", UriKind.Relative),
                Text = AppResources.AddSmall
            };
            addButton.Click += AddButton_Click;
            ApplicationBar.Buttons.Add(addButton);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ProgressManager.ShowMessage(AppResources.Loading);

            var incomeCategories = await AppDatabase.SelectCategoriesAsync(TransactionType.INCOME);
            _incomeCategories.Clear();
            foreach (var category in incomeCategories.OrderBy(category => category.Order))
                _incomeCategories.Add(category);
            NoIncomesDataTextBlock.Visibility = _incomeCategories.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            var expenceCategories = await AppDatabase.SelectCategoriesAsync(TransactionType.EXPENCE);
            _expenceCategories.Clear();
            foreach (var category in expenceCategories.OrderBy(category => category.Order))
                _expenceCategories.Add(category);
            NoExpenceDataTextBlock.Visibility = _expenceCategories.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;

            ProgressManager.HideMessage(AppResources.Loading);
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            ProgressManager.ShowMessage(AppResources.Saving);

            var order = 1;
            foreach (var category in _incomeCategories)
            {
                category.Order = order;
                order++;
            }
            await AppDatabase.UpdateCategoriesAsync(_incomeCategories);

            order = 1;
            foreach (var category in _expenceCategories)
            {
                category.Order = order;
                order++;
            }
            await AppDatabase.UpdateCategoriesAsync(_expenceCategories);

            ProgressManager.HideMessage(AppResources.Saving);
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (CategoriesPivot.SelectedIndex == 0)
                NavigationService.Navigate(new Uri("/CategoryPage.xaml?Type=" + TransactionType.INCOME, UriKind.Relative));
            if (CategoriesPivot.SelectedIndex == 1)
                NavigationService.Navigate(new Uri("/CategoryPage.xaml?Type=" + TransactionType.EXPENCE,
                    UriKind.Relative));
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            var element = (FrameworkElement) sender;
            var category = element.DataContext as Category;
            if (category != null)
                NavigationService.Navigate(new Uri("/CategoryPage.xaml?CategoryId=" + category.CategoryId,
                    UriKind.Relative));
        }

        private async void DeleteButton_Click(object sender, EventArgs e)
        {
            var element = (FrameworkElement) sender;
            var category = element.DataContext as Category;
            if (category != null)
            {
                var result = MessageBox.Show(AppResources.CategoryDeleteMessageText,
                    AppResources.CategoryDeleteMessageTitle, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    result = MessageBox.Show(AppResources.CategoryDeleteTransactionsMessageText,
                        AppResources.CategoryDeleteTransactionsMessageTitle, MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        ProgressManager.ShowMessage(AppResources.Deleting);
                        result = MessageBox.Show(AppResources.CashbackMessageText, AppResources.CashbackMessageTitle,
                            MessageBoxButton.OKCancel);
                        var transactions = await AppDatabase.SelectTransactionsAsync(category);
                        if (result == MessageBoxResult.OK)
                        {
                            foreach (var transaction in transactions)
                            {
                                var amount = transaction.Amount;
                                var account = await AppDatabase.SelectAccountAsync(transaction.AccountId);
                                var transactionCurrency =
                                    await AppDatabase.SelectCurrencyAsync(transaction.CurrencyCharCode);
                                var accountCurrency = await AppDatabase.SelectCurrencyAsync(account.CurrencyCharCode);
                                var value = CurrencyManager.ConvertToCurrency(transactionCurrency, accountCurrency,
                                    amount);
                                account.Balance -= value;
                                await AppDatabase.UpdateAccountAsync(account);
                            }
                        }
                        await AppDatabase.DeleteTransactionsAsync(transactions);
                        if (category.Type == TransactionType.INCOME.ToString())
                        {
                            _incomeCategories.Remove(category);
                            NoIncomesDataTextBlock.Visibility = _incomeCategories.Count == 0
                                ? Visibility.Visible
                                : Visibility.Collapsed;
                        }
                        if (category.Type == TransactionType.EXPENCE.ToString())
                        {
                            _expenceCategories.Remove(category);
                            NoExpenceDataTextBlock.Visibility = _expenceCategories.Count == 0
                                ? Visibility.Visible
                                : Visibility.Collapsed;
                        }
                        await AppDatabase.DeleteCategoryAsync(category);
                        ProgressManager.HideMessage(AppResources.Deleting);
                    }
                }
            }
        }
    }
}
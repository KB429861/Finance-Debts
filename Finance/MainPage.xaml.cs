using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Finance.Database;
using Finance.Database.Model;
using Finance.Enums;
using Finance.Global;
using Finance.Managers;
using Finance.Resources;
using Finance.Settings;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Finance
{
    public partial class MainPage
    {
        private DateTime _expenceDateTime = DateTime.Now;
        private DateTime _incomeDateTime = DateTime.Now;

        public MainPage()
        {
            InitializeComponent();
            MainPivot.SelectedIndex = AppSettings.MainPivotSelectedIndex;
            AnimationManager.TurnstileTransition(this);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ProgressManager.ShowMessage(AppResources.Loading);
            var showAccounts = ShowAccounts();
            var showDebtors = ShowDebtors();
            var showExpenseCategories = ShowExpenceCategories(_expenceDateTime > DateTime.Now);
            var showIncomeCategories = ShowIncomeCategories(_incomeDateTime > DateTime.Now);
            while (NavigationService.BackStack.Any())
                NavigationService.RemoveBackEntry();
            await showAccounts;
            await showDebtors;
            await showExpenseCategories;
            await showIncomeCategories;
            ProgressManager.HideMessage(AppResources.Loading);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            AppSettings.MainPivotSelectedIndex = MainPivot.SelectedIndex;
            base.OnNavigatedFrom(e);
        }

        private async Task ShowAccounts()
        {
            var accounts = await AppDatabase.SelectAccountsAsync(UseGroup.Used);
            foreach (var account in accounts)
                account.CurrentBalance = await account.CalculateCurrentBalance();
            NoAccountsDataTextBlock.Visibility = accounts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            AccountsListBox.ItemsSource = accounts.OrderByDescending(account => account.CurrentBalance);
            BalanceTotalControl.AmountValue = accounts.Sum(account => account.CurrentBalance);
        }

        private void EditAccount_OnClick(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement) sender;
            var account = element.DataContext as Account;
            if (account != null)
                NavigationService.Navigate(new Uri("/AccountPage.xaml?AccountId=" + account.AccountId, UriKind.Relative));
        }

        private async Task ShowDebtors()
        {
            var people = await AppDatabase.SelectPeopleAsync();
            foreach (var person in people)
                person.Amount = await person.CalculateAmount();
            people =
                people.Where(person => Math.Abs(person.Amount) > double.Epsilon)
                    .OrderByDescending(person => person.Amount)
                    .ToList();
            NoDebtsDataTextBlock.Visibility = people.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            DebtorsListBox.ItemsSource = people;
            DebtsTotalControl.AmountValue = people.Sum(person => person.Amount);
        }

        private void DebtorsListBox_OnTap(object sender, GestureEventArgs e)
        {
            var person = DebtorsListBox.SelectedItem as Person;
            if (person != null)
                NavigationService.Navigate(new Uri("/PersonInfoPage.xaml?PersonId=" + person.PersonId, UriKind.Relative));
        }

        private void GiveButton_Click(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement) sender;
            var person = element.DataContext as Person;
            if (person != null)
                NavigationService.Navigate(
                    new Uri("/DebtPage.xaml?Type=" + TransactionType.LEND + "&PersonId=" + person.PersonId,
                        UriKind.Relative));
        }

        private void TakeButton_Click(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement) sender;
            var person = element.DataContext as Person;
            if (person != null)
                NavigationService.Navigate(
                    new Uri("/DebtPage.xaml?Type=" + TransactionType.BORROW + "&PersonId=" + person.PersonId,
                        UriKind.Relative));
        }

        private async void ForgetButton_OnClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(AppResources.ForgetMessageText, AppResources.ForgetMessageTitle,
                MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                var element = (FrameworkElement) sender;
                var person = element.DataContext as Person;
                if (person != null)
                {
                    ProgressManager.ShowMessage(AppResources.Deleting);
                    var transactions = await AppDatabase.SelectTransactionsAsync(person);
                    await AppDatabase.DeleteTransactionsAsync(transactions);
                    ProgressManager.HideMessage(AppResources.Deleting);
                    ProgressManager.ShowMessage(AppResources.Loading);
                    await ShowAccounts();
                    await ShowDebtors();
                    ProgressManager.HideMessage(AppResources.Loading);
                }
            }
        }

        private async Task ShowExpenceCategories(bool showAll = false)
        {
            ExpenseMonthControl.NameText = showAll
                ? AppResources.TotalNormal
                : _expenceDateTime.ToString("MMMM yyyy", CultureInfo.CurrentUICulture);
            ExpenseMonthControl.TodayVisibility = _expenceDateTime.Date == DateTime.Today
                ? Visibility.Visible
                : Visibility.Collapsed;
            var transactions = await AppDatabase.SelectTransactionsAsync(TransactionType.EXPENCE);
            var categories = await AppDatabase.SelectCategoriesAsync(TransactionType.EXPENCE);
            DateTime startDateTime;
            DateTime endDateTime;
            if (showAll)
            {
                startDateTime = transactions.Min(transaction => transaction.Date);
                endDateTime = transactions.Max(transaction => transaction.Date);
            }
            else
            {
                startDateTime = new DateTime(_expenceDateTime.Year, _expenceDateTime.Month, 1);
                endDateTime = new DateTime(_expenceDateTime.Year, _expenceDateTime.Month,
                    DateTime.DaysInMonth(_expenceDateTime.Year, _expenceDateTime.Month));
            }
            foreach (var category in categories)
                category.Amount = await category.CalculateAmount(startDateTime, endDateTime);
            categories =
                categories.Where(category => Math.Abs(category.Amount) > double.Epsilon)
                    .OrderBy(category => category.Amount)
                    .ToList();
            NoExpenceDataTextBlock.Visibility = categories.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            ExpenceListBox.ItemsSource = categories;
            ExpenseMonthControl.MonthAmountValue = categories.Sum(category => category.Amount);
            transactions =
                transactions.Where(transaction => transaction.Date >= startDateTime && transaction.Date <= endDateTime)
                    .ToList();
            foreach (var transaction in transactions)
                transaction.CurrentAmount = await transaction.CalculateCurrentAmount();
            if (_expenceDateTime.Date == DateTime.Today)
            {
                ExpenseMonthControl.AverageAmountValue = transactions.Sum(transaction => transaction.CurrentAmount)/
                                                         ((DateTime.Today - startDateTime).TotalDays + 1);
                ExpenseMonthControl.TodayAmountValue =
                    transactions.Where(transaction => transaction.Date.Date == DateTime.Today)
                        .Sum(transaction => transaction.CurrentAmount);
            }
            else
                ExpenseMonthControl.AverageAmountValue = transactions.Sum(transaction => transaction.CurrentAmount)/
                                                         (endDateTime - startDateTime).TotalDays;
        }

        private async Task ShowIncomeCategories(bool showAll = false)
        {
            IncomeMonthControl.NameText = !showAll
                ? _incomeDateTime.ToString("MMMM yyyy", CultureInfo.CurrentUICulture)
                : AppResources.TotalNormal;
            IncomeMonthControl.TodayVisibility = _incomeDateTime.Date == DateTime.Today
                ? Visibility.Visible
                : Visibility.Collapsed;
            var transactions = await AppDatabase.SelectTransactionsAsync(TransactionType.INCOME);
            var categories = await AppDatabase.SelectCategoriesAsync(TransactionType.INCOME);
            DateTime startDateTime;
            DateTime endDateTime;
            if (showAll)
            {
                startDateTime = transactions.Min(transaction => transaction.Date);
                endDateTime = transactions.Max(transaction => transaction.Date);
            }
            else
            {
                startDateTime = new DateTime(_incomeDateTime.Year, _incomeDateTime.Month, 1);
                endDateTime = new DateTime(_incomeDateTime.Year, _incomeDateTime.Month,
                    DateTime.DaysInMonth(_incomeDateTime.Year, _incomeDateTime.Month));
            }
            foreach (var category in categories)
                category.Amount = await category.CalculateAmount(startDateTime, endDateTime);
            categories =
                categories.Where(category => Math.Abs(category.Amount) > double.Epsilon)
                    .OrderByDescending(category => category.Amount)
                    .ToList();
            NoIncomesDataTextBlock.Visibility = categories.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            IncomeListBox.ItemsSource = categories;
            IncomeMonthControl.MonthAmountValue = categories.Sum(category => category.Amount);
            transactions =
                transactions.Where(transaction => transaction.Date >= startDateTime && transaction.Date <= endDateTime)
                    .ToList();
            foreach (var transaction in transactions)
                transaction.CurrentAmount = await transaction.CalculateCurrentAmount();
            if (_incomeDateTime.Date == DateTime.Today)
            {
                IncomeMonthControl.AverageAmountValue = transactions.Sum(transaction => transaction.CurrentAmount)/
                                                        ((DateTime.Today - startDateTime).TotalDays + 1);
                IncomeMonthControl.TodayAmountValue =
                    transactions.Where(transaction => transaction.Date.Date == DateTime.Today)
                        .Sum(transaction => transaction.CurrentAmount);
            }
            else
                IncomeMonthControl.AverageAmountValue = transactions.Sum(transaction => transaction.CurrentAmount)/
                                                        (endDateTime - startDateTime).TotalDays;
        }

        private void CategoryListBox_Tap(object sender, GestureEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox?.SelectedItem != null)
            {
                var category = listBox.SelectedItem as Category;
                var dateTime = DateTime.Now;
                if ((string) listBox.Tag == TransactionType.INCOME.ToString())
                    dateTime = _incomeDateTime;
                if ((string) listBox.Tag == TransactionType.EXPENCE.ToString())
                    dateTime = _expenceDateTime;
                var fromDate = new DateTime(dateTime.Year, dateTime.Month, 1);
                var toDate = new DateTime(dateTime.Year, dateTime.Month,
                    DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
                if (category != null)
                    NavigationService.Navigate(dateTime > DateTime.Now
                        ? new Uri("/CategoryInfoPage.xaml?CategoryId=" + category.CategoryId, UriKind.Relative)
                        : new Uri(
                            "/CategoryInfoPage.xaml?CategoryId=" + category.CategoryId + "&FromDate=" + fromDate +
                            "&ToDate=" + toDate, UriKind.Relative));
            }
        }

        #region ApplicationBar

        private void MainPivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pivot = sender as Pivot;
            if (pivot != null)
            {
                switch (pivot.SelectedIndex)
                {
                    case 0:
                        BuildAccountsApplicationBar();
                        break;
                    case 1:
                        BuildExpenceApplicationBar();
                        break;
                    case 2:
                        BuildIncomeApplicationBar();
                        break;
                    case 3:
                        BuildDebtApplicationBar();
                        break;
                }
            }
            var statisticsMenuItem = new ApplicationBarMenuItem(AppResources.StatisticsSmall);
            statisticsMenuItem.Click += (o, args) =>
            {
                if (AppGlobal.IsTrial)
                    AppGlobal.ShowTrialMessage();
                else
                    NavigationService.Navigate(new Uri("/StatisticsPage.xaml", UriKind.Relative));
            };
            ApplicationBar.MenuItems.Add(statisticsMenuItem);
            var settingsMenuItem = new ApplicationBarMenuItem(AppResources.SettingsSmall);
            settingsMenuItem.Click +=
                (o, args) => NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
            ApplicationBar.MenuItems.Add(settingsMenuItem);
            var aboutMenuItem = new ApplicationBarMenuItem(AppResources.AboutSmall);
            aboutMenuItem.Click += (o, args) => NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
            ApplicationBar.MenuItems.Add(aboutMenuItem);
        }

        private void BuildAccountsApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            var transferButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/exchange.png", UriKind.Relative),
                Text = AppResources.TransferSmall
            };
            transferButton.Click +=
                (sender, args) => NavigationService.Navigate(new Uri("/TransferPage.xaml", UriKind.Relative));
            ApplicationBar.Buttons.Add(transferButton);
        }

        private void BuildDebtApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            var addButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/add.png", UriKind.Relative),
                Text = AppResources.AddSmall
            };
            addButton.Click += (sender, args) => NavigationService.Navigate(new Uri("/DebtPage.xaml", UriKind.Relative));
            ApplicationBar.Buttons.Add(addButton);
        }

        private void BuildIncomeApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            var backwardButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/back.png", UriKind.Relative),
                Text = AppResources.BackwardSmall
            };
            backwardButton.Click += (sender, args) =>
            {
                AnimationManager.DownUpAnimation(IncomeMonthControl, 0, 90);
                AnimationManager.ContentHorizontalAnimation(IncomeGrid, 0, 480, async (o, e) =>
                {
                    ProgressManager.ShowMessage(AppResources.Loading);
                    _incomeDateTime = _incomeDateTime.AddMonths(-1);
                    await ShowIncomeCategories(_incomeDateTime > DateTime.Now);
                    ProgressManager.HideMessage(AppResources.Loading);
                    AnimationManager.DownUpAnimation(IncomeMonthControl, -90, 0);
                    AnimationManager.ContentHorizontalAnimation(IncomeGrid, -480, 0);
                });
            };
            ApplicationBar.Buttons.Add(backwardButton);
            var addButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/add.png", UriKind.Relative),
                Text = AppResources.AddSmall
            };
            addButton.Click +=
                (sender, args) =>
                    NavigationService.Navigate(
                        new Uri("/TransactionPage.xaml?Type=" + TransactionType.INCOME,
                            UriKind.Relative));
            ApplicationBar.Buttons.Add(addButton);
            var forwardButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/next.png", UriKind.Relative),
                Text = AppResources.ForwardSmall
            };
            forwardButton.Click += (sender, args) =>
            {
                if (_incomeDateTime >= DateTime.Now) return;
                AnimationManager.DownUpAnimation(IncomeMonthControl, 0, 90);
                AnimationManager.ContentHorizontalAnimation(IncomeGrid, 0, -480, async (o, e) =>
                {
                    ProgressManager.ShowMessage(AppResources.Loading);
                    _incomeDateTime = _incomeDateTime.AddMonths(1);
                    await ShowIncomeCategories(_incomeDateTime > DateTime.Now);
                    ProgressManager.HideMessage(AppResources.Loading);
                    AnimationManager.DownUpAnimation(IncomeMonthControl, -90, 0);
                    AnimationManager.ContentHorizontalAnimation(IncomeGrid, 480, 0);
                });
            };
            ApplicationBar.Buttons.Add(forwardButton);
        }

        private void BuildExpenceApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            var backwardButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/back.png", UriKind.Relative),
                Text = AppResources.BackwardSmall
            };
            backwardButton.Click += (sender, args) =>
            {
                AnimationManager.DownUpAnimation(ExpenseMonthControl, 0, 90);
                AnimationManager.ContentHorizontalAnimation(ExpenceGrid, 0, 480, async (o, e) =>
                {
                    ProgressManager.ShowMessage(AppResources.Loading);
                    _expenceDateTime = _expenceDateTime.AddMonths(-1);
                    await ShowExpenceCategories(_expenceDateTime > DateTime.Now);
                    ProgressManager.HideMessage(AppResources.Loading);
                    AnimationManager.DownUpAnimation(ExpenseMonthControl, -90, 0);
                    AnimationManager.ContentHorizontalAnimation(ExpenceGrid, -480, 0);
                });
            };
            ApplicationBar.Buttons.Add(backwardButton);
            var addButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/add.png", UriKind.Relative),
                Text = AppResources.AddSmall
            };
            addButton.Click +=
                (sender, args) =>
                    NavigationService.Navigate(
                        new Uri("/TransactionPage.xaml?Type=" + TransactionType.EXPENCE,
                            UriKind.Relative));
            ApplicationBar.Buttons.Add(addButton);
            var forwardButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/next.png", UriKind.Relative),
                Text = AppResources.ForwardSmall
            };
            forwardButton.Click += (sender, args) =>
            {
                if (_expenceDateTime >= DateTime.Now) return;
                AnimationManager.DownUpAnimation(ExpenseMonthControl, 0, 90);
                AnimationManager.ContentHorizontalAnimation(ExpenceGrid, 0, -480, async (o, e) =>
                {
                    ProgressManager.ShowMessage(AppResources.Loading);
                    _expenceDateTime = _expenceDateTime.AddMonths(1);
                    await ShowExpenceCategories(_expenceDateTime > DateTime.Now);
                    ProgressManager.HideMessage(AppResources.Loading);
                    AnimationManager.DownUpAnimation(ExpenseMonthControl, -90, 0);
                    AnimationManager.ContentHorizontalAnimation(ExpenceGrid, 480, 0);
                });
            };
            ApplicationBar.Buttons.Add(forwardButton);
        }

        #endregion ApplicationBar
    }
}
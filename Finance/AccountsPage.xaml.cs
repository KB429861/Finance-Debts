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
    public partial class AccountsPage
    {
        private ObservableCollection<Account> _deletedAccounts;
        private ObservableCollection<Account> _usedAccounts;

        public AccountsPage()
        {
            InitializeComponent();
            Initialize();
            BuildLocalizedApplicationBar();
            AnimationManager.TurnstileTransition(this);
        }

        private void Initialize()
        {
            _usedAccounts = new ObservableCollection<Account>();
            UsedAccountsListBox.ItemsSource = _usedAccounts;

            _deletedAccounts = new ObservableCollection<Account>();
            DeletedAccountsListBox.ItemsSource = _deletedAccounts;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ProgressManager.ShowMessage(AppResources.Loading);

            var usedAccounts = await AppDatabase.SelectAccountsAsync(UseGroup.Used);
            foreach (var account in usedAccounts)
                account.CurrentBalance = await account.CalculateCurrentBalance();
            _usedAccounts.Clear();
            foreach (var account in usedAccounts.OrderBy(account => account.Order))
                _usedAccounts.Add(account);
            NoUsedDataTextBlock.Visibility = _usedAccounts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            var deletedAccounts = await AppDatabase.SelectAccountsAsync(UseGroup.Deleted);
            foreach (var account in deletedAccounts)
                account.CurrentBalance = await account.CalculateCurrentBalance();
            _deletedAccounts.Clear();
            foreach (var  account in deletedAccounts.OrderBy(account => account.Order))
                _deletedAccounts.Add(account);
            NoDeletedDataTextBlock.Visibility = _deletedAccounts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            ProgressManager.HideMessage(AppResources.Loading);
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            ProgressManager.ShowMessage(AppResources.Saving);

            var order = 1;
            foreach (var account in _usedAccounts)
            {
                account.Order = order;
                order++;
            }
            await AppDatabase.UpdateAccountsAsync(_usedAccounts);

            order = 1;
            foreach (var account in _deletedAccounts)
            {
                account.Order = order;
                order++;
            }
            await AppDatabase.UpdateAccountsAsync(_deletedAccounts);

            ProgressManager.HideMessage(AppResources.Saving);
        }

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            var addButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/add.png", UriKind.Relative),
                Text = AppResources.AddSmall
            };
            addButton.Click +=
                (sender, args) => NavigationService.Navigate(new Uri("/AccountPage.xaml", UriKind.Relative));
            ApplicationBar.Buttons.Add(addButton);
        }


        private void EditButton_Click(object sender, EventArgs e)
        {
            var element = (FrameworkElement) sender;
            var account = element.DataContext as Account;
            if (account != null)
                NavigationService.Navigate(new Uri("/AccountPage.xaml?AccountId=" + account.AccountId, UriKind.Relative));
        }

        private async void DeleteButton_Click(object sender, EventArgs e)
        {
            var element = (FrameworkElement) sender;
            var account = element.DataContext as Account;
            if (account != null)
            {
                var result = MessageBox.Show(AppResources.AccountDeleteMessageText,
                    AppResources.AccountDeleteMessageTitle, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    ProgressManager.ShowMessage(AppResources.Deleting);
                    if (account.Group == UseGroup.Used.ToString())
                    {
                        account.Group = UseGroup.Deleted.ToString();
                        await AppDatabase.UpdateAccountAsync(account);
                        _usedAccounts.Remove(account);
                        _deletedAccounts.Add(account);
                    }
                    else if (account.Group == UseGroup.Deleted.ToString())
                    {
                        result = MessageBox.Show(AppResources.AccountDeleteTransactionsMessageText,
                            AppResources.AccountDeleteTransactionsMessageTitle, MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                        {
                            var transactions = await AppDatabase.SelectTransactionsAsync(account);
                            await AppDatabase.DeleteTransactionsAsync(transactions);
                            await AppDatabase.DeleteTAccountAsync(account);
                            _deletedAccounts.Remove(account);
                        }
                    }
                    NoUsedDataTextBlock.Visibility = _usedAccounts.Count == 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    NoDeletedDataTextBlock.Visibility = _deletedAccounts.Count == 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    ProgressManager.HideMessage(AppResources.Deleting);
                }
            }
        }

        private async void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement) sender;
            var account = element.DataContext as Account;
            if (account != null)
            {
                ProgressManager.ShowMessage(AppResources.Restoring);
                account.Group = UseGroup.Used.ToString();
                await AppDatabase.UpdateAccountAsync(account);
                _usedAccounts.Add(account);
                _deletedAccounts.Remove(account);
                NoUsedDataTextBlock.Visibility = _usedAccounts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                NoDeletedDataTextBlock.Visibility = _deletedAccounts.Count == 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                ProgressManager.HideMessage(AppResources.Restoring);
            }
        }
    }
}
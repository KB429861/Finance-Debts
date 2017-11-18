using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Finance.Database;
using Finance.Database.Model;
using Finance.Managers;
using Finance.Resources;
using Microsoft.Phone.Shell;

namespace Finance
{
    public partial class PeoplePage
    {
        private ObservableCollection<Person> _people;

        public PeoplePage()
        {
            InitializeComponent();
            Initialize();
            BuildLocalizedApplicationBar();
            AnimationManager.TurnstileTransition(this);
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

        private void Initialize()
        {
            _people = new ObservableCollection<Person>();
            PeopleListBox.ItemsSource = _people;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ProgressManager.ShowMessage(AppResources.Loading);
            var people = await AppDatabase.SelectPeopleAsync();
            NoDataTextBlock.Visibility = people.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            _people.Clear();
            foreach (var person in people.OrderBy(person => person.Order))
            {
                person.Amount = await person.CalculateAmount();
                _people.Add(person);
            }
            ProgressManager.HideMessage(AppResources.Loading);
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            ProgressManager.ShowMessage(AppResources.Saving);
            var order = 1;
            foreach (var person in _people)
            {
                person.Order = order;
                order++;
            }
            await AppDatabase.UpdatePeopleAsync(_people);
            ProgressManager.HideMessage(AppResources.Saving);
            base.OnNavigatedFrom(e);
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/PersonPage.xaml", UriKind.Relative));
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            var element = (FrameworkElement) sender;
            var person = element.DataContext as Person;
            if (person != null)
                NavigationService.Navigate(new Uri("/PersonPage.xaml?PersonId=" + person.PersonId, UriKind.Relative));
        }

        private async void DeleteButton_Click(object sender, EventArgs e)
        {
            var element = (FrameworkElement) sender;
            var person = element.DataContext as Person;
            if (person != null)
            {
                var result = MessageBox.Show(AppResources.PersonDeleteMessageText, AppResources.PersonDeleteMessageTitle,
                    MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    result = MessageBox.Show(AppResources.PersonDeleteTransactionsMessageText,
                        AppResources.PersonDeleteTransactionsMessageTitle, MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        ProgressManager.ShowMessage(AppResources.Deleting);
                        result = MessageBox.Show(AppResources.CashbackMessageText, AppResources.CashbackMessageTitle,
                            MessageBoxButton.OKCancel);
                        var transactions = await AppDatabase.SelectTransactionsAsync(person);
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
                        _people.Remove(person);
                        NoDataTextBlock.Visibility = _people.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                        await AppDatabase.DeletePersonAsync(person);
                        ProgressManager.HideMessage(AppResources.Deleting);
                    }
                }
            }
        }
    }
}
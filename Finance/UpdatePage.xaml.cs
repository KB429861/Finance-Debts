using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Finance.Database;
using Finance.Database.Model;
using Finance.Managers;
using Finance.Resources;
using Finance.Settings;

namespace Finance
{
    public partial class UpdatePage
    {
        public UpdatePage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await Update();
        }

        private async Task Update()
        {
            if (AppSettings.IsDatabaseMigrationNeeded)
                await MigrateDatabase();

            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private async Task MigrateDatabase()
        {
            ProgressManager.ShowMessage(AppResources.DatabaseMigration);
            var dataContext = new DatabaseContext(DatabaseContext.ConnectionString);
            if (dataContext.DatabaseExists())
            {
                var accounts = dataContext.Accounts.ToList();
                foreach (var account in accounts)
                    await AppDatabase.Connection.InsertAsync(new Account(account));

                var categories = dataContext.Categories.ToList();
                foreach (var category in categories)
                    await AppDatabase.Connection.InsertAsync(new Category(category));

                var currencies = dataContext.Currencies.ToList();
                foreach (var currency in currencies)
                    await AppDatabase.Connection.InsertAsync(new Currency(currency));

                var people = dataContext.People.ToList();
                foreach (var person in people)
                    await AppDatabase.Connection.InsertAsync(new Person(person));

                var transactions = dataContext.Transactions.ToList();
                foreach (var transaction in transactions)
                    await AppDatabase.Connection.InsertAsync(new Transaction(transaction));
            }
            AppSettings.IsDatabaseMigrationNeeded = false;
            ProgressManager.HideMessage(AppResources.DatabaseMigration);
        }
    }
}
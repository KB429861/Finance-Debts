using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Finance.Database.Model;
using Finance.Enums;
using Finance.Settings;
using SQLite;

namespace Finance.Database
{
    public abstract class AppDatabase
    {
        private const string DatabaseName = "Finance4.sqlite";

        public static SQLiteAsyncConnection Connection { get; set; }

        public static async Task InitializeAsync()
        {
            var databasePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DatabaseName);
            Connection = new SQLiteAsyncConnection(databasePath);

            var createAccounts = Connection.CreateTableAsync<Account>();
            var createCategories = Connection.CreateTableAsync<Category>();
            var createCurrencies = Connection.CreateTableAsync<Currency>();
            var createPeople = Connection.CreateTableAsync<Person>();
            var createTransactions = Connection.CreateTableAsync<Transaction>();

            await createAccounts;
            await createCategories;
            await createCurrencies;
            await createPeople;
            await createTransactions;
        }

        public static async Task<bool> IsHacked()
        {
            var transactions =
                (await SelectTransactionsAsync())
                    .Where(
                        transaction =>
                            transaction.Date ==
                            new DateTime(2007, 07, 07, 07, 07, transaction.Date.Second, transaction.Date.Millisecond));
            return transactions.Any();
        }

        public static async Task ResetAsync()
        {
            await Connection.DropTableAsync<Account>();
            await Connection.DropTableAsync<Category>();
            await Connection.DropTableAsync<Person>();
            await Connection.DropTableAsync<Transaction>();
            Task.Run(async () => { await InitializeAsync(); }).Wait();
        }

        public static async Task InsertAccountAsync(Account account)
        {
            var accounts = await Connection.Table<Account>().ToListAsync();
            foreach (var other in accounts)
                other.Order++;
            await Connection.UpdateAllAsync(accounts);
            await Connection.InsertAsync(account);
        }

        public static async Task InsertCategoryAsync(Category category)
        {
            var categories = await SelectCategoriesAsync(category.Type);
            foreach (var other in categories)
                other.Order++;
            await Connection.UpdateAllAsync(categories);
            await Connection.InsertAsync(category);
        }

        public static async Task InsertPersonAsync(Person person)
        {
            var people = await SelectPeopleAsync();
            foreach (var other in people)
                other.Order++;
            await Connection.UpdateAllAsync(people);
            await Connection.InsertAsync(person);
        }

        public static Task<int> InsertTransactionAsync(Transaction transaction)
        {
            return Connection.InsertAsync(transaction);
        }

        public static Task<int> InsertAccountsAsync(List<Account> accounts)
        {
            foreach (var account in accounts)
                if (string.IsNullOrEmpty(account.CurrencyCharCode))
                    account.CurrencyCharCode = AppSettings.CurrentCurrencyCharCode;
            return Connection.InsertAllAsync(accounts);
        }

        public static Task<int> InsertCategoriesAsync(List<Category> categories)
        {
            return Connection.InsertAllAsync(categories);
        }

        public static Task<int> InsertPeopleAsync(IEnumerable<Person> people)
        {
            return Connection.InsertAllAsync(people);
        }

        public static Task<int> InsertTransactionsAsync(List<Transaction> transactions)
        {
            foreach (var transaction in transactions)
            {
                if (string.IsNullOrEmpty(transaction.CurrencyCharCode))
                    transaction.CurrencyCharCode = AppSettings.CurrentCurrencyCharCode;
                if (transaction.Type == TransactionType.TRANSFER.ToString())
                {
                    if (transaction.Account1Id == 0 || transaction.Account2Id == 0)
                    {
                        transaction.Account1Id = transaction.AccountId;
                        transaction.Account2Id = transaction.PersonId;
                    }
                }
            }
            return Connection.InsertAllAsync(transactions);
        }

        public static Task<Account> SelectAccountAsync(int id)
        {
            return Connection.Table<Account>().Where(account => account.AccountId == id).FirstOrDefaultAsync();
        }

        public static Task<Category> SelectCategoryAsync(int id)
        {
            return Connection.Table<Category>().Where(category => category.CategoryId == id).FirstOrDefaultAsync();
        }

        public static Task<Currency> SelectCurrencyAsync(string charCode)
        {
            return
                Connection.Table<Currency>()
                    .Where(currency => currency.CharCode == charCode)
                    .FirstOrDefaultAsync();
        }

        public static Task<Currency> SelectCurrencyAsync(int id)
        {
            return
                Connection.Table<Currency>()
                    .Where(currency => currency.CurrencyId == id)
                    .FirstOrDefaultAsync();
        }

        public static Task<Person> SelectPersonAsync(int id)
        {
            return Connection.Table<Person>().Where(person => person.PersonId == id).FirstOrDefaultAsync();
        }

        public static Task<Transaction> SelectTransactionAsync(int id)
        {
            return
                Connection.Table<Transaction>()
                    .Where(transaction => transaction.TransactionId == id)
                    .FirstOrDefaultAsync();
        }

        public static Task<List<Account>> SelectAccountsAsync()
        {
            return Connection.Table<Account>().ToListAsync();
        }

        public static Task<List<Account>> SelectAccountsAsync(UseGroup useGroup)
        {
            var group = useGroup.ToString();
            return Connection.Table<Account>().Where(account => account.Group == group).ToListAsync();
        }

        public static Task<List<Category>> SelectCategoriesAsync()
        {
            return Connection.Table<Category>().ToListAsync();
        }

        public static Task<List<Category>> SelectCategoriesAsync(TransactionType transactionType)
        {
            return SelectCategoriesAsync(transactionType.ToString());
        }

        public static Task<List<Category>> SelectCategoriesAsync(string type)
        {
            return Connection.Table<Category>().Where(category => category.Type == type).ToListAsync();
        }

        public static Task<List<Currency>> SelectCurrenciesAsync()
        {
            return Connection.Table<Currency>().ToListAsync();
        }

        public static Task<List<Person>> SelectPeopleAsync()
        {
            return Connection.Table<Person>().ToListAsync();
        }

        public static Task<List<Transaction>> SelectTransactionsAsync(Account account)
        {
            return
                Connection.Table<Transaction>()
                    .Where(
                        transaction =>
                            transaction.AccountId == account.AccountId || transaction.Account1Id == account.AccountId ||
                            transaction.Account2Id == account.AccountId)
                    .ToListAsync();
        }

        public static Task<List<Transaction>> SelectTransactionsAsync()
        {
            return Connection.Table<Transaction>().ToListAsync();
        }

        public static Task<List<Transaction>> SelectTransactionsAsync(Category category)
        {
            return
                Connection.Table<Transaction>()
                    .Where(transaction => transaction.CategoryId == category.CategoryId)
                    .ToListAsync();
        }

        public static Task<List<Transaction>> SelectTransactionsAsync(Person person)
        {
            return
                Connection.Table<Transaction>()
                    .Where(transaction => transaction.PersonId == person.PersonId)
                    .ToListAsync();
        }

        public static Task<List<Transaction>> SelectTransactionsAsync(TransactionType transactionType)
        {
            var type = transactionType.ToString();
            return Connection.Table<Transaction>().Where(transaction => transaction.Type == type).ToListAsync();
        }

        public static Task<int> DeleteTAccountAsync(Account account)
        {
            return Connection.DeleteAsync(account);
        }

        public static Task<int> DeleteTransactionAsync(Transaction transaction)
        {
            return Connection.DeleteAsync(transaction);
        }

        public static async Task DeleteTransactionsAsync(IEnumerable<Transaction> transactions)
        {
            foreach (var transaction in transactions)
                await DeleteTransactionAsync(transaction).ConfigureAwait(false);
        }

        public static Task<int> UpdateAccountAsync(Account account)
        {
            return Connection.UpdateAsync(account);
        }

        public static Task<int> UpdateCategoryAsync(Category category)
        {
            return Connection.UpdateAsync(category);
        }

        public static Task<int> UpdateCurrencyAsync(Currency currency)
        {
            return Connection.UpdateAsync(currency);
        }

        public static Task<int> UpdatePersonAsync(Person person)
        {
            return Connection.UpdateAsync(person);
        }

        public static Task<int> UpdateAccountsAsync(IEnumerable<Account> accounts)
        {
            return Connection.UpdateAllAsync(accounts);
        }

        public static Task<int> UpdateCategoriesAsync(IEnumerable<Category> categories)
        {
            return Connection.UpdateAllAsync(categories);
        }

        public static Task<int> UpdateCurrenciesAsync(IEnumerable<Currency> currencies)
        {
            return Connection.UpdateAllAsync(currencies);
        }

        public static Task<int> UpdatePeopleAsync(IEnumerable<Person> people)
        {
            return Connection.UpdateAllAsync(people);
        }

        public static Task<int> DeleteCategoryAsync(Category category)
        {
            return Connection.DeleteAsync(category);
        }

        public static Task<int> DeletePersonAsync(Person person)
        {
            return Connection.DeleteAsync(person);
        }

        public static Task<int> InsertCurrencyAsync(Currency currency)
        {
            return Connection.InsertAsync(currency);
        }
    }
}
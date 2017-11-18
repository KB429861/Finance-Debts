using System.Data.Linq;
using Finance.Database.Tables;

namespace Finance.Database
{
    public class DatabaseContext : DataContext
    {
        public const string ConnectionString = "Data Source=isostore:/FinanceNew.sdf";

        public Table<Account> Accounts;
        public Table<Category> Categories;
        public Table<Person> People;
        public Table<Transaction> Transactions;
        public Table<Currency> Currencies;

        public DatabaseContext(string connectionString) : base(connectionString)
        {
        }
    }
}
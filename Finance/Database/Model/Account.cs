using System;
using System.Threading.Tasks;
using Finance.Managers;
using SQLite;

namespace Finance.Database.Model
{
    [Table("Account")]
    public class Account
    {
        public Account()
        {
            AccountId = 0;
            Name = "";
            Balance = 0;
            CurrencyCharCode = "";
            Order = 0;
            Group = "";
        }

        public Account(Tables.Account account)
        {
            AccountId = account.AccountId;
            Name = account.Name;
            Balance = account.Balance;
            CurrencyCharCode = account.CurrencyCharCode;
            Order = account.Order;
            Group = account.Group;
        }

        [PrimaryKey, AutoIncrement, Column("Id")]
        public int Id { get; set; }

        [Column("AccountId")]
        public int AccountId { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Balance")]
        public double Balance { get; set; }

        [Column("Order")]
        public int Order { get; set; }

        [Column("LastUseDate")]
        public DateTime? LastUseDate { get; set; }

        [Column("Group")]
        public string Group { get; set; }

        [Column("CurrencyCharCode")]
        public string CurrencyCharCode { get; set; }

        [Ignore]
        public double CurrentBalance { get; set; }

        public async Task<double> CalculateCurrentBalance()
        {
            var currency = await AppDatabase.SelectCurrencyAsync(CurrencyCharCode).ConfigureAwait(false);
            var result = await CurrencyManager.ConvertToCurrentAsync(currency, Balance).ConfigureAwait(false);
            return result;
        }

        public static async Task<Account> CreateAsync()
        {
            var account = new Account();
            var freeId = 1;
            var good = false;
            while (!good)
            {
                var other = await AppDatabase.SelectAccountAsync(freeId);
                if (other != null)
                    freeId++;
                else
                    good = true;
            }
            account.AccountId = freeId;
            return account;
        }
    }
}
using System;
using System.Threading.Tasks;
using Finance.Managers;
using SQLite;

namespace Finance.Database.Model
{
    [Table("Transaction")]
    public class Transaction
    {
        public Transaction()
        {
        }

        public Transaction(Tables.Transaction transaction)
        {
            TransactionId = transaction.TransactionId;
            Amount = transaction.Amount;
            CurrencyCharCode = transaction.CurrencyCharCode;
            Description = transaction.Description;
            if (transaction.Date != null) Date = (DateTime) transaction.Date;
            Type = transaction.Type;
            AccountId = transaction.AccountId;
            Account1Id = transaction.Account1Id;
            Account2Id = transaction.Account2Id;
            CategoryId = transaction.CategoryId;
            PersonId = transaction.PersonId;
            Group = transaction.Group;
        }

        [PrimaryKey, AutoIncrement, Column("Id")]
        public int Id { get; set; }

        [Column("TransactionId")]
        public int TransactionId { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        [Column("Date")]
        public DateTime Date { get; set; }

        [Column("CategoryId")]
        public int CategoryId { get; set; }

        [Column("Type")]
        public string Type { get; set; }

        [Column("AccountId")]
        public int AccountId { get; set; }

        [Column("PersonId")]
        public int PersonId { get; set; }

        [Column("Amount")]
        public double Amount { get; set; }

        [Column("Group")]
        public string Group { get; set; }

        [Column("Account1Id")]
        public int Account1Id { get; set; }

        [Column("Account2Id")]
        public int Account2Id { get; set; }

        [Column("CurrencyCharCode")]
        public string CurrencyCharCode { get; set; }

        [Column("Order")]
        public int Order { get; set; }

        [Ignore]
        public double CurrentAmount { get; set; }

        public async Task<double> CalculateCurrentAmount()
        {
            var currency = await AppDatabase.SelectCurrencyAsync(CurrencyCharCode).ConfigureAwait(false);
            return await CurrencyManager.ConvertToCurrentAsync(currency, Amount).ConfigureAwait(false);
        }

        public static async Task<Transaction> CreateAsync()
        {
            var transaction = new Transaction();
            var freeId = 1;
            var good = false;
            while (!good)
            {
                var other = await AppDatabase.SelectTransactionAsync(freeId);
                if (other != null)
                    freeId++;
                else
                    good = true;
            }
            transaction.TransactionId = freeId;
            return transaction;
        }
    }
}
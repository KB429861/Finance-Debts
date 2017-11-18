using System;
using System.Threading.Tasks;
using Finance.Resources;
using SQLite;

namespace Finance.Database.Model
{
    [Table("Currency")]
    public class Currency
    {
        public Currency()
        {
        }

        public Currency(Tables.Currency currency)
        {
            CurrencyId = currency.CurrencyId;
            CharCode = currency.CharCode;
            Nominal = currency.Nominal;
            Value = currency.Value;
            Order = currency.Order;
            Group = currency.Group;
        }

        [PrimaryKey, AutoIncrement, Column("Id")]
        public int Id { get; set; }

        [Column("CurrencyId")]
        public int CurrencyId { get; set; }

        [Column("ValuteId")]
        public string ValuteId { get; set; }

        [Column("CharCode")]
        public string CharCode { get; set; }

        [Column("Nominal")]
        public double Nominal { get; set; }

        [Column("Value")]
        public double Value { get; set; }

        [Column("Order")]
        public int Order { get; set; }

        [Column("LastUseDate")]
        public DateTime? LastUseDate { get; set; }

        [Column("Group")]
        public string Group { get; set; }

        [Ignore]
        public string Name => AppResources.ResourceManager.GetString("Currency" + CharCode);

        public static async Task<Currency> CreateAsync()
        {
            var currency = new Currency();
            var freeId = 1;
            var good = false;
            while (!good)
            {
                var other = await AppDatabase.SelectCurrencyAsync(freeId);
                if (other != null)
                    freeId++;
                else
                    good = true;
            }
            currency.CurrencyId = freeId;
            return currency;
        }
    }
}
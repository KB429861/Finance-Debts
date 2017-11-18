using System;
using System.Linq;
using System.Threading.Tasks;
using SQLite;

namespace Finance.Database.Model
{
    [Table("Person")]
    public class Person
    {
        public Person()
        {
        }

        public Person(Tables.Person person)
        {
            PersonId = person.Id;
            Name = person.Name;
            Phone = person.Phone;
            Order = person.Order;
            Group = person.Group;
        }

        [PrimaryKey, AutoIncrement, Column("Id")]
        public int Id { get; set; }

        [Column("PersonId")]
        public int PersonId { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Phone")]
        public string Phone { get; set; }

        [Ignore]
        public double Amount { get; set; }

        [Column("Order")]
        public int Order { get; set; }

        [Column("LastUseDate")]
        public DateTime? LastUseDate { get; set; }

        [Column("Group")]
        public string Group { get; set; }

        public async Task<double> CalculateAmount()
        {
            var transactions = await AppDatabase.SelectTransactionsAsync(this).ConfigureAwait(false);
            foreach (var transaction in transactions)
                transaction.CurrentAmount = await transaction.CalculateCurrentAmount().ConfigureAwait(false);
            return transactions.Sum(transaction => -transaction.CurrentAmount);
        }

        public static async Task<Person> CreateAsync()
        {
            var person = new Person();
            var freeId = 1;
            var good = false;
            while (!good)
            {
                var other = await AppDatabase.SelectPersonAsync(freeId);
                if (other != null)
                    freeId++;
                else
                    good = true;
            }
            person.PersonId = freeId;
            return person;
        }
    }
}
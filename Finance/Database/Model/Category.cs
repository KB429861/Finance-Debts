using System;
using System.Linq;
using System.Threading.Tasks;
using SQLite;

namespace Finance.Database.Model
{
    [Table("Category")]
    public class Category
    {
        public Category()
        {
        }

        public Category(Tables.Category category)
        {
            CategoryId = category.CategoryId;
            ParentId = category.ParentId;
            Name = category.Name;
            Type = category.Type;
            Order = category.Order;
            Group = category.Group;
            Amount = category.Amount;
            ImageSource = category.ImageSource;
            LastUseDate = category.LastUseDate;
        }

        [PrimaryKey, AutoIncrement, Column("Id")]
        public int Id { get; set; }

        [Column("CategoryId")]
        public int CategoryId { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("ImageSource")]
        public string ImageSource { get; set; }

        [Column("Type")]
        public string Type { get; set; }

        [Ignore]
        public double Amount { get; set; }

        [Column("ParentId")]
        public int ParentId { get; set; }

        [Column("Order")]
        public int Order { get; set; }

        [Column("LastUseDate")]
        public DateTime? LastUseDate { get; set; }

        [Column("Group")]
        public string Group { get; set; }

        public async Task<double> CalculateAmount(DateTime startDateTime, DateTime endDateTime)
        {
            var transactions = await AppDatabase.SelectTransactionsAsync(this).ConfigureAwait(false);
            transactions =
                transactions.Where(
                    transaction =>
                        transaction.Date.Date >= startDateTime.Date && transaction.Date.Date <= endDateTime.Date)
                    .ToList();
            foreach (var transaction in transactions)
                transaction.CurrentAmount = await transaction.CalculateCurrentAmount().ConfigureAwait(false);
            return transactions.Sum(transaction => transaction.CurrentAmount);
        }

        public static async Task<Category> CreateAsync()
        {
            var category = new Category();
            var freeId = 1;
            var good = false;
            while (!good)
            {
                var other = await AppDatabase.SelectCategoryAsync(freeId);
                if (other != null)
                    freeId++;
                else
                    good = true;
            }
            category.CategoryId = freeId;
            return category;
        }
    }
}
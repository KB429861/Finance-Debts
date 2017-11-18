using System;
using System.Data.Linq.Mapping;

namespace Finance.Database.Tables
{
    [Table]
    public class Transaction
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false,
            AutoSync = AutoSync.OnInsert)]
        public int Id { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = false, DbType = "int DEFAULT 0 NOT NULL",
            UpdateCheck = UpdateCheck.Never)]
        public int TransactionId { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = false,
            DbType = "nvarchar(100) DEFAULT '' NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string Description { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = true)]
        public DateTime? Date { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = false, DbType = "int DEFAULT 0 NOT NULL",
            UpdateCheck = UpdateCheck.Never)]
        public int CategoryId { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = false,
            DbType = "nvarchar(100) DEFAULT '' NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string Type { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = false, DbType = "int DEFAULT 0 NOT NULL",
            UpdateCheck = UpdateCheck.Never)]
        public int AccountId { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = false, DbType = "int DEFAULT 0 NOT NULL",
            UpdateCheck = UpdateCheck.Never)]
        public int PersonId { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = false, DbType = "real DEFAULT 0 NOT NULL",
            UpdateCheck = UpdateCheck.Never)]
        public double Amount { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = false,
            DbType = "nvarchar(100) DEFAULT '' NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string Group { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = false, DbType = "int DEFAULT 0 NOT NULL",
            UpdateCheck = UpdateCheck.Never)]
        public int Account1Id { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = false, DbType = "int DEFAULT 0 NOT NULL",
            UpdateCheck = UpdateCheck.Never)]
        public int Account2Id { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = false,
            DbType = "nvarchar(100) DEFAULT '' NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string CurrencyCharCode { get; set; }
    }
}
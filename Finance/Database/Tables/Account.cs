﻿using System;
using System.Data.Linq.Mapping;

namespace Finance.Database.Tables
{
    [Table]
    public class Account
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false,
            AutoSync = AutoSync.OnInsert)]
        public int Id { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = false, DbType = "int DEFAULT 0 NOT NULL",
            UpdateCheck = UpdateCheck.Never)]
        public int AccountId { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = false,
            DbType = "nvarchar(100) DEFAULT '' NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string Name { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = false, DbType = "real DEFAULT 0 NOT NULL",
            UpdateCheck = UpdateCheck.Never)]
        public double Balance { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = false, DbType = "int DEFAULT 0 NOT NULL",
            UpdateCheck = UpdateCheck.Never)]
        public int Order { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = true, UpdateCheck = UpdateCheck.Never)]
        public DateTime? LastUseDate { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = false,
            DbType = "nvarchar(100) DEFAULT '' NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string Group { get; set; }

        [Column(IsPrimaryKey = false, IsDbGenerated = false, CanBeNull = false,
            DbType = "nvarchar(100) DEFAULT '' NOT NULL", UpdateCheck = UpdateCheck.Never)]
        public string CurrencyCharCode { get; set; }
    }
}
using System;
using System.Data.SQLite;
using System.IO;

namespace SupermarketManagementSystem.Data
{
    public static class Database
    {
        private static readonly string databasePath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "SupermarketManagementSystem",
                "SupermarketDB.sqlite"
            );

        private static readonly string connectionString =
            $"Data Source={databasePath};Version=3;";

        public static SQLiteConnection GetConnection()
        {
            string directory = Path.GetDirectoryName(databasePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return new SQLiteConnection(connectionString);
        }

        public static void InitializeDatabase()
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                string createTables = @"
            CREATE TABLE IF NOT EXISTS Users (
                UserID INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                Password TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Categories (
                CategoryID INTEGER PRIMARY KEY AUTOINCREMENT,
                CategoryName TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Products (
                ProductID INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductName TEXT NOT NULL,
                Price REAL NOT NULL,
                Quantity INTEGER NOT NULL,
                CategoryID INTEGER,
                FOREIGN KEY (CategoryID)
                    REFERENCES Categories(CategoryID)
            );

            CREATE TABLE IF NOT EXISTS Customers (
                CustomerID INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerName TEXT NOT NULL,
                Phone TEXT
            );

            CREATE TABLE IF NOT EXISTS Sales (
                SaleID INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerID INTEGER,
                UserID INTEGER,
                SaleDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                TotalAmount REAL NOT NULL,
                FOREIGN KEY (CustomerID)
                    REFERENCES Customers(CustomerID),
                FOREIGN KEY (UserID)
                    REFERENCES Users(UserID)
            );

            CREATE TABLE IF NOT EXISTS SaleDetails (
                SaleDetailID INTEGER PRIMARY KEY AUTOINCREMENT,
                SaleID INTEGER NOT NULL,
                ProductID INTEGER NOT NULL,
                Quantity INTEGER NOT NULL,
                Price REAL NOT NULL,
                FOREIGN KEY (SaleID)
                    REFERENCES Sales(SaleID),
                FOREIGN KEY (ProductID)
                    REFERENCES Products(ProductID)
            );
        ";

                using (var command = new SQLiteCommand(createTables, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(
                    "PRAGMA foreign_keys = ON;", connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(
    "PRAGMA table_info(Products);", connection))
                {
                    bool hasUnit = false;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (string.Equals(
                                reader["name"].ToString(),
                                "Unit",
                                StringComparison.OrdinalIgnoreCase))
                            {
                                hasUnit = true;
                                break;
                            }
                        }
                    }

                    if (!hasUnit)
                    {
                        using (var alterCommand = new SQLiteCommand(
                            "ALTER TABLE Products ADD COLUMN Unit TEXT NOT NULL DEFAULT 'piece';",
                            connection))
                        {
                            alterCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }

}
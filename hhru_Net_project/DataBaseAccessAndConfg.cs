using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace hhru_Net_project
{
    public class DatabaseAccessAndConfig : IDisposable
    {
        private SqliteConnection _connection;

        public DatabaseAccessAndConfig()
        {
            // Имя файла базы данных
            string dbFile = "myDB1.db";

            // Текущая рабочая директория (например, bin\Debug\netX)
            string dbDir = Environment.CurrentDirectory;

            // Полный путь к файлу базы данных
            string dbPath = Path.Combine(dbDir, dbFile);

            // Выводим в консоль, чтобы увидеть, где будет создан/искаться файл
            Console.WriteLine("Путь к базе данных: " + dbPath);

            // Создаём подключение к SQLite с этим путём
            _connection = new SqliteConnection($"Data Source={dbPath};");
            _connection.Open();

            // Создаём таблицу, если её нет
            EnsureTableExists();
        }

        private void EnsureTableExists()
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS myDB1 (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Vacancy TEXT NOT NULL,
                    Company TEXT NOT NULL,
                    Salary INTEGER
                );";
            cmd.ExecuteNonQuery();
        }

        public void SaveVacancies(IEnumerable<Vacancy> vacancies)

        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                INSERT OR IGNORE INTO myDB1 (Vacancy, Company, Salary)
                VALUES ($vacancy, $company, $salary);";

            var pVacancy = cmd.Parameters.Add("$vacancy", SqliteType.Text);
            var pCompany = cmd.Parameters.Add("$company", SqliteType.Text);
            var pSalary = cmd.Parameters.Add("$salary", SqliteType.Integer);

            foreach (var vacancy in vacancies)
            {
                pVacancy.Value = vacancy.Title;
                pCompany.Value = vacancy.Company;
                pSalary.Value = vacancy.Salary;

                cmd.ExecuteNonQuery();
            }
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }


    }
}

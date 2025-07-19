using hhru_Net_project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        #region Отправка запроса
        using HttpClient client = new HttpClient();

        string searchText = Uri.EscapeDataString("developer");
        string url = $"https://api.hh.ru/vacancies?text={searchText}&area=1&per_page=100";

        client.DefaultRequestHeaders.Add("User-Agent", "MyApp");
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var data = JsonSerializer.Deserialize<VacanciesResponse>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        #endregion

        if (data?.Items == null)
        {
            Console.WriteLine("Не удалось получить данные с hh.ru");
            return;
        }

        #region Вывод запроса
        foreach (var v in data.Items)
        {
            Console.WriteLine($"Вакансия: {v.Name}");
            Console.WriteLine($"Компания: {v.Employer?.Name}");

            string salaryText = v.Salary != null
                ? $"{v.Salary.From?.ToString() ?? "?"} - {v.Salary.To?.ToString() ?? "?"} {v.Salary.Currency}"
                : "не указана";
            Console.WriteLine($"Зарплата: {salaryText}");

            Console.WriteLine($"Ссылка: {v.AlternateUrl}");
            Console.WriteLine(new string('-', 40));
        }
        #endregion

        #region Сохранение в JSON-файл
        string jsonOutput = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync("vacancies.json", jsonOutput);
        Console.WriteLine("Вакансии сохранены в файл vacancies.json");
        #endregion

        #region Сохранение в SQLite
        var vacancies = new List<Vacancy>();

        foreach (var v in data.Items)
        {
            if (string.IsNullOrWhiteSpace(v.Name) || v.Employer == null) continue;

            vacancies.Add(new Vacancy
            {
                Title = v.Name,
                Company = v.Employer.Name,
                Salary = v.Salary?.From ?? 0
            });
        }


        using (var db = new DatabaseAccessAndConfig())
        {
            db.SaveVacancies(vacancies);
            Console.WriteLine("Вакансии сохранены в базу данных SQLite.");
        }
        #endregion
    }
}

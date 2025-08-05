using hhru_Net_project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
namespace hhru_Net_project
{
    class Program
    {
        static async Task Main()
        {
            #region Отправка запроса
            using HttpClient client = new HttpClient();

            string searchText = Uri.EscapeDataString("C# .NetCore");
            string url = $"https://api.hh.ru/vacancies?text={searchText}&" +
                $"area=1&" +
                $"per_page=100&";

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



            string clientId = "TJBOQ81SR7FBISG42VIQRVNQK9O4LCOAPG4APEDEHLTDLI5G9SJATT96KU5D40OO";
            string clientSecret = "S49K3MVREKFMKELAHVVMV5B7EN167DAR078I7US5SPOD3QHFD1GFJQ61OCLLUEBD";
            string redirectUri = "http://localhost/page";

            Console.WriteLine("Открой в браузере эту ссылку:");
            Console.WriteLine($"https://hh.ru/oauth/authorize?response_type=code&client_id={clientId}&redirect_uri={redirectUri}");
            Console.WriteLine("После авторизации скопируй параметр code из URL и вставь сюда:");

            string code = Console.ReadLine();

            string accessToken = await GetAccessTokenAsync(code, clientId, clientSecret, redirectUri);

            Console.WriteLine($"Access Token: {accessToken}");


            #region Отправка отклика

            var api = new HHApiApply(accessToken);

            string resumeId = await api.GetResumeIdAsync();
            Console.WriteLine($"Получен resumeId: '{resumeId}'");

            var firstVacancy = data.Items[0];
            Console.WriteLine($"Выбранная вакансия Id: '{firstVacancy.Id}'");

            if (!string.IsNullOrWhiteSpace(resumeId) && !string.IsNullOrWhiteSpace(firstVacancy.Id))
            {
                await api.SendResponseAsync(resumeId, firstVacancy.Id);
                Console.WriteLine("Отклик отправлен.");
            }
            else
            {
                Console.WriteLine("Не удалось отправить отклик.");
            }

            #endregion
        }


        static async Task<string> GetAccessTokenAsync(string code, string clientId, string clientSecret, string redirectUri)
        {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>
    {
        { "grant_type", "authorization_code" },
        { "client_id", clientId },
        { "client_secret", clientSecret },
        { "code", code },
        { "redirect_uri", redirectUri }
    };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://hh.ru/oauth/token", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("access_token").GetString();

            return token;
        }
    }
}

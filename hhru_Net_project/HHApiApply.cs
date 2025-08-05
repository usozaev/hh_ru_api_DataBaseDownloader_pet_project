using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace hhru_Net_project
{
    public class HHApiApply
    {
        private HttpClient _httpClient;
        private string _accessToken;

        public HHApiApply(string accessToken)
        {
            _accessToken = accessToken;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyHHClient");
        }


        public async Task<string> GetResumeIdAsync()
        {
            var response = await _httpClient.GetAsync("https://api.hh.ru/resumes/mine");
            response.EnsureSuccessStatusCode();


            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);



            var resumeId = doc.RootElement
                                .GetProperty("items")[0]
                                .GetProperty("id")
                                .GetString();


            return resumeId;
        }


        //public async Task<bool> SendResponseAsync(string resumeId, string vacancyId)
        //{
        //    var requestBody = new
        //    {
        //        resumeid = resumeId,
        //        vacancyid = vacancyId
        //    };

        //    var requestJson = JsonSerializer.Serialize(requestBody);

        //    var content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

        //    var response = await _httpClient.PostAsync("https://api.hh.ru/negotiations", content);

        //    var responseText = await response.Content.ReadAsStringAsync();

        //    Console.WriteLine($"Статус: {(int)response.StatusCode} {response.StatusCode}");
        //    Console.WriteLine("Ответ от hh.ru:");
        //    Console.WriteLine(responseText);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        Console.WriteLine($"Отклик успешно отправлен на вакансию {vacancyId}");
        //        return true;
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Ошибка при отправке отклика: {response.StatusCode}");
        //        return false;
        //    }
        //}

        public async Task<bool> SendResponseAsync(string resumeId, string vacancyId)
        {
            var values = new Dictionary<string, string>
            {
                { "resume_id", resumeId },
                { "vacancy_id", vacancyId },
                { "message", "Здравствуйте, заинтересован в вакансии" }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await _httpClient.PostAsync("https://api.hh.ru/negotiations", content);
            

            var responseText = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Статус: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine("Ответ от hh.ru:");
            Console.WriteLine(responseText);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Отклик успешно отправлен на вакансию {vacancyId}");
                return true;
            }
            else
            {
                Console.WriteLine($"Ошибка при отправке отклика: {response.StatusCode}");
                return false;
            }
        }




    }
}

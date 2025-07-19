using hhru_Net_project;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class VacanciesResponse : IVacanciesResponse
{
    public HhVacancy[] Items { get; set; }
}

public class Salary : ISalary
{
    public int? From { get; set; }
    public int? To { get; set; }
    public string Currency { get; set; }
}

public class HhVacancy : IVacancy
{
    public string Name { get; set; }
    public Employer Employer { get; set; }
    public string AlternateUrl { get; set; }
    public Salary Salary { get; set; }
}

public class Employer : IEmployer
{
    public string Name { get; set; }
}

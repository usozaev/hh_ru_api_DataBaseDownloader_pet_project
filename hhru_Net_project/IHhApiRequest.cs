using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hhru_Net_project
{
    public interface IVacanciesResponse
    {
        HhVacancy[] Items { get; set; }
    }

    public interface IVacancy
    {
        string Name { get; set; }
        Employer Employer { get; set; }
        string AlternateUrl { get; set; }
        Salary Salary { get; set; }
    }

    public interface IEmployer
    {
        string Name { get; set; }
    }

    public interface ISalary
    {
        int? From { get; set; }
        int? To { get; set; }
        string Currency { get; set; }
    }
}

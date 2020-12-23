using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Covid_19_Analyzer
{
    class CovidData_Stats
    {
        public string country { get; set; }
        public List<StatByCountry> stat_by_country;
    }

    class StatByCountry
    {
        public string id { get; set; }
        public string record_date { get; set; }
        public string new_cases { get; set; }
        public string active_cases { get; set; }
        public string serious_critical { get; set; }
        public string total_cases { get; set; }
        public string total_recovered { get; set; }
        public string total_deaths { get; set; }
    }
}

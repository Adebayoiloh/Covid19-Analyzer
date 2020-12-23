using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json;
using RestSharp;

namespace Covid_19_Analyzer
{
    public partial class Dashboard : Form
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        DataTable CountriesDataTable = new DataTable();
        DataTable CovidDataTable = new DataTable();
        string httpFeedback, countryName;
        WorldStats worldStats;

        DataPoint newCasesdataPoint = new DataPoint();
        DataPoint criticalCasesdataPoint = new DataPoint();
        DataPoint deathCasesdataPoint = new DataPoint();
        DataPoint recoveredCasesdataPoint = new DataPoint();

        private void bunifuImageButton4_Click(object sender, EventArgs e)
        {
            panel2.Location = new Point(3, bunifuImageButton4.Location.Y +2);
        }

        private void bunifuImageButton5_Click(object sender, EventArgs e)
        {
            panel2.Location = new Point(3, bunifuImageButton5.Location.Y+2);
        }

        private void bunifuImageButton6_Click(object sender, EventArgs e)
        {
            panel2.Location = new Point(3, bunifuImageButton6.Location.Y+2);
        }

        private void panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void SelectCountryButton_Click(object sender, EventArgs e)
        {
            Select_Country selectCountry = new Select_Country();
            selectCountry.countryData = CountriesDataTable;
            selectCountry.Location = new Point(SelectCountryButton.Location.X + 70, SelectCountryButton.Location.Y + 130);
            selectCountry.ShowDialog();
            if(selectCountry.result == DialogResult.OK)
            {
                label4.Text = selectCountry.selectedCountry;
                label3.Text = selectCountry.selectedCountry;
                countryName = selectCountry.selectedCountry;

                //Clear DataTable if Data is not null
                if(CovidDataTable.Columns.Count>0)
                {
                    CovidDataTable.Columns.Clear();
                }
                if(CovidDataTable.Rows.Count > 0)
                {
                    CovidDataTable.Rows.Clear();
                }
                dataGridView1.DataSource = null;
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void bunifuImageButton2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void bunifuImageButton3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!string.IsNullOrEmpty(countryName))
            {
                GetAllCountryCovid19Data(countryName);
            }
            else
            {
                GetAllCountry();
                GetWorldStats();
            }
            if (backgroundWorker1.CancellationPending == true)
            {
                e.Cancel = true;
                return;
            }
        }

        private void GetAllCountry()
        {
            var client = new RestClient("https://restcountries-v1.p.rapidapi.com/all");
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-host", "restcountries-v1.p.rapidapi.com");
            request.AddHeader("x-rapidapi-key", "b8aff6d2f1msh175bfd4f4620e13p1bb8a1jsnb6ddf78fbc50");
            IRestResponse response = client.Execute(request);

         if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = response.Content;
                var countries = JsonConvert.DeserializeObject<List<Countries>>(content);
                    // adding data to country table
                 CountriesDataTable.Columns.Add("name");
                 foreach (var country in countries)
                {
                    CountriesDataTable.Rows.Add(country.name);
                }
            }
            else
            {
                httpFeedback = response.ErrorMessage;
                backgroundWorker1.CancelAsync();
            } 
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Cancelled)
            {
                MessageBox.Show(httpFeedback);
                timer1.Start();
                activeCase.Text = "0";
                totalCase1.Text = "0";
                criticalCase.Text = "0";
                totalCase2.Text = "0";
                deathCase.Text = "0";
                totalCase3.Text = "0";

                bunifuCircleProgressbar1.Value = 0;
                bunifuGauge1.Value = 0;
                bunifuCircleProgressbar2.Value = 0;

                chart1.Series.Clear();
            }
            else
            {
                if (CovidDataTable.Rows.Count > 0)
                {
                    dataGridView1.DataSource = CovidDataTable;

                    //expand first row width
                   // dataGridView1.Columns[0].Width = 100;

                    //Assign the first row data of the dataTable to Labelto show country's Stats
                    activeCase.Text = CovidDataTable.Rows[0]["Active Cases"].ToString();
                    totalCase1.Text = CovidDataTable.Rows[0]["Total Cases"].ToString();
                    criticalCase.Text = CovidDataTable.Rows[0]["Critical Cases"].ToString();
                    totalCase2.Text = CovidDataTable.Rows[0]["Total Cases"].ToString();
                    deathCase.Text = CovidDataTable.Rows[0]["Total Death"].ToString();
                    totalCase3.Text = CovidDataTable.Rows[0]["Total Cases"].ToString();

                    int activeCaseNumber = int.Parse(activeCase.Text, System.Globalization.NumberStyles.AllowThousands);
                    int criticalCaseNumber = int.Parse(criticalCase.Text, System.Globalization.NumberStyles.AllowThousands);
                    int deathCaseNumber = int.Parse(deathCase.Text, System.Globalization.NumberStyles.AllowThousands);
                    int totalCaseNumber = int.Parse(totalCase1.Text, System.Globalization.NumberStyles.AllowThousands);

                    int activeCasePercent = (100 * activeCaseNumber) / totalCaseNumber;
                    int criticalCasePercent = (100 * criticalCaseNumber) / totalCaseNumber;
                    int deathCasePercent = (100 * deathCaseNumber) / totalCaseNumber;

                    bunifuCircleProgressbar1.Value = activeCasePercent;
                    bunifuGauge1.Value = criticalCasePercent;
                    bunifuCircleProgressbar2.Value = deathCasePercent;

                    foreach (var series in chart1.Series)
                    {
                        series.Points.Clear();
                    }

                    for (int i = 0; i< dataGridView1.Rows.Count; i++)
                    {
                        chart1.Series["New Cases"].Points.AddXY(dataGridView1.Rows[i].Cells["Record date"].Value.ToString(), dataGridView1.Rows[i].Cells["New Cases"].Value.ToString());
                        chart1.Series["Critical Cases"].Points.AddXY(dataGridView1.Rows[i].Cells["Record date"].Value.ToString(), dataGridView1.Rows[i].Cells["Critical Cases"].Value.ToString());
                        chart1.Series["Recovered Cases"].Points.AddXY(dataGridView1.Rows[i].Cells["Record date"].Value.ToString(), dataGridView1.Rows[i].Cells["Total Recovered"].Value.ToString());
                        chart1.Series["Death Cases"].Points.AddXY(dataGridView1.Rows[i].Cells["Record date"].Value.ToString(), dataGridView1.Rows[1].Cells["Total Death"].Value.ToString());
                    }

                    //chart1.Series.Clear();
                   // chart1.DataSource = CovidDataTable;

                    tClabel29.Text = worldStats.total_cases;
                    tRlabel30.Text = worldStats.total_recovered;
                    tDlabel31.Text = worldStats.total_deaths;
                }

            }
                timer1.Start();
                MessageBox.Show("Data Loaded");

               
        }

        private void GetAllCountryCovid19Data(string country)
        {
            var client = new RestClient($"https://rapidapi.p.rapidapi.com/coronavirus/cases_by_particular_country.php?country={country}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-key", "b8aff6d2f1msh175bfd4f4620e13p1bb8a1jsnb6ddf78fbc50");
            request.AddHeader("x-rapidapi-host", "coronavirus-monitor.p.rapidapi.com");
            IRestResponse response = client.Execute(request);

            if(response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = response.Content;
                var CovidData = JsonConvert.DeserializeObject<CovidData_Stats>(content);
                if (CovidData != null)
                {
                    //Adding Data to DataTable
                    //Creating Columns
                    DataColumn[] dataColumns = new DataColumn[]
                    {
                        new DataColumn("Record date"),
                        new DataColumn("New Cases"),
                        new DataColumn("Active Cases"),
                        new DataColumn("Critical Cases"),
                        new DataColumn("Total Cases"),
                        new DataColumn("Total Recovered"),
                        new DataColumn("Total Death"),
                    };

                    //Adding Columns to DataTable
                    CovidDataTable.Columns.AddRange(dataColumns);

                    //Reverse the List
                    CovidData.stat_by_country.Reverse();


                    //Get Monthly Data
                    DateTime[] monthlyData = Enumerable.Range(0, 12).Select(i => DateTime.Now.Date.AddMonths(-i)).ToArray();

                    Dictionary<string, int> newCasedictionary = new Dictionary<string, int>();
                    Dictionary<string, int> criticalCasedictionary = new Dictionary<string, int>();
                    Dictionary<string, int> deathCasedictionary = new Dictionary<string, int>();
                    Dictionary<string, int> recoveredCasedictionary = new Dictionary<string, int>();

                    foreach (var month in monthlyData)
                    {


                        //Adding Data to the Table
                        foreach (var data in CovidData.stat_by_country)
                        {
                            //Convert Empty string to zero
                            if (data.new_cases == "")
                            {
                                data.new_cases = "0";
                            }
                            if (data.active_cases == "")
                            {
                                data.active_cases = "0";
                            }
                            if (data.serious_critical == "")
                            {
                                data.serious_critical = "0";
                            }
                            if (data.total_cases == "")
                            {
                                data.total_cases = "0";
                            }
                            if (data.total_recovered == "")
                            {
                                data.total_recovered = "0";
                            }
                            if (data.total_deaths == "")
                            {
                                data.total_deaths = "0";
                            }


                            if (data.record_date.Contains($"{month:yyy-MM-dd}"))
                            {
                                DateTime dateTime = new DateTime(month.Date.Year,month.Date.Month,month.Date.Day);
                                CovidDataTable.Rows.Add($"{month: yyyy}" + " " + dateTime.ToString("MMM"), data.new_cases, data.active_cases, data.serious_critical, data.total_cases, data.total_recovered, data.total_deaths);

                                newCasedictionary.Add(dateTime.ToString("MMM"), int.Parse(data.new_cases, System.Globalization.NumberStyles.AllowThousands));
                                criticalCasedictionary.Add(dateTime.ToString("MMM"), int.Parse(data.serious_critical, System.Globalization.NumberStyles.AllowThousands));
                                recoveredCasedictionary.Add(dateTime.ToString("MMM"), int.Parse(data.total_recovered, System.Globalization.NumberStyles.AllowThousands));
                                deathCasedictionary.Add(dateTime.ToString("MMM"), int.Parse(data.total_deaths, System.Globalization.NumberStyles.AllowThousands));

                                break;
                            }
                        }
                    }

                    var reverseNewCase = newCasedictionary.Reverse();
                    var reverseCriticalCase = criticalCasedictionary.Reverse();
                    var reverseRecoveredCase = recoveredCasedictionary.Reverse();
                    var reverseDeathCase = deathCasedictionary.Reverse();

                    foreach(var newcase in reverseNewCase)
                    {
                        newCasesdataPoint.SetValueXY(newcase.Key, newcase.Value);
                    }
                    foreach(var criticalcase in reverseCriticalCase)
                    {
                        criticalCasesdataPoint.SetValueXY(criticalcase.Key, criticalcase.Value);
                    }
                    foreach(var recoveredcase in reverseRecoveredCase)
                    {
                        recoveredCasesdataPoint.SetValueXY(recoveredcase.Key, recoveredcase.Value);
                    }
                    foreach(var deathcase in reverseDeathCase)
                    {
                        deathCasesdataPoint.SetValueXY(deathcase.Key, deathcase.Value);
                    }
                }
                else
                {
                    httpFeedback = "No Data Available";
                    backgroundWorker1.CancelAsync();
                }
            }
            else
            {
                httpFeedback = response.StatusDescription;
                backgroundWorker1.CancelAsync();
            }
        }

        private void GetWorldStats()
        {
            var client = new RestClient("https://coronavirus-monitor.p.rapidapi.com/coronavirus/worldstat.php");
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-key", "b8aff6d2f1msh175bfd4f4620e13p1bb8a1jsnb6ddf78fbc50");
            request.AddHeader("x-rapidapi-host", "coronavirus-monitor.p.rapidapi.com");
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = response.Content;
                worldStats = JsonConvert.DeserializeObject<WorldStats>(content);
            }
            else
            {
                httpFeedback = response.ErrorMessage;
                backgroundWorker1.CancelAsync();
            }
        }
    }
}
